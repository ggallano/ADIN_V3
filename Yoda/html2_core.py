
from __future__ import print_function

import re
import os
import time
import shutil
import errno
import math
import YodaVerilog as Verilog
import traceback
import tempfile
from YodaGenCL import *
from YodaHTML import *
from YodaInfo import *
from subprocess import call
from shutil import copyfile
import YodaLoader
import hashlib
from YodaDollarCurly import PerformDSPTextProcessing
import xml.etree.ElementTree as ET
from random import randint

GblRootHDoc = None
GblTOCOrder = []
GblMasterTOCNumber = {}

def MakeDir(path):
    try:
        os.makedirs(path)
    except OSError as exc:			# Python >2.5		# pragma: no cover
        if not ( exc.errno == errno.EEXIST and os.path.isdir(path) ):	# pragma: no cover
            raise							# pragma: no cover


Subdirectory = YodaGenCL.Option("HTML.Subdirectory","html2")
if len(Subdirectory) == 0:
    print("*E* the generator 'Subdirectory' option is blank.")
    exit(1)

def JobDir():
    if JobDir.dir == "":
        eng_name = TheModule.nameToUse()
        eng_name = re.sub("[^A-Za-z0-9_]","_",eng_name)
        JobDir.dir = os.path.join(YodaGenCL.OutputDir(), "design", Subdirectory, eng_name)
        try:
            shutil.rmtree(JobDir.dir)	# Out with the old.
        except Exception:
            pass
        MakeDir(JobDir.dir)
    return JobDir.dir
JobDir.dir = ""

YODA_GENTEST_ACTIVE = False
try:
    if os.environ['YODA_GENTEST_ACTIVE'] == '1':
        YODA_GENTEST_ACTIVE = True
except Exception:
    pass

def JobResourcesDir():
    if JobResourcesDir.dir == "":
        JobResourcesDir.dir = os.path.join( JobDir(), "resources" )
        MakeDir(JobResourcesDir.dir)
    return JobResourcesDir.dir
JobResourcesDir.dir = ""

GenSingleFile = False
if YodaGenCL.Option("HTML.GenSingleFile","False") == "True":
    GenSingleFile = True

GenImages = False
if YodaGenCL.Option("HTML.GenImages","False") == "True":
    GenImages = True

GenSimpleRegTable = True
if YodaGenCL.Option("HTML.RegTableStyle.Full","False") == "True":
    GenSimpleRegTable = False

GenHardwareContextOpt = True
if YodaGenCL.Option("HTML.HardSoftContext.Hardware","True") == "False":
    GenHardwareContextOpt = False

GblMaxRegPartWidth = YodaGenCL.Option("HTML.MaxRegPartWidth","16")

TheModule = None

# IntegerTypes = (types.IntType, types.LongType)
# StringTypes = (types.StringType, types.UnicodeType)

NotedImageNotFound = False

GlobalAllRegistersFlat = []
GlobalFinalHTMLPageBaseName = ""
GlobalFinalHTMLPageFullPathName = ""

class ErrorHandler():
    def report(self,iptr,msg):
        typ = iptr.objectType()
        if len(typ) == 0:
            typ = "NOTYPE"
        name = iptr.nameToUse()
        if len(name) == 0:
            name = "NONAME"
        uid = iptr.uid()
        uid8 = uid[len(uid)-8:] if len(uid) >= 8 else uid
        print("*E* %s %s(%s), %s" % (typ,name,uid8,msg))
EH = ErrorHandler()

def MakeSrcLink( fpn, item ):
    if len(fpn) > 0:
        fpn = fpn + "_"
    return "#{0}{1}:{2}".format(fpn,item.objectType(),item.uid()[-8:])

def MakeDstLink( fpn, item ):
    if len(fpn) > 0:
        fpn = fpn + "_"
    return "{0}{1}:{2}".format(fpn,item.objectType(),item.uid()[-8:])

def SetHardSoftContext( mod ):
    global GenHardwareContextOpt
    if GenHardwareContextOpt:
        mod.setHardwareContext()
    else:
        mod.setSoftwareContext()

def HSTypeIsToBeShown( field ):
    global GenHardwareContextOpt
    if GenHardwareContextOpt:
        return False if (field.rawHSType()=="Software") else True	# Hardware or Both.
    else:
        return False if (field.rawHSType()=="Hardware") else True	# Software or Both.

def DocumentationToUse( item ):
    if GenHardwareContextOpt:
        return item.engDocumentation();
    else:
        return item.swDocumentation();

def DescriptionToUse( item ):
    if GenHardwareContextOpt:
        desc = item.engDescription();
    else:
        desc = item.swDescription();
    if len(desc) > 0 and desc[-1] != "." and desc[-1] != "!" and desc[-1] != "?":
        desc += "."
    return desc

def DescDocCombo( item1, item2, full_path_name, error_handler ):
    desc = DescriptionToUse(item1); desc = PerformDSPTextProcessing( desc, item2, full_path_name, EH )
    desc = desc.strip();
    doc = DocumentationToUse(item1); doc = PerformDSPTextProcessing( doc,  item2, full_path_name, EH )
    doc = doc.strip();
    line = desc
    if len(line) > 0 and len(doc) > 0:
        line += " "
    line += doc
    #if len(line) == 0:
    #    line = "No documentation."
    line = re.sub("\n\n+","<br/>",line)
    return (desc,line)

def RegisterAccess( reg ):
    read = ""; write = ""
    for ref in reg.fieldRefs():
        field = ref.bitField()
        if not HSTypeIsToBeShown(field):
            continue
        if field.nameToUse().upper() == "RESERVED":
            pass	# No influence.
        else:
            a = field.rawAccess()
            if "R" in a:
                read = "R"
            if "W" in a:
                write = "W"
    sum = read
    if len(read) > 0 and len(write) > 0:
        sum += "/"
    sum += write
    if len(sum) == 0:
        sum = "R/W"	# pragma: no cover
    return sum


def VerilogAddress( addr ):
    addr = int(addr)
    str = Verilog.FormatNumber(addr,16,4,0)
    # str = str.replace("'h","0x")
    return str


# Only do this if otherwise all upper case.
#
def ProcessName( name ):
    name = name.replace("[","")
    name = name.replace("]","")
    return name


def AllReserved( reg ):
    all_res = True
    for ref in reg.fieldRefs():
        field = ref.bitField()
        if not HSTypeIsToBeShown(field):
            continue
        elif field.nameToUse() != "RESERVED":
            all_res = False
            break
    return all_res


def RegistersToDisplay( rmap ):
    list = [ ]
    for reg in rmap.registers():
        if not AllReserved(reg):
            list.append(reg)
    return list


""" SOME_LONG_LONG_LONG_NAME """
def break_long_name( r, limit ):
    len_r = len(r)
    if len_r > limit:
        s = ""
        line_length = 0
        for i in range(0,len_r):
            c = r[i]
            s = s + c
            line_length = line_length + 1
            if c == "_" and line_length > limit and i < len_r-2:
                s = s + "<br/>"
                line_length = 0
        # print("break_long_name:{0} -> {1}".format(r,s))
    else:
        s = r
    return s

def add_underscores( value, n=0 ):
    # print("add_underscores:",value,n)
    if "'" in value:
        parts = Verilog.Split(value)
        if n == 0:
            n = 4				# 4 for hex and binary.
            if "'o" in parts[1]: n = 3		# 3 for octal.
            elif "'d" in parts[1]: n = 0	# No underscores for decimal.
        # print("with n = ",n)
        value = parts[0] + parts[1] + add_underscores(parts[2],n)
    elif not "_" in value and n > 0:		# Leave any existing underscores alone.
        s = ""
        i = len(value)
        while i > n:
            for j in range(i-1,i-(n+1),-1):
                s += value[j]
            s += "_"
            i = i - n
        for j in range(i-1,-1,-1):
            s += value[j]
        # value = s[len(value)-1:-1:-1]		# Reverse order.
        value = s[::-1]		# Reverse order.
        value = value.upper()
    return value
def test_add_underscores(str,exp,n=0):
    if add_underscores(str,n) != exp: raise Exception("BIST Failed.")
test_add_underscores("","")
test_add_underscores("123456","123456")
test_add_underscores("'hF","'hF")
test_add_underscores("'hFF","'hFF")
test_add_underscores("'hFFF","'hFFF")
test_add_underscores("'hFFFF","'hFFFF")
test_add_underscores("'hFFFFF","'hF_FFFF")
test_add_underscores("'hFFFFFF","'hFF_FFFF")
test_add_underscores("'hFFFFFFF","'hFFF_FFFF")
test_add_underscores("'hFFFFFFFF","'hFFFF_FFFF")
test_add_underscores("'hFFFFFFFFF","'hF_FFFF_FFFF")
test_add_underscores("'hfffffffff","'hF_FFFF_FFFF")
test_add_underscores("'b111111","'b11_1111")
test_add_underscores("'o77777","'o77_777")
test_add_underscores("'d77777","'d77777")

def list_of_register_widths( regs ):
    list = [ ]
    for reg in regs:
        width = reg.intWidth()
        if not width in list:
            list.append(width)
    return list

def SimplifiedRegSummaryTable( rmap, parent, base_address, full_path_name ):
    regs = RegistersToDisplay(rmap)
    widths = list_of_register_widths(regs)
    if len(widths) == 1:
        HPara( parent=parent, text="All registers are {0} bits wide.".format(widths[0]))
    # HPara( parent=parent, text="Simplified register summary table.")
    tbl = HTable(parent=parent,style="Normal",border_width=2)
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Address"); header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    if len(widths) > 1:
        cell = HTableCell(text="Width"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Reset"); header.addCell(cell)
    cell = HTableCell(text="Access"); header.addCell(cell)
    for reg in regs:
        row = HTableRow(parent=tbl)

        reg_int_addr = base_address + reg.intAddress()
        reg_addr = reg_int_addr
        arr = reg.arraySpec()
        if arr.enabled():
            from_addr = VerilogAddress(reg_addr)
            to_addr = reg_addr + (arr.m_Size-1) * arr.m_Stride
            to_addr = VerilogAddress(to_addr)
            reg_addr = from_addr + " &#8211; <br/>" + to_addr
        else:
            reg_addr = VerilogAddress(reg_addr)
        cell = HTableCell(text=reg_addr,tt=True)
        row.addCell(cell)

        long_reg_name = reg.nameToUse()
        long_reg_name = ProcessName(long_reg_name)
        reg_name = break_long_name(long_reg_name,8)
        cell = HTableCell(parent=row,text=reg_name)
        cell.setLinkSrc(MakeSrcLink(full_path_name,reg))
        row.addCell(cell)

        if len(widths) > 1:
            width = reg.intWidth()
            cell = HTableCell(text=width)
            row.addCell(cell)

        desc = DescriptionToUse(reg)
        desc = PerformDSPTextProcessing( desc, reg, full_path_name, EH )
        #if len(desc) == 0:
        #    desc += "No description provided."
        cell = HTableCell(text=desc)
        row.addCell(cell)

        reset = reg.rawResetValue().lower()
        reset = Verilog.ConvertBase(reset,2,16)[0]
        reset = add_underscores(reset)
        cell = HTableCell(text=reset,tt=True)
        row.addCell(cell)

        access = RegisterAccess(reg)
        cell = HTableCell(text=access)
        row.addCell(cell)

        global GlobalFinalHTMLPageBaseName
        link = GlobalFinalHTMLPageBaseName + MakeSrcLink(full_path_name,reg)
        if arr.enabled():
            reg_name = reg.nameToUse()
            reg_name = break_long_name(reg_name,16)
            reg_name = (full_path_name+"_"+reg_name) if (len(full_path_name)>0) else reg_name
            names = arr.impliedNames(reg_name)
            addrs = arr.impliedAddresses(reg_int_addr)
            for name,addr in zip(names,addrs):
                GlobalAllRegistersFlat.append( (addr,VerilogAddress(addr),name,desc,link) )
        else:
            reg_name = break_long_name(long_reg_name,16)
            reg_name_to_use = (full_path_name+"_"+reg_name) if (len(full_path_name)>0) else reg_name
            GlobalAllRegistersFlat.append( (reg_int_addr,reg_addr,reg_name_to_use,desc,link) )


def WidestRegister( regs ):
    widest = 8
    for reg in regs:
        w = reg.intWidth()
        if w > widest:
            widest = w
    return widest


def FullRegSummaryTable( rmap, parent, base_address, full_path_name ):
    regs = RegistersToDisplay(rmap)
    widths = list_of_register_widths(regs)
    if len(widths) == 1:
        HPara( parent=parent, text="All registers are {0} bits wide.".format(widths[0]))
    # HPara( parent=parent, text="Full register summary table.")
    tbl = HTable(parent=parent,style="Normal",border_width=2)
    header = HTableRow(parent=tbl,header=True)
    provide_BITS_column = WidestRegister(regs) > 8
    cell = HTableCell(text="Addr"); header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    if provide_BITS_column:
        cell = HTableCell(text="Bits"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;7"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;6"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;5"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;4"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;3"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;2"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;1"); header.addCell(cell)
    cell = HTableCell(text="Bit&nbsp;0"); header.addCell(cell)
    cell = HTableCell(text="Reset"); header.addCell(cell)
    cell = HTableCell(text="Access"); header.addCell(cell)
    for reg in regs:
        # print("Next register: ",reg.nameToUse())

        bits_per_section = 8
        # +0.0 so math is floating point.
        num_lines = int( math.ceil( (reg.intWidth()+0.0) / bits_per_section ) )
        # 4: 3,2,1,0
        first_line = True
        for section_index in range(num_lines-1,-1,-1):
            section_msb = ((section_index+1)*bits_per_section)-1
            section_lsb = section_msb-(bits_per_section-1)
            # print("____Next section: index:{0}, msb:{1}, lsb:{2}".format(section_index,section_msb,section_lsb))

            row = HTableRow(parent=tbl)

            """ Address """
            if first_line:
                reg_int_addr = base_address + reg.intAddress()
                reg_addr = reg_int_addr
                arr = reg.arraySpec()
                if arr.enabled():
                    from_addr = VerilogAddress(reg_addr)
                    to_addr = reg_addr + (arr.m_Size-1) * arr.m_Stride
                    to_addr = VerilogAddress(to_addr)
                    reg_addr = from_addr + " &#8211; <br/>" + to_addr
                else:
                    reg_addr = VerilogAddress(reg_addr)
                cell = HTableCell(text=reg_addr,tt=True,rowspan=num_lines,vert_align="middle")
                row.addCell(cell)

            """ Name """
            if first_line:
                long_reg_name = reg.nameToUse()
                long_reg_name = ProcessName(long_reg_name)
                reg_name = break_long_name(long_reg_name,8)
                cell = HTableCell(parent=row,text=reg_name,rowspan=num_lines,vert_align="middle")
                cell.setLinkSrc(MakeSrcLink(full_path_name,reg));
                row.addCell(cell)

            if provide_BITS_column:
                """ Bits 1:15-8 0:7-0 """
                cell = HTableCell(text="[{0}:{1}]".format(section_msb,section_lsb),horiz_align="right",vert_align="middle")
                row.addCell(cell)

            for ref in reg.fieldRefs():
                field = ref.bitField()
                if not HSTypeIsToBeShown(field):
                    continue
                # print("_______Next ref: {0} reg-offset:{1}, slice-width:{2}".format(ref.nameToUse(),ref.intRegOffset(),ref.intSliceWidth()))
                reg_lsb = ref.intRegOffset()
                reg_msb = reg_lsb + ref.intSliceWidth() - 1
                # print("___________reg section [{0}:{1}] :".format(section_msb,section_lsb))
                # print("___________reg ref slice [{0}:{1}] :".format(regmsb,reg_lsb))
                if reg_lsb > section_msb or reg_msb < section_lsb:
                    None		# Out of this register sections range.
                else:
                    reg_msb_clipped = min(reg_msb,section_msb)
                    reg_lsb_clipped = max(reg_lsb,section_lsb)
                    fld_msb_clipped = reg_msb_clipped - ref.intRegOffset() + ref.intSliceOffset()
                    fld_lsb_clipped = reg_lsb_clipped - ref.intRegOffset() + ref.intSliceOffset()
                    colspan = reg_msb_clipped - reg_lsb_clipped + 1
                    # print("___________Covers part or all of this section. reg_msb_clipped:{0}, reg_lsb_clipped:{1}".format(reg_msb_clipped,reg_lsb_clipped))
                    name = field.nameToUse()
                    name = break_long_name(name,colspan*9)
                    if colspan < field.intWidth() and name != "RESERVED":
                        # print("___________Section only contains part of the field. Tell them which part.")
                        if colspan == 1:
                            # print("_____________Contained section is only one bit wide.")
                            name = name + "[{0}]".format(fld_lsb_clipped)
                        else:
                            # print("_____________Contained section is more than one bit wide.")
                            name = name + "[{0}:{1}]".format(fld_msb_clipped,fld_lsb_clipped)
                    # print("___________Name to use: {0}".format(name))
                    if name == "RESERVED":
                        bgcolor = "#EEEEEE"
                    else:
                        bgcolor = ""
                    cell = HTableCell(text=name,colspan=colspan,horiz_align="center",bgcolor=bgcolor,vert_align="middle")
                    row.addCell(cell)

            if first_line:
                reset = reg.rawResetValue().lower()
                reset = Verilog.ConvertBase(reset,2,16)[0]
                reset = add_underscores(reset)
                cell = HTableCell(text=reset,tt=True,rowspan=num_lines,vert_align="middle")
                row.addCell(cell)

            if first_line:
                access = RegisterAccess(reg)
                cell = HTableCell(text=access,rowspan=num_lines,vert_align="middle")
                row.addCell(cell)

            if first_line:
                desc = DescriptionToUse(reg)
                desc = PerformDSPTextProcessing( desc, reg, full_path_name, EH )

                global GlobalFinalHTMLPageBaseName
                link = GlobalFinalHTMLPageBaseName + MakeSrcLink(full_path_name,reg)
                if arr.enabled():
                    reg_name = reg.nameToUse()
                    reg_name = break_long_name(reg_name,16)
                    reg_name = (full_path_name+"_"+reg_name) if (len(full_path_name)>0) else reg_name
                    names = arr.impliedNames(reg_name)
                    addrs = arr.impliedAddresses(reg_int_addr)
                    for name,addr in zip(names,addrs):
                        GlobalAllRegistersFlat.append( (addr,VerilogAddress(addr),name,desc,link) )
                else:
                    reg_name = break_long_name(long_reg_name,16)
                    reg_name_to_use = (full_path_name+"_"+reg_name) if (len(full_path_name)>0) else reg_name
                    GlobalAllRegistersFlat.append( (reg_int_addr,reg_addr,reg_name_to_use,desc,link) )

            first_line = False

GblMapRawReadToDirName = { }
def ModuleImageDirectory( mod ):
    global GblMapRawReadToDirName
    dir_name = ""
    raw_read = mod.rawReadFileName()
    if raw_read in GblMapRawReadToDirName:
        dir_name = GblMapRawReadToDirName[raw_read]
    else:
        n = len(GblMapRawReadToDirName)
        if n == 0:
            dir_name = mod.nameToUse()
        else:
            dir_name = "{0}_{1}".format(mod.nameToUse(),n)
        dir_name = re.sub("[^A-Za-z0-9_]","_",dir_name)
        GblMapRawReadToDirName[raw_read] = dir_name
    return dir_name

def RelativeImageDirectory(mod):
    sub = ModuleImageDirectory(mod)
    return "images/{0}".format(sub)

def FullPathImageDirectory(mod):
    sub = ModuleImageDirectory(mod)
    return "{0}/images/{1}".format(JobResourcesDir(),sub)

def FullPathImageFileName(mod,uid):
    return "{0}/{1}.png".format(FullPathImageDirectory(mod),uid)

def RelativeImageFileName(mod,uid):
    return "{0}/{1}.png".format(RelativeImageDirectory(mod),uid)

created_images_for_module = { }
def CreateImages( mod ):
    global created_images_for_module
    src_file = mod.rawReadFileName()
    if not src_file in created_images_for_module:
        full_out_dir = FullPathImageDirectory(mod)
        MakeDir(full_out_dir)
        out_file = "{0}/$UID.png".format(full_out_dir)
        cmd = [ YodaInfo.Executable(),
                "-doc-set", "DS1",
                "-image",
                "-object", "__ALL__",
                "-im-expose-private-bitfields", "False",
                "-im-include-bf-enums", "False",
                "-im-max-reg-part-width", GblMaxRegPartWidth,
                "-im-label-reserved-bitfields", "False",
                # "-im-scale-factor", "2",
                "-im-td-scale-factor", "1.5",
                "-im-pkg-scale-factor", "2",
                "-software",
                "-outfile", out_file,
                src_file
              ]
        #if YodaInfo.TestMode():
        #    print("CreatingImages:",cmd)
        with open(os.devnull,'w') as dev_null:
            # output = call( cmd, shell=False, stdout=dev_null, stderr=dev_null )
            call( cmd, shell=False, stdout=dev_null, stderr=dev_null )
        created_images_for_module[src_file] = True


def RegisterImage( reg, parent ):
    CreateImages( reg.parentModule() )				# pragma: no cover
    file_name = FullPathImageFileName(reg.parentModule(),reg.uid())
    if os.path.isfile(file_name):
        file_name = RelativeImageFileName(reg.parentModule(),reg.uid())
        HImage( parent=parent, src=file_name )
    else:
        global NotedImageNotFound
        if not NotedImageNotFound:
            print("*W* could not access image file for register:",reg.nameToUse(),"(Last notice.)")
            NotedImageNotFound = True


def TimingDiagramImage( td, parent ):
    CreateImages( td.parentModule() )				# pragma: no cover
    file_name = FullPathImageFileName(td.parentModule(),td.uid())
    if os.path.isfile(file_name):
        file_name = RelativeImageFileName(td.parentModule(),td.uid())
        HImage( parent=parent, src=file_name )


def ObjectImage( obj, parent ):
    CreateImages( obj.parentModule() )				# pragma: no cover
    file_name = FullPathImageFileName(obj.parentModule(),obj.uid())
    if os.path.isfile(file_name):
        file_name = RelativeImageFileName(obj.parentModule(),obj.uid())
        HImage( parent=parent, src=file_name )


def RegisterBits( ref, fld ):
    if ref.intSliceWidth() == 1:
        return ref.intRegOffset()
    else:
        return "{0}:{1}".format(ref.intRegOffset()+ref.intSliceWidth()-1,ref.intRegOffset())


def using_page_masks( refs ):
    using = False
    for ref in refs:
        field = ref.bitField()
        if field.isHSTypeSoftware():
            continue				# This gen is currently a pure HW context.
        if len(field.rawPageMask()) > 0:
            using = True
            break
    return using

# Copy to resources dir and share duplicates.
#
def InternImageFile( file_name ):
    # print("InternImageFile:",file_name)
    # print("JobResourcesDir:",JobResourcesDir())
    mod_file = TheModule.rawYDAFileName()
    mod_dir = os.path.dirname(mod_file)
    fpn = os.path.join(mod_dir,file_name)
    # print("Module file:",mod_file)
    # print("Module dir:",mod_dir)
    # print("FPN:",fpn)
    md5_digest = "abc"
    with open(fpn,'rb') as fd:
        contents = fd.read()
        m = hashlib.md5()
        m.update(contents)
        md5_digest = m.hexdigest()
    # print("md5:",md5_digest)
    cache_file = os.path.join(JobResourcesDir(),md5_digest) + ".dat"
    # print("cache file:",cache_file)
    if not os.path.isfile(cache_file):
        copyfile( fpn, cache_file )
    return md5_digest + ".dat"

def FetchImage( eqn, size, out_file ):
    in_file = os.path.join(JobResourcesDir(),"eqn{0}.dat".format(randint(1000,9999)))
    try:
        with open(in_file,'w') as fd:
            print( eqn.rstrip(), file=fd )
        cmd = [ YodaInfo.Executable(), "-eqn1", in_file, out_file, size, "red" ]
        # print("html2:",cmd)
        dev_null = open(os.devnull,'w')
        call( cmd, shell=False, stdout=dev_null, stderr=dev_null )
        dev_null.close()
    finally:
        os.remove(in_file)

def InternEquationFile( eqn, size ):
    m = hashlib.md5()
    m.update(eqn.encode("utf-8"))
    m.update(size.encode("utf-8"))
    md5_digest = m.hexdigest()
    cache_file = os.path.join(JobResourcesDir(),md5_digest) + ".dat"
    if not os.path.isfile(cache_file):
        FetchImage( eqn, size, cache_file )
    return md5_digest + ".dat"

def XMLify( text ):
    text = re.sub('&','&amp;',text)
    text = re.sub('<','&lt;',text)
    text = re.sub('>','&gt;',text)
    text = re.sub('"','&quot;',text)
    text = re.sub(r'\u2028','<br/>',text)		# Browser does not appear to honor the line-separator char.
    return text

# Add paragraph justification (halign) and image processing.
#
empty_paragraph = True
def ConvertToHtml(parent,subject_item,error_handler,current_list_depth,current_indent,full_path_name):
    global empty_paragraph
    html = ""
    if parent.tag == "para":
        indent = int(parent.get("indent",default=0))
        halign = parent.get("halign",default="left")
        # m = int(parent.get("indent",default=0))*25
        m = 0
        if current_list_depth > 0:
            m = 25 - current_list_depth*16 - current_list_depth*25 + indent*25
        else:
            m = indent*25
        if current_list_depth > 1:
            m = m - 20
        if m == 0 and halign == "left":
            html = html + "\n<p>\n"
        else:
            html = html + "\n<p style=\"margin-left: "+str(m)+"px ; padding-left: 0px; text-align: "+ halign +";\">\n"
        empty_paragraph = True
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent,full_path_name)
        if empty_paragraph:
            html = html + "<br/>"		# Empty paragraph shows a blank line in Yoda.
        html = html + "</p>"
    elif parent.tag == "td":
        html = html + "\n<td>\n"
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent,full_path_name)
        html = html + "\n</td>\n"
    elif parent.tag == "tr":
        html = html + "\n<tr>\n"
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent,full_path_name)
        html = html + "\n</tr>\n"
    elif parent.tag == "tbl":
        width = parent.get("width",default="90%")
        html = html + "\n<table class=\"BML\" bordercolor=\"red\" width=\"{0}\" align=\"center\">\n".format(width)
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent,full_path_name)
        html = html + "\n</table>\n"
    elif parent.tag == "list":
        empty_paragraph = False
        style = parent.get("list-style",default="decimal")
        if style == "decimal":
            html = html + "\n<ol>\n"
        else:
            # html = html + "\n<ul style=\"list-style-type:disk\">\n"
            html = html + "\n<ul>\n"
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth+1,current_indent,full_path_name)
        if style == "decimal":
            html = html + "</ol>"
        else:
            html = html + "</ul>"
    elif parent.tag == "list-item":
        empty_paragraph = False
        indent = int(parent.get("indent",default=current_indent+1))
        # m = indent*15 - current_list_depth*15
        m = 25 - current_list_depth*16 - current_list_depth*25 + indent*25
        html = html + "\n<li style=\"margin-left: "+str(m)+"px ; padding-left: 0px;\">\n"
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent+indent,full_path_name)
        html = html + "</li>"
    elif parent.tag == "text":
        empty_paragraph = False
        span = ""
        weight = parent.get("font-weight",default=None)
        if not weight is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "font-weight:" + weight + ";"
        style = parent.get("font-style",default=None)
        if not style is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "font-style:" + style + ";"
        size = parent.get("font-size",default=None)
        if not size is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "font-size:" + size + ";"
        family = parent.get("font-family",default=None)
        if not family is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "font-family:" + family + ";"
        overline = parent.get("font-overline",default=None)
        if not overline is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "text-decoration:overline;"
        color = parent.get("color",default=None)
        if not color is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "color:" + color + ";"
        bgcolor = parent.get("background-color",default=None)
        if not bgcolor is None:
            if len(span) == 0: span = "<span style=\""
            span = span + "background-color:" + bgcolor + ";"
        html = html + span
        if len(span) > 0:
            html = html + "\">"
        text = parent.text
        text = XMLify( text )
        html = html + text
        if len(span) > 0:
            html = html + "</span>"
    elif parent.tag == "macro":
        empty_paragraph = False
        for child in parent:
            html = html + ConvertToHtml(child,subject_item,error_handler,current_list_depth,current_indent,full_path_name)
    elif parent.tag == "image":
        empty_paragraph = False
        file_name = ""
        width = ""
        height = 0
        for child in parent:
            if child.tag == "image-file-name": file_name = child.text
            elif child.tag == "image-scaled-width": width = child.text
            elif child.tag == "image-scaled-height": height = child.text
        resource = InternImageFile(file_name)
        # print(file_name,width,height)
        if len(width) > 0:
            html += '<img src="{0}" alt="Image not found." width="{1}" height="{2}"/>'.format(resource,width,height)
        else:
            html += '<img src="{0}" alt="Image not found."/>'.format(resource)
    elif parent.tag == "eqn":
        empty_paragraph = False
        eqn = parent.text
        font_size = parent.get("font-size",default="12pt")
        if len(eqn) > 0:
            resource = InternEquationFile(eqn,font_size)
            html += '<img src="{0}" alt="Image not found." style="vertical-align:baseline;"/>'.format(resource)
            # html += '<img src="{0}" alt="Image not found." style="vertical-align:middle;"/>'.format(resource)
    # elif parent.tag == "spec-last-value":		# Last macro value: MOD_REG.FIELD
        # html = html + parent.text
    elif parent.tag == "macro-spec":			# Specified macro value: ${.:.:.}
        empty_paragraph = False
        # html = html + parent.text
        # print("PerformDSPTextProcessing: ",parent.text,subject_item)
        html = html + PerformDSPTextProcessing( parent.text, subject_item, full_path_name, error_handler )
    return html

def ConvertBMLToHTML( bml, subject_item, full_path_name, error_handler ):
    try:
        html = ""
        root = ET.fromstring(bml)
        for child in root:
            html = html + ConvertToHtml(child,subject_item,error_handler,0,0,full_path_name) + "\n"
        return html;
    except ET.ParseError as err:
        raise ImportError(err)


def add_user( obj, parent, subject_item, full_path_name, error_handler ):
    user = obj.rawUser()
    if user and (len(user) > 0) and (not "yoda:WriteEnableBit" in user):
        user = re.sub("\n+","<br/>",user)
        user = "User: " + user
        div = HSection( parent=parent, style="User" )
        HPara( parent=div, text=user )


def add_notes( obj, parent, subject_item, full_path_name, error_handler ):
    notes = obj.rawNotes()
    # print(">>>"+notes)
    if notes and len(notes) > 0:
        # None of :this: is announced or officially supported.
        if notes.startswith(":pre:"):
            notes = notes[5:]
            notes = PerformDSPTextProcessing( notes, obj, full_path_name, EH, html=False )
            notes = "<pre>" + notes + "</pre>"
        elif notes.startswith(":html:"):
            notes = notes[6:]
            notes = PerformDSPTextProcessing( notes, obj, full_path_name, EH, html=True )
            """User is responsible for any pre."""
        elif notes.startswith(":plain:"):
            notes = notes[7:]
            """No processing."""
            notes = "<pre>" + notes + "</pre>"
        elif notes.startswith(":dsp:"):
            notes = notes[6:]
            notes = PerformDSPTextProcessing( notes, obj, full_path_name, EH, html=False )
        elif notes.strip().startswith("<bml"):
            notes = ConvertBMLToHTML(notes,subject_item,full_path_name,error_handler)
            notes = "<span style=\"color:red\">" + notes + "</span>"	# Notes are colored red.
            HText(parent=parent,text=notes)
            return
        else:
            notes = PerformDSPTextProcessing( notes, obj, full_path_name, EH, html=False )
        notes = re.sub("\n\n+","<br/>",notes)
        div = HSection( parent=parent, style="Notes" )
        HPara( parent=div, text=notes )

    add_user( obj, parent, subject_item, full_path_name, error_handler )


def MyFieldAccess(field):
    acc = field.fancyAccess()
    # if "yoda:WriteEnableBit" in field.rawUser():
    if field.isAutoWriteEnableBit():
        acc = "R0/W1E"
    return acc

def RegisterDetailTable( reg, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal",border_width=2)

    pm_column = using_page_masks(reg.fieldRefs())

    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Bits");  header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Reset"); header.addCell(cell)
    cell = HTableCell(text="Access"); header.addCell(cell)
    if pm_column:
        cell = HTableCell(text="Page<br/>Mask"); header.addCell(cell)
    cell = HTableCell(text="Visibility"); header.addCell(cell)

    for ref in reg.fieldRefs():
        field = ref.bitField()
        if not HSTypeIsToBeShown(field):
            continue
        row = HTableRow(parent=tbl)
        cell = HTableCell(text=RegisterBits(ref,field)); row.addCell(cell)

        reset = field.rawDefaultValue()
        field_name = field.nameToUse()
        field_width = field.intWidth()
        slice_width = ref.intSliceWidth()
        if slice_width < field_width:
            slice_offset = ref.intSliceOffset()
            if slice_width == 1:
                field_name = "%s[%s]" % (field_name,slice_offset)
                if reset == "Unspecified":
                    pass
                else:
                    reset = "'h" + Verilog.BitRange(reset,16,slice_offset,slice_offset)
            else:
                msb = slice_offset+slice_width-1
                lsb = slice_offset
                field_name = "%s[%s:%s]" % (field_name,msb,lsb)
                if reset == "Unspecified":
                    pass
                else:
                    reset = "'h" + Verilog.BitRange(reset,16,msb,lsb)
        cell = HTableCell(text=field_name); row.addCell(cell)

        (desc,line) = DescDocCombo( field, ref, full_path_name, EH )

        cell = HTableCell(); row.addCell(cell)
        HText(parent=cell,text=line)

        enums = field.enumerations()
        if len(enums) > 0:
            enum_tbl = HTable(parent=cell,style="Enum",border_width=0)
            for enum in enums:
                enum_row = HTableRow(parent=enum_tbl,header=False)
                value = enum.value()
                (desc,line) = DescDocCombo( enum, ref, full_path_name, EH )

                enum_cell = HTableCell(); enum_row.addCell(enum_cell)
                HText(parent=enum_cell,text=value)

                enum_cell = HTableCell(); enum_row.addCell(enum_cell)
                HText(parent=enum_cell,text=":")

                enum_cell = HTableCell(); enum_row.addCell(enum_cell)
                HText(parent=enum_cell,text=line)

                add_notes( enum, enum_cell, ref, full_path_name, EH )

        add_notes( field, cell, ref, full_path_name, EH )

        if field.hasForcedValue():
            forced = "Forced value: {0}, signal: {1}" .format(field.rawForcedValue(),field.rawForcedValueSignal())
            div = HSection( parent=cell, style="Notes" )
            HPara( parent=div, text=forced )

        reset = add_underscores(reset)
        cell = HTableCell(text=reset,tt=True)
        row.addCell(cell)

        cell = HTableCell(text=MyFieldAccess(field))
        row.addCell(cell)

        if pm_column:
            cell = HTableCell(text=field.rawPageMask())
            cell.setLinkSrc("#PAGE_MASK_TABLE")
            row.addCell(cell)

        cell = HTableCell(text=field.rawVisibility())
        row.addCell(cell)

    return tbl


def RegDetail( reg, parent, base_address, full_path_name ):
    reg_name = reg.nameToUse()
    reg_name = ProcessName(reg_name)

    (reg_desc,line) = DescDocCombo( reg, reg, full_path_name, EH )

    reg_reset = reg.rawResetValue().lower()
    reg_reset = Verilog.ConvertBase(reg_reset,2,16)[0]

    from_addr = base_address + reg.intAddress()
    arr = reg.arraySpec()
    if arr.enabled():
        to_addr = from_addr + (arr.m_Size-1) * arr.m_Stride
        to_addr_str = VerilogAddress(to_addr)
        from_addr_str = VerilogAddress(from_addr)
        reg_addr_str = "Address:<tt>{0}</tt> &#8211; <tt>{1}</tt> by <tt>{2}</tt>" \
                                     .format(from_addr_str,to_addr_str,str(arr.m_Stride))
        reg_quantity_str = ", quantity:<tt>{0}</tt>".format(arr.m_Size)
    else:
        reg_addr_str = "Address:<tt>" + VerilogAddress(from_addr) + "</tt>"

        reg_quantity_str = ""

    if len(reg_desc) > 0:
        # toc_text = reg_name + "&#8195;" + reg_desc		# emspace
        toc_text = reg_name + " : " + reg_desc
    else:
        toc_text = reg_name

    para = HPara( style="H2", text="Register: {0}".format(reg_name) )
    para.setLinkDst(MakeDstLink(full_path_name,reg))
    for ref in reg.fieldRefs():
        field = ref.bitField()
        if not HSTypeIsToBeShown(field):
            continue
        if field.nameToUse() != "RESERVED":
            para.addLinkDst(MakeDstLink(full_path_name,field))
    sec = HSection(parent=parent,style="sec",toc=HText(toc_text),head=para)

    reg_reset_str = ", Reset:<tt>{0}</tt>".format(reg_reset)
    HPara( parent=sec, style="PLine", text="{0}{1}{2}".format(reg_addr_str,reg_quantity_str,reg_reset_str) )

    HPara( parent=sec, text=line  )

    add_notes( reg, sec, reg, full_path_name, EH )

    # sec = HSection(parent=sec,style="sec")
    if GenImages:
        RegisterImage( reg, sec )
    RegisterDetailTable( reg, sec, full_path_name )


def gen_rmap( rmap, one_std, parent, base_address, full_path_name ):
    rmap_name = rmap.nameToUse()
    if one_std:
        para = HPara( style="H2", text="Registers:" )
        para.setLinkDst(MakeDstLink(full_path_name,rmap))
        rmap_div = HSection(parent=parent,style="sec",toc=HText("Registers"),head=para)
    else:
        para = HPara( style="H2", text="Register Map: {0}".format(rmap_name) )
        para.setLinkDst(MakeDstLink(full_path_name,rmap))
        rmap_desc = DescriptionToUse(rmap); rmap_desc = PerformDSPTextProcessing( rmap_desc, rmap, full_path_name, EH )
        if len(rmap_desc) > 0:
            # toc_text = rmap_name + "&#8195;" + rmap_desc		# emspace
            toc_text = rmap_name + " : " + rmap_desc
        else:
            toc_text = rmap_name
        rmap_div = HSection(parent=parent,style="sec",toc=HText(toc_text),head=para)

    rmap_address = base_address + rmap.intAddress()
    if one_std:
        pass
    else:
        rmap_addr_width = rmap.intAddressWidth()
        map_byte_order = rmap.rawByteOrder()
        HPara( parent=rmap_div, style="PLine", \
               text="Address:<tt>{0}</tt>, AddressWidth:<tt>{1}</tt>, ByteOrder:<tt>{2}</tt>" \
                                .format(VerilogAddress(rmap_address),rmap_addr_width,map_byte_order) )

    regs = RegistersToDisplay(rmap)
    if len(regs) > 0:
        # sec = HSection(parent=rmap_div,style="sec")
        HPara( parent=rmap_div, text="Register summary table:")
        if GenSimpleRegTable:
            SimplifiedRegSummaryTable( rmap, rmap_div, rmap_address, full_path_name )
        else:
            FullRegSummaryTable( rmap, rmap_div, rmap_address, full_path_name )
        # HPara( parent=rmap_div, style="H2", text="Registers:" )
        # regs_div = HSection(parent=rmap_div,style="sec")
        for reg in regs:
            RegDetail( reg, rmap_div, rmap_address, full_path_name )


def gen_timing_diagrams( mod, parent, full_path_name ):
    td_list = mod.timingDiagrams()
    if len(td_list) > 0:
        para = HPara( style="H2", text="Timing Diagrams:" )
        tds_div = HSection(parent=parent,style="sec",toc=HText("Timing Diagrams"),head=para)
        for td in td_list:
            td_name = td.nameToUse()
            para = HPara( style="H2", text="Timing Diagram: {0}".format(td_name) )
            para.setLinkDst( "TimDiag{0}".format(td.uid()) )

            (td_desc,line) = DescDocCombo( td, td, full_path_name, EH )
            if len(td_desc) > 0:
                # toc_text = td_name + "&#8195;" + td_desc		# emspace
                toc_text = td_name + " : " + td_desc
            else:
                toc_text = td_name
            td_div = HSection(parent=tds_div,style="sec",toc=HText(toc_text),head=para)

            HPara( parent=td_div, text=line  )

            add_notes( td, td_div, td, full_path_name, EH )

            TimingDiagramImage( td, td_div )


def gen_package_drawings( mod, parent, full_path_name ):
    pkg_list = mod.packages()
    if len(pkg_list) > 0:
        para = HPara( style="H2", text="Package Drawings:" )
        pkgs_div = HSection(parent=parent,style="sec",toc=HText("Package Drawings"),head=para)
        for pkg in pkg_list:
            pkg_name = pkg.nameToUse()
            para = HPara( style="H2", text="Package Drawing: {0}".format(pkg_name) )
            para.setLinkDst( "PkgDraw{0}".format(pkg.uid()) )

            desig = pkg.rawADIDesignator()
            if len(desig) == 0:
                desig = "N/A"
            pkg_type = pkg.rawPkgType()
            if len(pkg_type) == 0:
                pkg_type = "N/A"
            pin_count = pkg.rawPinCount()
            if len(pin_count) >= 2 and pin_count[:2] == "'d":
                pin_count = pin_count[2:]

            (pkg_desc,line) = DescDocCombo( pkg, pkg, full_path_name, EH )
            if len(pkg_desc) > 0:
                # toc_text = pkg_name + "&#8195;" + pkg_desc		# emspace
                toc_text = pkg_name + " : " + pkg_desc
            else:
                toc_text = pkg_name

            pkg_div = HSection(parent=pkgs_div,style="sec",toc=HText(toc_text),head=para)

            HPara( parent=pkg_div, style="PLine", \
                   text="Package designator: {0}, Package type: {1}, Pin count: {2}".format(desig,pkg_type,pin_count) )

            HPara( parent=pkg_div, text=line  )

            add_notes( pkg, pkg_div, pkg, full_path_name, EH )

            ObjectImage( pkg, pkg_div )


def gen_port_table( ports, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Direction"); header.addCell(cell)
    cell = HTableCell(text="Port Type"); header.addCell(cell)
    cell = HTableCell(text="Width"); header.addCell(cell)
    cell = HTableCell(text="Polarity"); header.addCell(cell)
    cell = HTableCell(text="Interrupt"); header.addCell(cell)
    cell = HTableCell(text="DMA Channel"); header.addCell(cell)
    cell = HTableCell(text="Trigger"); header.addCell(cell)
    cell = HTableCell(text="Chip Signal"); header.addCell(cell)
    cell = HTableCell(text="Sensitivity"); header.addCell(cell)
    cell = HTableCell(text="Visibility"); header.addCell(cell)
    for port in ports:
        row = HTableRow(parent=tbl,header=False)

        name = port.nameToUse(); # name = ProcessName( name )
        arr = port.arraySpec()
        if arr.enabled():
            lsb = Verilog.IntegerValue( arr.m_From, 10 )[0]
            msb = Verilog.IntegerValue( arr.m_To, 10 )[0]
            if lsb == msb:
                name = re.sub("\[.*\]","[%s]"%(lsb),name)
            else:
                name = re.sub("\[.*\]","[%s:%s]"%(msb,lsb),name)
        cell = HTableCell(text=name); row.addCell(cell)

        (desc,line) = DescDocCombo( port, port, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( port, cell, port, full_path_name, EH )

        dir = port.rawDirection()
        cell = HTableCell(text=dir); row.addCell(cell)

        ptype = port.rawPortType()
        cell = HTableCell(text=ptype); row.addCell(cell)

        width = port.intWidth()
        cell = HTableCell(text=width); row.addCell(cell)

        pol = port.rawPolarity()
        cell = HTableCell(text=pol); row.addCell(cell)

        inter = port.rawInterrupt()
        cell = HTableCell(text=inter); row.addCell(cell)

        dma = port.rawDMAChannel()
        cell = HTableCell(text=dma); row.addCell(cell)

        trig = port.rawTrigger()
        cell = HTableCell(text=trig); row.addCell(cell)

        chips = port.rawChipSignal()
        cell = HTableCell(text=chips); row.addCell(cell)

        sens = port.rawSensitivity()
        cell = HTableCell(text=sens); row.addCell(cell)

        vis = port.rawVisibility()
        cell = HTableCell(text=vis); row.addCell(cell)


sub_mod_files = {}
def master_file( depth, inst, inst_name, base_address, full_path_name ):
    # print("|{0}master_file: inst_name:{1} fpn:{2}" .format('_'*2*(depth-1),inst_name,full_path_name))
    global sub_mod_files
    # inst_name = inst.nameToUse()
    # print("master_file:","inst_name:",inst_name)
    if not full_path_name in sub_mod_files:
        # sz = len(sub_mod_files)
        # sub_mod_files[path] = "place_holder"
        # base_address = inst.intAbsoluteAddress()
        path = inst.rawPath()					# /tmp/SPORT.V1234
        # print("master_file:","path:",path)

        # Ignore any base address of the Module (override with 0).
        # At run time the bass address cookie is used to offset all addresses.
        mod = YodaLoader.YModule(path,0)
        SetHardSoftContext(mod)
        # mod = YodaLoader.YModule(path,"'d"+str(base_address))

        # Keep the file names simple.
        mod.m_HTMLFilePrefix = full_path_name + "_"
        mod.m_HTMLFilePrefix = mod.m_HTMLFilePrefix.replace("<br/>","_")
        mod.m_HTMLFilePrefix = re.sub("[^A-Za-z0-9_]","_",mod.m_HTMLFilePrefix)

        fn = gen_files(depth,mod,base_address,False,inst,full_path_name)
        sub_mod_files[full_path_name] = fn
    else:
        print("|{0}Seen before.".format('_'*2*(depth-1)))
        pass
    fn = sub_mod_files[full_path_name]
    # print("master_file:","fn:",fn)
    return fn


def is_alpha_based( arr ):
    ret = (len(arr.m_From) == 1 and arr.m_From[0].isalpha()) and\
            (len(arr.m_To) == 1 and arr.m_To[0].isalpha())
    return ret;


# How many letters are there between the brackets: [nn] <- 2
#
def implied_field_width(name):
    ret = 1
    match = re.search("\[(.*)\]", name)
    if match:
        ret = len(match.group(1))
    return ret


def iname_iaddr_pairs( name, addr, stride, arr ):
    list = []
    fw = implied_field_width(name)
    if is_alpha_based(arr):
        for x in range(ord(arr.m_From),ord(arr.m_To)+1,int(arr.m_By)):
            iname = re.sub( "\[.*\]", "[%s]"%(chr(x)), name )
            list.append( (iname,addr) )
            addr = addr + stride
    else:
        from_i = Verilog.IntegerValue( arr.m_From, 10 )[0]
        to_i = Verilog.IntegerValue( arr.m_To, 10 )[0]
        by_i = Verilog.IntegerValue( arr.m_By, 10 )[0]
        for x in range(from_i,to_i+1,by_i):
            ind = str(x); ind = ind.zfill(fw);	# Pad left with 0 to field width.
            repl = "[{0}]".format(ind);
            iname = re.sub( "\[.*\]", repl, name )
            list.append( (iname,addr) )
            addr = addr + stride
    return list


def gen_inst_table_row(tbl,depth,inst,inst_name,ref_mod_name,inst_addr_num,inst_addr_str,over_text,desc_line,fpn):

    row = HTableRow(parent=tbl,header=False)

    mod_file = master_file(depth+2,inst,inst_name,inst_addr_num,fpn)

    # Column #1: Instance name.
    cell = HTableCell(text=inst_name); row.addCell(cell)
    if GenSingleFile:
        cell.setLinkSrc("#TOC{0}".format(GblMasterTOCNumber[fpn]))
    else:
        cell.setLinkSrc("{0}".format(mod_file),target="_blank")

    # Column #2: Instanced module name.
    #            Hyperlinked to the master.
    cell = HTableCell(text=ref_mod_name); row.addCell(cell)

    # Column #3: Instanced address.
    cell = HTableCell(text=inst_addr_str,tt=True)
    row.addCell(cell)

    # Column #4: Parameter override.
    cell = HTableCell(text=over_text); row.addCell(cell)

    # Column #5: Description.
    cell = HTableCell(text=desc_line); row.addCell(cell)


def gen_inst_table( depth, base_address, insts, parent, full_path_name ):
    # print("|{0}gen_inst_table: base_address:{1}, fpn:{2}".format('_'*2*(depth-1),VerilogAddress(base_address),full_path_name))
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Instance<br/>Name"); header.addCell(cell)
    cell = HTableCell(text="Module"); header.addCell(cell)
    cell = HTableCell(text="Instance<br/>Address"); header.addCell(cell)
    cell = HTableCell(text="Parameter<br/>Ovedrrides"); header.addCell(cell)
    cell = HTableCell(text="Module Description"); header.addCell(cell)
    for inst in insts:

        inst_name = inst.nameToUse(); # inst_name = ProcessName( inst_name )

        ref_mod_name = inst.rawModuleName()

        over_text = ""
        overs = inst.parameterOverrides()
        for over in overs:
            if len(over_text) > 0:
                over_text += "<br/>"
            over_text += "{0}={1}".format( over.parameterName(), over.overrideValue() )

        ref_mod = inst.referencedModule()
        desc = ref_mod.rawDescription(); desc = PerformDSPTextProcessing( desc, ref_mod, full_path_name, EH )
        desc_line = desc
        if len(desc_line) > 0 and desc_line[len(desc_line)-1] != ".":
            desc_line += "."
        desc_line = re.sub("\n\n+","<br/>",desc_line)

        inst_addr_num = base_address + inst.intAddress()
        arr = inst.arraySpec()
        if arr.enabled():
            pair_list = iname_iaddr_pairs( inst_name, inst_addr_num, arr.m_Stride, arr )
            for pair in pair_list:
                iname = pair[0]; iaddr = pair[1]
                fpn = (full_path_name+"_"+iname) if (len(full_path_name)>0) else iname
                inst_addr_str = VerilogAddress(iaddr)
                gen_inst_table_row(tbl,depth,inst,iname,ref_mod_name,iaddr,inst_addr_str,over_text,desc_line,fpn)
        else:
            inst_addr_str = VerilogAddress(inst_addr_num)
            fpn = (full_path_name+"_"+inst_name) if (len(full_path_name)>0) else inst_name
            gen_inst_table_row(tbl,depth,inst,inst_name,ref_mod_name,inst_addr_num,inst_addr_str,over_text,desc_line,fpn)


def gen_interrupt_table( intrs, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="ID"); header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Instance"); header.addCell(cell)
    cell = HTableCell(text="Port"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for intr in intrs:
        row = HTableRow(parent=tbl,header=False)

        id_number = intr.rawID();
        cell = HTableCell(text=id_number); row.addCell(cell)

        name = intr.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        instance_ref = intr.rawInstanceRef();
        cell = HTableCell(text=instance_ref); row.addCell(cell)

        port_ref = intr.rawPortRef();
        cell = HTableCell(text=port_ref); row.addCell(cell)

        (desc,line) = DescDocCombo( intr, intr, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( intr, cell, intr, full_path_name, EH )


def gen_trig_table( trigs, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="ID"); header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Instance"); header.addCell(cell)
    cell = HTableCell(text="Port"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for trig in trigs:
        row = HTableRow(parent=tbl,header=False)

        id_number = trig.rawID();
        cell = HTableCell(text=id_number); row.addCell(cell)

        name = trig.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        instance_ref = trig.rawInstanceRef();
        cell = HTableCell(text=instance_ref); row.addCell(cell)

        port_ref = trig.rawPortRef();
        cell = HTableCell(text=port_ref); row.addCell(cell)

        (desc,line) = DescDocCombo( trig, trig, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( trig, cell, trig, full_path_name, EH )


def gen_dmac_table( dmacs, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="ID"); header.addCell(cell)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Instance"); header.addCell(cell)
    cell = HTableCell(text="Port"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for dmac in dmacs:
        row = HTableRow(parent=tbl,header=False)

        id_number = dmac.rawID();
        cell = HTableCell(text=id_number); row.addCell(cell)

        name = dmac.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        instance_ref = dmac.rawInstanceRef();
        cell = HTableCell(text=instance_ref); row.addCell(cell)

        port_ref = dmac.rawPortRef();
        cell = HTableCell(text=port_ref); row.addCell(cell)

        (desc,line) = DescDocCombo( dmac, dmac, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( dmac, cell, dmac, full_path_name, EH )


def gen_csig_table( csigs, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Instance"); header.addCell(cell)
    cell = HTableCell(text="Port"); header.addCell(cell)
    cell = HTableCell(text="Port<br/>Letter"); header.addCell(cell)
    cell = HTableCell(text="Port<br/>Index"); header.addCell(cell)
    cell = HTableCell(text="Pad Ref"); header.addCell(cell)
    cell = HTableCell(text="Port<br/>Mux Slot"); header.addCell(cell)
    cell = HTableCell(text="Polarity"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for csig in csigs:
        row = HTableRow(parent=tbl,header=False)

        name = csig.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        instance_ref = csig.rawInstanceRef();
        cell = HTableCell(text=instance_ref); row.addCell(cell)

        port_ref = csig.rawPortRef();
        cell = HTableCell(text=port_ref); row.addCell(cell)

        port_letter = csig.rawPortLetter();
        cell = HTableCell(text=port_letter); row.addCell(cell)

        port_index = csig.rawPortIndex();
        cell = HTableCell(text=port_index); row.addCell(cell)

        pad_ref = csig.rawPadRef();
        cell = HTableCell(text=pad_ref); row.addCell(cell)

        port_mux_slot = csig.rawPortMuxSlot();
        cell = HTableCell(text=port_mux_slot); row.addCell(cell)

        polarity = csig.rawPolarity();
        cell = HTableCell(text=polarity); row.addCell(cell)

        (desc,line) = DescDocCombo( csig, csig, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( csig, cell, csig, full_path_name, EH )


def gen_pad_table( pads, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Pad<br/>Type"); header.addCell(cell)
    cell = HTableCell(text="Driver<br/>Type"); header.addCell(cell)
    cell = HTableCell(text="Power<br/>Domain"); header.addCell(cell)
    cell = HTableCell(text="Pkg 1<br/>Pins"); header.addCell(cell)
    cell = HTableCell(text="Pkg 2<br/>Pins"); header.addCell(cell)
    cell = HTableCell(text="Pkg 3<br/>Pins"); header.addCell(cell)
    cell = HTableCell(text="Pkg 4<br/>Pins"); header.addCell(cell)
    for pad in pads:
        row = HTableRow(parent=tbl,header=False)

        name = pad.nameToUse();
        color = pad.rawColor();
        cell = HTableCell(text=name,bgcolor=color); row.addCell(cell)

        (desc,line) = DescDocCombo( pad, pad, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( pad, cell, pad, full_path_name, EH )

        pad_type = pad.rawPadType();
        cell = HTableCell(text=pad_type); row.addCell(cell)

        driver_type = pad.rawDriverType();
        cell = HTableCell(text=driver_type); row.addCell(cell)

        power_domain = pad.rawPowerDomain();
        cell = HTableCell(text=power_domain); row.addCell(cell)

        pkg1pins = pad.rawPkg1Pins();
        cell = HTableCell(text=pkg1pins); row.addCell(cell)

        pkg2pins = pad.rawPkg2Pins();
        cell = HTableCell(text=pkg2pins); row.addCell(cell)

        pkg3pins = pad.rawPkg3Pins();
        cell = HTableCell(text=pkg3pins); row.addCell(cell)

        pkg4pins = pad.rawPkg4Pins();
        cell = HTableCell(text=pkg4pins); row.addCell(cell)


def gen_page_mask_table( page_masks, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Width"); header.addCell(cell)
    cell = HTableCell(text="Read Priority"); header.addCell(cell)
    for page_mask in page_masks:
        row = HTableRow(parent=tbl,header=False)

        name = page_mask.nameToUse(); # name = ProcessName( name )
        cell = HTableCell(text=name); row.addCell(cell)

        (desc,line) = DescDocCombo( page_mask, page_mask, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( page_mask, cell, page_mask, full_path_name, EH )

        width = page_mask.rawWidth()
        width = Verilog.IntegerValue( width, 10 )[0]
        cell = HTableCell(text=width); row.addCell(cell)

        read_prio = page_mask.readPriority()
        cell = HTableCell(text=read_prio); row.addCell(cell)


def gen_errors_table( error_cats, parent ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Error<br/>Number"); header.addCell(cell)
    cell = HTableCell(text="Error<br/>Description"); header.addCell(cell)
    cell = HTableCell(text="Number<br/>of Occurrences"); header.addCell(cell)
    for error_cat in error_cats:
        row = HTableRow(parent=tbl,header=False)

        num = error_cat.intErrorNumber()
        cell = HTableCell(text=num); row.addCell(cell)

        desc = error_cat.rawDescription()
        cell = HTableCell(text=desc); row.addCell(cell)

        count = error_cat.intInstanceCount()
        cell = HTableCell(text=count); row.addCell(cell)


def gen_suppression_table( supps, parent ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Error<br/>Number"); header.addCell(cell)
    cell = HTableCell(text="Error<br/>Description"); header.addCell(cell)
    cell = HTableCell(text="Scope<br/>of Suppression"); header.addCell(cell)
    cell = HTableCell(text="Subject(s)"); header.addCell(cell)
    cell = HTableCell(text="Involved<br/>Property"); header.addCell(cell)
    cell = HTableCell(text="Hit<br/>Count"); header.addCell(cell)
    for supp in supps:
        row = HTableRow(parent=tbl,header=False)

        num = supp.intErrorNumber()
        cell = HTableCell(text=num); row.addCell(cell)

        error_desc = supp.rawErrorDescription()
        cell = HTableCell(text=error_desc); row.addCell(cell)

        scope = supp.rawScope()
        cell = HTableCell(text=scope); row.addCell(cell)

        subject = supp.subjectObject()
        if subject is None:
            subject = supp.rawTargetType()
            if subject is None:
                subject = "N/A";
            else:
                subject = "All {0}s".format(subject)
        else:
            subject = "{0}: {1}".format(subject.objectType(),subject.nameToUse())
        cell = HTableCell(text=subject); row.addCell(cell)

        prop = supp.rawProperty()
        cell = HTableCell(text=prop); row.addCell(cell)

        count = supp.intHitCount()
        cell = HTableCell(text=count); row.addCell(cell)


def gen_parameter_table( parms, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Type"); header.addCell(cell)
    cell = HTableCell(text="Value"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Visibility"); header.addCell(cell)
    for parm in parms:
        row = HTableRow(parent=tbl,header=False)

        name = parm.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        dtype = parm.rawDataType();
        cell = HTableCell(text=dtype); row.addCell(cell)

        value = parm.rawDefaultValue();
        cell = HTableCell(text=value); row.addCell(cell)

        (desc,line) = DescDocCombo( parm, parm, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( parm, cell, parm, full_path_name, EH )

        vis = parm.rawVisibility();
        cell = HTableCell(text=vis); row.addCell(cell)


def gen_rmap_table( rmaps, parent, base_address, full_path_name ):
    div = parent
    tbl = HTable(parent=div,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Address"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    cell = HTableCell(text="Address<br/>Width"); header.addCell(cell)
    cell = HTableCell(text="Byte Order"); header.addCell(cell)
    for rmap in rmaps:
        row = HTableRow(parent=tbl,header=False)

        name = rmap.nameToUse();
        name = break_long_name( name, 10 )
        cell = HTableCell(text=name); row.addCell(cell)
        cell.setLinkSrc(MakeSrcLink(full_path_name,rmap))

        rmap_address = base_address + rmap.intAddress()
        rmap_address = Verilog.FormatNumber(rmap_address,16,4,0)
        cell = HTableCell(text=rmap_address); row.addCell(cell)

        (desc,line) = DescDocCombo( rmap, rmap, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        addr_width = rmap.intAddressWidth()
        cell = HTableCell(text=addr_width); row.addCell(cell)

        map_byte_order = rmap.rawByteOrder()
        cell = HTableCell(text=map_byte_order); row.addCell(cell)


def has_any_registers( rmaps ):
    for rmap in rmaps:
        for reg in rmap.registers():
            return True
    return False


def one_std_rmap( rmaps ):
    std = False
    #if len(rmaps) == 1:
    #    rmap = rmaps[0]
    #    addr = rmap.intAddress()
    #    byte_order = rmap.rawByteOrder()
    #    name = rmap.nameToUse()
    #    if addr == 0 and byte_order == "LittleEndian" and name == "RegMap1":
    #        std = True
    return std


def trigger_types( trigs ):
    m = []
    s = []
    for t in trigs:
        tt = t.rawTrigType();
        if tt == "Master" or tt == "Both":
            m.append(t)
        else:
            s.append(t)
    return (m,s)


def add_description_cell( row, obj, full_path_name ):
    (desc,line) = DescDocCombo( obj, obj, full_path_name, EH )
    cell = HTableCell(text=line); row.addCell(cell)
    add_notes( obj, cell, obj, full_path_name, EH )


def gen_fgrps_table( frgps, parent, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for grp in frgps:
        row = HTableRow(parent=tbl,header=False)

        name = grp.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)
        cell.setLinkSrc(MakeSrcLink(full_path_name,grp))

        add_description_cell( row, grp, full_path_name )

    for grp in frgps:
        title = "Group: {0}".format(grp.nameToUse())
        para = HPara( style="H2", text=title )
        para.setLinkDst(MakeDstLink(full_path_name,grp))
        grp_div = HSection(parent=parent,style="sec",toc=HText(title),head=para)
        tbl_sec = HSection(parent=grp_div)
        tbl = HTable(parent=tbl_sec,style="Normal")
        header = HTableRow(parent=tbl,header=True)
        cell = HTableCell(text="Name"); header.addCell(cell)
        cell = HTableCell(text="Type"); header.addCell(cell)
        cell = HTableCell(text="Description"); header.addCell(cell)
        for ref in grp.objectRefs():
            row = HTableRow(parent=tbl,header=False)

            name = ref.referencedObject().nameToUse();
            obj_type = ref.referencedObject().objectType();

            cell = HTableCell(text=name); row.addCell(cell)
            if obj_type == "Register":
                cell.setLinkSrc(MakeSrcLink(full_path_name,ref.referencedObject()))
            elif obj_type == "BitField":
                cell.setLinkSrc(MakeSrcLink(full_path_name,ref.referencedObject()))
            elif obj_type == "FunctionalGroup":
                cell.setLinkSrc(MakeSrcLink(full_path_name,ref.referencedObject()))

            cell = HTableCell(text=obj_type); row.addCell(cell)

            add_description_cell( row, ref.referencedObject(), full_path_name )


def gen_mregs_table( mregs, parent, base_address, full_path_name ):
    tbl = HTable(parent=parent,style="Normal")
    header = HTableRow(parent=tbl,header=True)
    cell = HTableCell(text="Name"); header.addCell(cell)
    cell = HTableCell(text="Access"); header.addCell(cell)
    cell = HTableCell(text="Address"); header.addCell(cell)
    cell = HTableCell(text="Size"); header.addCell(cell)
    cell = HTableCell(text="UnitWidth"); header.addCell(cell)
    cell = HTableCell(text="Type"); header.addCell(cell)
    cell = HTableCell(text="DataFormat"); header.addCell(cell)
    cell = HTableCell(text="Location"); header.addCell(cell)
    cell = HTableCell(text="Description"); header.addCell(cell)
    for mreg in mregs:
        row = HTableRow(parent=tbl,header=False)

        name = mreg.nameToUse();
        cell = HTableCell(text=name); row.addCell(cell)

        access = mreg.rawAccess();
        cell = HTableCell(text=access); row.addCell(cell)

        address = base_address + mreg.intAddress();
        address = VerilogAddress(address)
        cell = HTableCell(text=address); row.addCell(cell)

        size = mreg.rawSize();
        cell = HTableCell(text=size); row.addCell(cell)

        unit_width = mreg.rawUnitWidth();
        cell = HTableCell(text=unit_width); row.addCell(cell)

        type_str = mreg.rawType();
        cell = HTableCell(text=type_str); row.addCell(cell)

        data_format = mreg.rawDataFormat();
        cell = HTableCell(text=data_format); row.addCell(cell)

        location = mreg.rawLocation();
        cell = HTableCell(text=location); row.addCell(cell)

        (desc,line) = DescDocCombo( mreg, mreg, full_path_name, EH )
        cell = HTableCell(text=line); row.addCell(cell)

        add_notes( mreg, cell, mreg, full_path_name, EH )

def gen_mregs_image( mod, parent ):
    CreateImages( mod )				# pragma: no cover
    mreg_root_uid = mod.memoryRegionRootUID()
    file_name = FullPathImageFileName(mod,mreg_root_uid)
    if os.path.isfile(file_name):
        file_name = RelativeImageFileName(mod,mreg_root_uid)
        HImage( parent=parent, src=file_name )


def gen_html_hdoc( depth, mod, base_address, inst, full_path_name ):
    global GblRootHDoc

    d = HDocument("Test")
    if GenSingleFile:
        if depth == 1:
            # print("Set root doc")
            GblRootHDoc = d
        else:
            # print("Encounterd sub doc")
            GblRootHDoc.addTaggedHDocDiv(full_path_name,d)

    # margin: top right bottom left
    d.registerStyle("a",         "{ text-decoration:none; }")	# No underline. Interferes with underscores.
    d.registerStyle("p",         "{ margin: 0.3em 0em 0.3em 0em; padding:0em; }")
    d.registerStyle("p.H1",      "{ font-weight:bold; font-size:175%; }")
    d.registerStyle("p.H2",      "{ font-weight:bold; font-size:125%; margin: 1.0em 0em 0.5em 0em; padding:0em; }")
    d.registerStyle("p+ul",      "{ margin-top: 0; }")
    d.registerStyle("p+ol",      "{ margin-top: 0; }")
    d.registerStyle("ol",        "{ margin-bottom: 0; }")
    d.registerStyle("ul",        "{ margin-bottom: 0; }")
    d.registerStyle("p.PLine",   ( "{"
                                   "  margin: -0.4em 0.0em 0.5em 0.0em;"
                                   "  font-family: courier;"
                                   "  font-size:110%;"
                                   "  padding: 0em;"
                                   "}" ))
    d.registerStyle("p.RegName", "{ font-weight: bold;    }")
    d.registerStyle("div.Notes", "{ color: rgb(204,51,0); }")
    d.registerStyle("div.User", "{ color: rgb(204,51,0); }")
    # d.registerStyle("pre",       "{ font-family: courier; }")
    d.registerStyle("img.Center","{"+\
                    "  display: block;"+\
                    "  margin-left: auto;"+\
                    "  margin-right: auto;"+\
                    "}")
    d.registerStyle("div.sec",   "{ margin-left:2em;  }")
    d.registerStyle("div.adi",   "{ border: solid;"+\
                    "  border-width: thin;"+\
                    "  font-family: verdana,arial,sans-serif;"+\
                    "  font-size:14px;"+\
                    "  width: 80%;"+\
                    "  background: rgb(0,102,255);"+\
                    "  color: rgb(255,255,255);"+\
                    "  padding: 0.5em;"+\
                    "}")
    d.registerStyle("table.Normal", "{"+\
                    "  margin: 1.0em 2.0em 0.5em 0.0em;"+\
                    "  font-family: verdana,arial,sans-serif;"+\
                    "  font-size:11px;"+\
                    "  color:#333333;"+\
                    "  border-width: 1px;"+\
                    "  border-color: #666666;"+\
                    "  border-collapse: collapse;"+\
                    "}")
    d.registerStyle("table.Normal td", "{"+\
                    "  border-width: 1px;"+\
                    "  padding: 6px;"+\
                    "  border-style: solid;"+\
                    "  border-color: #666666;"+\
                    "  background-color: #ffffff;"+\
                    "  vertical-align: top;"+\
                    "}")
    d.registerStyle("table.Normal th", "{"+\
                    "  border-width: 1px;"+\
                    "  padding: 6px;"+\
                    "  border-style: solid;"+\
                    "  border-color: #666666;"+\
                    "  background-color: #dedede;"+\
                    "}")
    d.registerStyle("table.BML", "{"+\
                    "  border: 1px solid red;"+\
                    "  border-collapse: collapse;"+\
                    "}")
    d.registerStyle("table.BML th", "{"+\
                    "  border: 1px solid red;"+\
                    "  padding: 0px 0px 0px 0px;"+\
                    "  border-collapse: collapse;"+\
                    "}")
    d.registerStyle("table.BML td", "{"+\
                    "  border: 1px solid red;"+\
                    "  padding: 0px 0px 0px 0px;"+\
                    "  border-collapse: collapse;"+\
                    "}")
    d.registerStyle("table.BML p", "{"+\
                    "  margin:  3px 3px 3px 3px;"+\
                    "  padding: 0px 0px 0px 0px;"+\
                    "}")
    d.registerStyle("table.Enum", "{"+\
                    "  margin: 0.0em 0.0em 0.0em 1.0em;"+\
                    "  padding: 0.0em 0.0em 0.0em 0.0em;"+\
                    "  font-family: verdana,arial,sans-serif;"+\
                    "  font-size:11px;"+\
                    "  border-width: 0px;"+\
                    "}")
    d.registerStyle("table.Enum td", "{"+\
                    "  margin: 0.0em 0.0em 0.0em 0.0em;"+\
                    "  padding: 0.0em 0.0em 0.0em 0.0em;"+\
                    "  border-width: 0px;"+\
                    "  padding: 0px;"+\
                    "  vertical-align: top;"+\
                    "  border-width: 0px;"+\
                    "}")

    if not GenSingleFile or depth == 1:
        from_logo = os.path.join( YodaInfo.GeneratorResourceDir(), "adilogo.jpg" )
        logo_file = os.path.join( JobResourcesDir(), "adilogo.jpg" )
        copyfile( from_logo, logo_file )
        logo_file = "adilogo.jpg"

        intro = HSection(parent=d,style="adi")
        if intro:
            HImage(parent=intro,src=logo_file)
            lt = YodaLoader._LegalText("HTML","internal.dat")
            lt = lt.replace( "\n", "<br/>" )
            if len(lt) > 0:
                lt = "<br/>" + lt + "<br/>" + "<br/>"
            HPara(parent=intro,text=lt)
            date_time = time.strftime("%Y %b %d, %H:%M %Z").upper()
            #if YodaInfo.TestMode():
            #    date_time = "__TEST_MODE__"
            HPara(parent=intro,text="Last updated: {0}".format(date_time) )

    if inst and inst.rawModuleName() != mod.nameToUse():
        mod_spec = "{} ({})".format(mod.nameToUse(),inst.rawModuleName())
    else:
        mod_spec = mod.nameToUse()

    if len(full_path_name) > 0:
        para = HPara(style="H1",text="Module: {0}, Instance: {1}".format(mod_spec,full_path_name))
    else:
        para = HPara(style="H1",text="Module: {0}".format(mod.nameToUse()))

    if GenSingleFile and len(full_path_name) > 0:
        toc_text = "{0} ({1})".format(full_path_name,mod.nameToUse())
    else:
        toc_text = mod_spec
    main = HSection(parent=d,toc=HText(toc_text),head=para)
    GblMasterTOCNumber[full_path_name] = main.m_TOCNumber

    mod_desc = mod.rawDescription()
    mod_desc = PerformDSPTextProcessing( mod_desc, mod, full_path_name, EH )
    if len(mod_desc) > 0:
        HPara(parent=main,text=mod_desc)

    mod_doc = mod.rawDocumentation()
    mod_doc = PerformDSPTextProcessing( mod_doc, mod, full_path_name, EH )
    mod_doc = re.sub("\n\n+","<br/>",mod_doc)
    if len(mod_doc) > 0 and mod_doc != mod_desc:
        HPara(parent=main,text=mod_doc)

    add_notes( mod, main, mod, full_path_name, EH )

    para = HPara( style="H2", text="Module Properties:" )
    sec = HSection(parent=main,style="sec",toc=HText("Properties"),head=para)
    tbl = HTable(parent=sec,style="Normal")
    tbl.setHeader2( "Property", "Value" )
    tbl.addRow2( "Address", VerilogAddress(base_address) )
    tbl.addRow2( "Address Unit Bits", mod.intAddressUnitBits() )
    tbl.addRow2( "Bit Field Scope", mod.rawBitFieldScope() )
    tbl.addRow2( "Enumeration Scope", mod.rawEnumerationScope() )
    tbl.addRow2( "Reserve Option", mod.rawReserveOption() )
    tbl.addRow2( "Memory Access Method", mod.rawMemoryAccessMethod() )

    parms = mod.parameters()
    if len(parms) > 0:
        para = HPara( style="H2", text="Parameters:" )
        parms_div = HSection(parent=main,style="sec",toc=HText("Parameters"),head=para)
        gen_parameter_table(parms,parms_div,full_path_name)

    error_cats = mod.errorCategories()
    if len(error_cats) > 0:
        para = HPara( style="H2", text="Unsuppressed Errors:" )
        error_cats_div = HSection(parent=main,style="sec",toc=HText("Unsuppressed Errors"),head=para)
        gen_errors_table(error_cats,error_cats_div)

    supps = mod.suppressions()
    if len(supps) > 0:
        para = HPara( style="H2", text="Suppressed Errors:" )
        supps_div = HSection(parent=main,style="sec",toc=HText("Suppressed Errors"),head=para)
        gen_suppression_table(supps,supps_div)

    page_masks = mod.pageMasks()
    if len(page_masks) > 0:
        para = HPara( style="H2", text="Page Masks:" )
        para.setLinkDst("PAGE_MASK_TABLE")
        page_mask_div = HSection(parent=main,style="sec",toc=HText("Page Masks"),head=para)
        gen_page_mask_table(page_masks,page_mask_div,full_path_name)

    ports = mod.ports()
    if len(ports) > 0:
        para = HPara( style="H2", text="Ports:" )
        para.setLinkDst("PORT_TABLE")
        port_div = HSection(parent=main,style="sec",toc=HText("Ports"),head=para)
        gen_port_table(ports,port_div,full_path_name)

    insts = mod.instances()
    if len(insts) > 0:
        para = HPara( style="H2", text="Instances:" )
        inst_div = HSection(parent=main,style="sec",toc=HText("Instances"),head=para)
        gen_inst_table(depth,base_address,insts,inst_div,full_path_name)

    intrs = mod.interrupts()
    if len(intrs) > 0:
        para = HPara( style="H2", text="Interrupts:" )
        intr_div = HSection(parent=main,style="sec",toc=HText("Interrupts"),head=para)
        gen_interrupt_table(intrs,intr_div,full_path_name)

    trigs = mod.triggers()
    if len(trigs) > 0:
        ptrig = HPara( style="H2", text="Triggers:" )
        trig_div = HSection(parent=main,style="sec",toc=HText("Triggers"),head=ptrig)
        tup = trigger_types(trigs)

        pmast = HPara( style="H2", text="Trigger Masters:" )
        dmast = HSection(parent=trig_div,style="sec",toc=HText("Trigger Masters"),head=pmast)
        gen_trig_table(tup[0],dmast,full_path_name)

        pslave = HPara( style="H2", text="Trigger Slaves:" )
        dslave = HSection(parent=trig_div,style="sec",toc=HText("Trigger Slaves"),head=pslave)
        gen_trig_table(tup[1],dslave,full_path_name)

    dmacs = mod.dmaChannels()
    if len(dmacs) > 0:
        para = HPara( style="H2", text="DMA Channels:" )
        dmac_div = HSection(parent=main,style="sec",toc=HText("DMA Channels"),head=para)
        gen_dmac_table(dmacs,dmac_div,full_path_name)

    csigs = mod.chipSignals()
    if len(csigs) > 0:
        para = HPara( style="H2", text="Chip Signals:" )
        csigs_div = HSection(parent=main,style="sec",toc=HText("Chip Signals"),head=para)
        gen_csig_table(csigs,csigs_div,full_path_name)

    pads = mod.pads()
    if len(pads) > 0:
        para = HPara( style="H2", text="Pads:" )
        pads_div = HSection(parent=main,style="sec",toc=HText("Pads"),head=para)
        gen_pad_table(pads,pads_div,full_path_name)

    rmaps = mod.registerMaps()
    if has_any_registers(rmaps):
        one_std = False
        if len(rmaps) > 0:
            if one_std_rmap(rmaps):
                rmaps_div = main
                one_std = True
            else:
                para = HPara( style="H2", text="Register Maps:" )
                rmaps_div = HSection(parent=main,style="sec",toc=HText("Register Maps"),head=para)
                HPara( parent=rmaps_div, text="Note: All addresses are absolute." )
            if len(rmaps) > 1:
                gen_rmap_table(rmaps,rmaps_div,base_address,full_path_name)
            for rmap in rmaps:
                gen_rmap(rmap,one_std,rmaps_div,base_address,full_path_name)

    mregs = mod.memoryRegions()
    if len(mregs) > 0:
        para = HPara( style="H2", text="Memory regions:" )
        mregs_div = HSection(parent=main,style="sec",toc=HText("Memory regions"),head=para)
        gen_mregs_table(mregs,mregs_div,base_address,full_path_name)
        gen_mregs_image(mregs[0].parentModule(),mregs_div)

    fgrps = mod.functionalGroups()
    if len(fgrps) > 0:
        para = HPara( style="H2", text="Functional Groups:" )
        fgrps_div = HSection(parent=main,style="sec",toc=HText("Functional Groups"),head=para)
        gen_fgrps_table(fgrps,fgrps_div,full_path_name)

    if GenImages:
        gen_timing_diagrams(mod,main,full_path_name)
        gen_package_drawings(mod,main,full_path_name)

    return d


def IndexDotHTML( depth, mod, base_address, top_level ):
    # print("|{0}IndexDotHTML: module:{1}, base_address:{2}, top_level:{3}" \
    #     .format('_'*2*(depth-1),mod.nameToUse(),VerilogAddress(base_address),top_level))
    mod_name = mod.nameToUse()
    html_file_prefix = getattr(mod,"m_HTMLFilePrefix","")
    toc_file_name = html_file_prefix + "toc.html"
    page_file_name = html_file_prefix + "page.html"
    ind = '''
        <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
        <HTML>
        <HEAD>
        <TITLE>MOD_NAME</TITLE>
        </HEAD>

        <FRAMESET ROWS="*">
           <FRAMESET COLS="20%,*">
              <FRAME SRC="TOC_PATH_SPEC" NAME=TOC SCROLLING=AUTO>
              <FRAME SRC="PAG_PATH_SPEC" NAME=CHAPTER>
           </FRAMESET>
        </FRAMESET>
    '''
    if top_level is True:
        # Top level needs to look down into the resource dir.
        ind = ind.replace("TOC_PATH_SPEC","resources/TOC_FILE")
        ind = ind.replace("PAG_PATH_SPEC","resources/PAG_FILE")
    else:
        # All other module references (instances) come from someone who is already down in the resource dir.
        ind = ind.replace("TOC_PATH_SPEC","TOC_FILE")
        ind = ind.replace("PAG_PATH_SPEC","PAG_FILE")
    ind = ind.replace("MOD_NAME",mod_name)
    ind = ind.replace("TOC_FILE",toc_file_name)
    ind = ind.replace("PAG_FILE",page_file_name)
    ind = ind.replace("BASE_ADDRESS",str(base_address))
    return ind

def TOCDotJS():
    return '''
var _console = null;
var _num_ul = 0;

function is_netscape() {
  if (navigator.userAgent.indexOf("Netscape") != -1) {
    return true;
  } else {
    return false;
  }
}

document.writeln('<style>');

document.writeln('UL LI { display:block; margin-left: 0; }');

document.writeln('UL UL.showList LI { display:block; list-style-type:none; margin-left: 0.4em; }');
document.writeln('UL UL.defaultStyles LI { display:none; list-style-type:none; }');

document.writeln('INPUT.visible { border-width: 1px; border-color: #aaaaaa; font-family: monospace; background-color: #ffee55; color: #555555; font-weight: bold; margin: 1px; border-style: solid; padding: 0px; margin-left: 0; margin-bottom: 4px; }');

document.writeln('INPUT.highlighted { border-width: 1px; border-color: #000000; font-family: monospace; background-color: #dddddd; color: #000000; font-weight: bold; margin: 1px; border-style: solid; padding: 0px; margin-left: 0; margin-bottom: 4px; }');

document.writeln('INPUT.invisible {border-width: 1px; font-family: monospace; background-color: #ffffff; color: #ffffff; border-color: #ffffff; font-weight: bold; margin: 1px; border-style: solid; padding: 0px; margin-left: 0; margin-bottom: 4px; }');

document.writeln('</style>');

if (! is_netscape()) {
  document.writeln('<script>');
  document.writeln('function open_links(dl_id, button_id) {');
  document.writeln('  var node = document.getElementById(button_id);');
  document.writeln('  var node2 = document.getElementById(dl_id);');
  document.writeln('  if (node.value == "+") {');
  document.writeln('    node2.className="showList";');
  document.writeln('    node.value = "-";');
  document.writeln('  } else {');
  document.writeln('    node2.className="defaultStyles";');
  document.writeln('    node.value = "+";');
  document.writeln('  }');
  document.writeln('  window.status = "";');
  document.writeln('  return false;');
  document.writeln('}');
  document.writeln('</' + 'script>');
}

function mouseover(b) {
  b.className = 'highlighted';
  if (b.value == "+") {
    window.status = 'Click to open';
  } else {
    window.status = 'Click to close';
  }
  return true;
}

function mouseout(b) {
  b.className = 'visible';
  window.status = '';
}

function debug(msg)
{
  if ((_console == null) || (_console.closed)) {
    _console = window.open("","console","width=600,height=300,resizeable,scrollbars");
    _console.document.open("text/plain");
  }
  _console.focus();
  _console.document.writeln(msg);
}

function get_child_list(n) {
  var nxt = n.nextSibling;
  if (nxt && nxt.tagName == "UL") {
    return nxt;
  } else {
    // Internet explorer insists that everything between the LI tags is
    // a child whether you put in the </li> tag or not...
    var children = n.childNodes;
    if (children && children.length > 1 && children[1].tagName == "UL") {
      return children[1];
    } else {
      return null;
    }
  }
}

function make_button(n, cl) {
  if (cl) {
    var ulname = "u" + String(_num_ul);
    var bname = "b" + String(_num_ul);
    _num_ul = _num_ul + 1;

    var b = document.createElement("input");
    b.setAttribute("type", "button");
    b.setAttribute("id", bname);
    n.setAttribute("num_ul", _num_ul-1);     // NEW hold the num_ul in the UL so I can use it to call open_links
    b.setAttribute("value", "+");
    b.onclick = function() { open_links(ulname, bname); return false; };
    b.onmouseover = function() { mouseover(b); }
    b.onmouseout = function() { mouseout(b); }
    b.className='visible';
    n.insertBefore(b, n.firstChild);

    cl.setAttribute("id", ulname);
    cl.className = "defaultStyles";
  } else {
    var b = document.createElement("input");
    b.setAttribute("type", "button");
    b.setAttribute("value", " ");
    b.setAttribute("onclick", ";");
    b.className = 'invisible';
    n.insertBefore(b,n.firstChild);
  }
}

function reformat_toc_get(node) {
  var dts = node.getElementsByTagName("LI");
  for (var i = dts.length-1; i >= 0; i--) {
    var dt = dts[i];
    var cl = get_child_list(dt);
    var p = dt.parentNode;
    make_button(dt, cl);
  }
}

function reformat_toc() {
  // Netscape 4 leaves document.body undefined
  if (document.body && !is_netscape()) {
    var args = location.search;
    if (args != "?static") {
      _num_ul = 0;
      reformat_toc_get(document.body);

      if (_console) {
        _console.document.close();
      }
      document.body.normalize();

      // Find the top UL
      var dts = document.body.getElementsByTagName("LI");
      for (var i = dts.length-1; i >= 0; i--) {
          var dt = dts[i];
          var txt = dt["innerText" in dt ? "innerText" : "textContent"];

          // Is this the top entry "1 First Chapter"
          var re = /^1 /;
          if(re.test(txt)) {
            // Ok call the open_links
            var n_ul = dt.getAttribute("num_ul");
            open_links("u" + String(n_ul), "b" + String(n_ul));
          }
      }
    }
  }
}
    '''


def TOCDotCSS():
    return '''
/*
 * Styles for TOC.
 */

* {
    margin-left: -0.7em;
    padding-left: 0.4em;
    text-indent: 0.0em;
    margin-bottom: -0.3em;
}

h1,h2,h3,h4 {
    margin-top: 0.0em;
    margin-bottom: -.3em;
    white-space: nowrap;
    font-size:100%;
}

center {
    margin-top: -0.5em;
    margin-bottom: -0.7em;
    text-decoration: underline;
}

h1 {
    margin-top: 0.3em;
}

h2 {
    margin-left: 0.6em;
}

h3 {
    margin-left: 1.2em;
}

h4 {
    margin-left: 1.8em;
}

ul {
    margin-left: 0.0em;
    white-space: nowrap;
}

li ul {
    margin-left: -0.7em;
    white-space: nowrap;
}

li {
    margin-left: 0.0em;
    white-space: nowrap;
}

a {
    margin-left: 0.0em;
    text-decoration: none;
    white-space: nowrap;
}

a:hover {
    text-decoration: underline;
}
    '''


def TOCDotHTML( mod, doc, top_level ):
    if GenSingleFile:
        page_file_name = "page.html"
    else:
        page_file_name = getattr(mod,"m_HTMLFilePrefix","") + "page.html"
    toc = doc.toTOC(page_file_name)
    return toc


# Load the sp1 file and create a YModule.
#
TheModule = YodaLoader.YModule(YodaGenCL.InputFile())
SetHardSoftContext(TheModule)


# Construct the java script file.
#
toc_js_file = os.path.join( JobResourcesDir(), "toc.js" )
with open(toc_js_file,'wb') as fd:
    toc = TOCDotJS()
    enc = toc.encode("ascii","xmlcharrefreplace")
    fd.write(enc.rstrip()+b'\n')


# Construct the CSS file.
#
toc_css_file = os.path.join( JobResourcesDir(), "toc.css" )
with open(toc_css_file,'wb') as fd:
    toc = TOCDotCSS()
    enc = toc.encode("ascii","xmlcharrefreplace")
    fd.write(enc.rstrip()+b'\n')

TOCHeader = '''
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
<link rel="stylesheet" href="toc.css" type="text/css" media="screen"/>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
<title>Adice Documentation</title>
<style type="text/css">
  UL LI { display:block; list-style-type:none; }
</style>
<script src="toc.js" type="text/javascript">
</script>
</head>
<body onload="reformat_toc()">
<center><b>Table of Contents</b></center>
    '''

TOCTrailer = '''
</body>
</html>
    
'''

def complete_toc_file(toc_html_file):
    if os.path.exists(toc_html_file):
        tf = tempfile.NamedTemporaryFile(mode='w')
        with open(toc_html_file,'r') as fd:
            toc = fd.read()
            tf.write(TOCHeader)			# Give it a header.
            tf.write(toc)
            tf.write(TOCTrailer)		# Give it a trailer.
        tf.flush()
        copyfile(tf.name,toc_html_file)
        tf.close()
    else:
        print("File not found:",toc_html_file)


def clean_up():
    if GenSingleFile:
        for prefix in GblTOCOrder:
            if len(prefix) > 0:
                toc_file = os.path.join( JobResourcesDir(), prefix+"toc.html" )
                os.remove(toc_file)
                page_file = os.path.join( JobResourcesDir(), prefix+"page.html" )
                os.remove(page_file)
                index_file = os.path.join( JobResourcesDir(), prefix+"index.html" )
                os.remove(index_file)


def make_single_toc():
    tf_combined = tempfile.NamedTemporaryFile(mode='w')
    tf_combined.write(TOCHeader)			# Give it a header.
    for prefix in GblTOCOrder:
        toc_file_name = prefix + "toc.html"
        toc_html_file = os.path.join(JobResourcesDir(),toc_file_name)
        if os.path.exists(toc_html_file):
            with open(toc_html_file,'r') as fd:
                tf_combined.write(fd.read())
        else:
            print("File not found:",toc_html_file)
    tf_combined.write(TOCTrailer)			# Give it a trailer.
    tf_combined.flush()
    combined_toc_file = os.path.join(JobResourcesDir(),"toc.html")
    copyfile(tf_combined.name,combined_toc_file)
    tf_combined.close()
    #if YodaInfo.TestMode():
    #    with open(combined_toc_file,'r') as fd:
    #        contents = fd.read()
    #        print(contents)			# Print it to stdout for capture in the gold .out file.


def gen_flat_reg_table( d ):
    global GlobalAllRegistersFlat

    GlobalAllRegistersFlat = sorted( GlobalAllRegistersFlat, key=lambda x: x[0] )	# Sort by int addr.

    if len(GlobalAllRegistersFlat) > 0:
        head = HPara( style="H2", text="Flat Register Table" )
        flat = HSection(parent=d,style="sec",toc=HText("Flat Register Table"),head=head)
        tbl = HTable(parent=flat,style="Normal",border_width=2)
        header = HTableRow(parent=tbl,header=True)
        cell = HTableCell(text=""); header.addCell(cell)
        cell = HTableCell(text="Address"); header.addCell(cell)
        cell = HTableCell(text="Name"); header.addCell(cell)
        cell = HTableCell(text="Description"); header.addCell(cell)
        rnum = 1
        for tup in GlobalAllRegistersFlat:
            link_src = tup[4]
            row = HTableRow(parent=tbl)
            cell = HTableCell(text=str(rnum)); row.addCell(cell)
            cell = HTableCell(text=tup[1],tt=True); row.addCell(cell)
            cell = HTableCell(text=tup[2]); row.addCell(cell)
            cell.setLinkSrc(link_src)
            cell = HTableCell(text=tup[3]); row.addCell(cell)
            rnum += 1


def gen_files( depth, mod, base_address, top_level, inst, full_path_name ):
    # print("gen_files: ",mod.nameToUse()," @ ",base_address," ",VerilogAddress(base_address))
    # print("|{0}gen_files: module:{1}, base_address:{2}, top_level:{3}" \
    #     .format('_'*2*(depth-1),mod.nameToUse(),VerilogAddress(base_address),top_level))

    hdoc = None
    try:
        if depth == 1:
            # Make sure the top level goes first and gets dir #1 (without the _# suffix.)
            ModuleImageDirectory(mod)

        html_file_prefix = getattr(mod,"m_HTMLFilePrefix","")
        # print("html_file_prefix:",html_file_prefix)

        # Construct the page file name.
        html_page_file = os.path.join( JobResourcesDir(), html_file_prefix+"page.html" )

        separate_files = not GenSingleFile
        if depth == 1 or separate_files:
            global GlobalFinalHTMLPageFullPathName, GlobalFinalHTMLPageBaseName
            GlobalFinalHTMLPageFullPathName = html_page_file
            # Strip off the directory prefix to provide a relative path name so the HTML can be relocated.
            GlobalFinalHTMLPageBaseName = os.path.basename(GlobalFinalHTMLPageFullPathName)
            # print("Full:",GlobalFinalHTMLPageFullPathName)
            # print("Base:",GlobalFinalHTMLPageBaseName)

        # print("TOC Order:",full_path_name,html_file_prefix)
        GblTOCOrder.append(html_file_prefix)

        hdoc = gen_html_hdoc( depth, mod, base_address, inst, full_path_name )

        if depth == 1:
            gen_flat_reg_table(hdoc)

        # print("Convert to HTML string.",full_path_name,depth)
        if GenSingleFile:
            hdoc_s = hdoc.toString("",depth==1)
        else:
            hdoc_s = hdoc.toString("",True)

        # For output, encode non-ascii chars with their xml (hopefully html) entities. &#123
        hdoc_s = hdoc_s.encode("ascii","xmlcharrefreplace")

        #if YodaInfo.TestMode():
        if False:
            print(hdoc_s)				# Print it to stdout for capture in the gold .out file.
        else:
            # print("Open:",html_page_file)
            with open(html_page_file,'wb') as fd:
                fd.write(hdoc_s)			# Print it to the specified file.

        toc = TOCDotHTML(mod,hdoc,top_level)
        # if not GenSingleFile:
        toc = toc.encode("ascii","xmlcharrefreplace")
        toc_file_name = html_file_prefix + "toc.html"
        toc_file_dir = JobResourcesDir()
        toc_html_file = os.path.join(toc_file_dir,toc_file_name)
        with open(toc_html_file,'wb') as fd:
            fd.write(toc)				# Print it to the specified file.
        if not GenSingleFile:
            complete_toc_file(toc_html_file)
            # if YodaInfo.TestMode():
            if False:
                with open(toc_html_file,'r') as fd:
                    contents = fd.read()
                    print(contents)			# Print it to stdout for capture in the gold .out file.
                os.remove(toc_html_file)
        else:
            # Combined in to a single TOC file at the end.
            pass

        # Construct the index.html file.
        index_file_name = html_file_prefix + "index.html"
        index_file_dir = JobDir() if top_level else JobResourcesDir()
        index_full_path = os.path.join( index_file_dir, index_file_name )
        with open(index_full_path,'wb') as fd:
            ind = IndexDotHTML(depth,mod,base_address,top_level)
            enc = ind.encode("ascii","xmlcharrefreplace")
            fd.write(enc.rstrip()+b'\n')

        return index_file_name

    except Exception:				# as e:
        # print("*E*,",e)			Part of trace back.
        print(traceback.format_exc())
        YodaLoader._ErrorCount += 1


def TopRun( collect_coverage ):

    global GblCollectCoverage
    GblCollectCoverage = collect_coverage

    gen_files( 1, TheModule, TheModule.intAddress(), True, None, "" )

    if GenSingleFile:
        make_single_toc()

    clean_up()
