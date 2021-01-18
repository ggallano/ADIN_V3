import logging
import os
import re
import json
from collections import OrderedDict
from lxml import etree
from lxml import objectify
from shutil import copyfile
import os 
from os import listdir

re_value = re.compile(r"^\s*(?P<width>\d+)*'(?P<base>\w)(?P<value>\w+)")

logger = logging.getLogger(os.path.basename(__file__))


class YodaAccessObject(object):
    def __init__(self):
        pass

    def __str__(self):
        if hasattr(self, "RegOffset") and hasattr(self, "SliceWidth"):
            # This is a bitfield in a register
            access_string = "%s[%d:%d]" % (self.RegisterName,
                                           self.RegOffset +
                                           self.SliceWidth - 1,
                                           self.RegOffset)
            access_string += " {%s}" % self.BitFieldName
        else:
            # This is a full register
            access_string = "%s[%d:0]" % (self.RegisterName,
                                          self.Width - 1)

        return access_string

    def is_bitfield(self):
        """
        Return true if this defines a bitfield (i.e. something that
        is smaller than the full 16 bit register content)
        :return:
        """
        return hasattr(self, "RegOffset") and hasattr(self, "SliceWidth")

    def extract_field_value(self, reg_contents):
        """
        Return the value of the specified sub-field
        :param reg_contents: The full register contents
        :return: Sub field value
        """
        if self.is_bitfield():
            mask = (1 << self.SliceWidth) - 1
            field_contents =  (reg_contents >> self.RegOffset) & mask
        else:
            field_contents = reg_contents

        return field_contents

    def insert_value(self, reg_contents, field_value):
        """

        :param reg_contents:
        :param field_value:
        :return:
        """
        if self.is_bitfield():
            mask = (1 << self.SliceWidth) - 1
            reg_contents &= ~(mask << self.RegOffset)
            reg_contents |= (field_value & mask) << self.RegOffset
        else:
            reg_contents = field_value

        return reg_contents


def load_yoda_file(yodafile):
    """
    Load and validate the output file, and return it
    :return:
    """

    if hasattr(yodafile, "seek"):  # Can pass in a file object also
        yodafile.seek(0)  # reset to start

    vparser = objectify.makeparser()
    try:
        yoda_file = objectify.parse(yodafile, vparser).getroot()
    except etree.XMLSyntaxError as e:
        name = yodafile
        if hasattr(yodafile, "name"):  # Can pass in a file object also
            name = yodafile.name
        raise RuntimeError("Output file '%s' contained an error [%s] " % (name, e))

    return yoda_file

def export_json_file(filename,directory,src_img, img_dst):


    print("Exporting")


    json_file = {}


    registers = []

    json_file["Registers"] = registers

    
    print("*** del del del ***")
    folder_path = "../ADIN1300-Eval/Images/Yoda/%s/" % img_dst
    for file_name in listdir(folder_path):
        if file_name.endswith('.png'):
            logging.info("Removing '%s'" % file_name)
            os.remove(folder_path + file_name)
    # Get all the register names first
    for Module in __yoda_files:
        for RegisterMap in Module.MemoryMap.RegisterMaps.RegisterMap:
                # Search the registers
                for Register in RegisterMap.Registers.Register:
                    print(Register.Name)
                    if str(Register.Visibility) != "Private":
                        src = "%sMdioMap_%s/resources/images/MdioMap_%s/%s.png" % (src_img,RegisterMap.Name,RegisterMap.Name,str(Register.UID))
                        dst = "../ADIN1300-Eval/Images/Yoda/%s/%s.png" % (img_dst,str(Register.UID))
                        logging.info("%s -> %s" % (src,dst))
                        copyfile(src, dst)

                        register = OrderedDict()
                        register["IncludeInDump"] = False
                        # assert str(Register.SWName) == str(Register.Name), "%s %s" % (Register.SWName,Register.Name)
                        if str(Register.SWName) != str(Register.Name): 
                            logging.warning("%s %s" % (Register.SWName,Register.Name))
                        register["Name"] = str(Register.SWName)
                        register["Address"] = get_value(Register.Address)
                        register["ResetValue"] = get_value(Register.ResetValue)
                        register["MMap"] = str(RegisterMap.Name)
                        register["Documentation"] = str(Register.SWDescription)
                        register["Visibility"] = str(Register.Visibility)
                        register["Image"] = directory + "/" + str(Register.UID) + ".png"
                        register["Access"] = "R"
                        register["Group"] = "Global"
                        if hasattr(Register, "BitFieldRefs"):
                            register["BitFields"] = []


                            for RegisterBitFieldRef in Register.BitFieldRefs.BitFieldRef:
                                insert_bf = OrderedDict()
                                for BitField in Module.MemoryMap.BitFields.BitField:
                                    # print(BitFieldRef)
                                    if BitField["UID"] == RegisterBitFieldRef["BF-UID"]:
                                        insert_bf["IncludeInDump"] = False
                                        if str(BitField.Visibility) != "Private":
                                            insert_bf["Name"] = str(BitField["SWName"])
                                            # assert str(BitField.SWName) == str(BitField.Name)
                                            if str(BitField.SWName) != str(BitField.Name):
                                                logging.warning("%s %s" % (BitField.SWName,BitField.Name))
                                            insert_bf["IncludeInDump"] = False
                                        else:
                                            insert_bf["Name"] = "RESERVED"

                                        if insert_bf["Name"] == "RESERVED":
                                            insert_bf["IncludeInDump"] = False

                                        insert_bf["Start"] = get_value(RegisterBitFieldRef.RegOffset)
                                        insert_bf["Width"] = get_value(RegisterBitFieldRef.SliceWidth)
                                        insert_bf["ResetValue"] = get_value(BitField.DefaultValue)
                                        insert_bf["Access"] = str(BitField.Access)
                                        if str(BitField.Access) != "R":
                                            register["Access"] = "R/W"

                                        insert_bf["MMap"] = str(RegisterMap.Name)
                                        insert_bf["Value"] = get_value(BitField.DefaultValue)
                                        if str(BitField.Visibility) != "Private":
                                            insert_bf["Documentation"] = str(BitField.SWDescription)
                                        else:
                                            insert_bf["Documentation"] = "None"
                                        insert_bf["Visibility"] = str(BitField.Visibility)
                                        print("    " + BitField.Name)
                                        break
                                register["BitFields"].append(insert_bf)
                        registers.append(register)


    # Now get all the bitfields in those registers

    with open(filename,"wt") as f:
        f.write(json.dumps(json_file, sort_keys=False, indent=4))


def get_value(yodavaluestring):
    """
    Convert a string indicating a value from the yoda
    file to an actual number
    :param yodavaluestring:
    :return:
    """
    bases = {"b": 2, "d": 10, "h": 16}
    match = re_value.match(str(yodavaluestring))
    if match:
        results = match.groupdict()
        logger.debug(results)
        value = int(results["value"], bases[results["base"]])
    else:
        # Assume plain base 10 string
        value = int(str(yodavaluestring), 10)
    return value


def get_node(mmap, reg_or_bit):
    details = []

    bf_uid = None

    for Module in __yoda_files:
        for RegisterMap in Module.MemoryMap.RegisterMaps.RegisterMap:
            if RegisterMap.Name == mmap:
                # Search the bitfields
                for BitField in Module.MemoryMap.BitFields.BitField:
                    if BitField.Name == reg_or_bit:
                        details.append(BitField)
                        bf_uid = BitField.UID
                        break

                # Search the registers
                for Register in RegisterMap.Registers.Register:
                    if bf_uid:
                        # We are searching for the register
                        # that the bitfield is in
                        if hasattr(Register, "BitFieldRefs"):
                            for BitFieldRef in Register.BitFieldRefs.BitFieldRef:
                                if bf_uid == BitFieldRef["BF-UID"]:
                                    details.append(BitFieldRef)
                                    details.append(Register)
                                    break
                    else:
                        # We are still searching for the name
                        # that was passed in. Assumed to be a
                        # register at this point
                        if Register.Name == reg_or_bit:
                            details.append(Register)

    return details

__yoda_files = []

def process_yoda_files(yoda_files):
    global __yoda_files
    __yoda_files = []
    for yda in yoda_files:
        __yoda_files.append(load_yoda_file(yda))


def getYodaDetails(mmap, reg_or_bit):
    """
    Get details of specified register or bitfield

    :param mmap: Name of memory map that the register is in
    :param reg_or_bit: Register or Bifield name to access in the memory map
    :param value: Value to write to register or bitfield
    :return:
    """
    logger.debug("getYodaDetails(mmap='%s', reg_or_bit='%s')" % (mmap, reg_or_bit))

    reg_or_bit_details = get_node(mmap, reg_or_bit)

    # If the element is a register access, then we will get
    # a single item returned.
    # [0] Register
    # If the element is a bitfield, we will get
    # [0] BitField
    # [1] BitfieldRef
    # [2] Register
    if len(reg_or_bit_details) == 1:
        assert reg_or_bit_details[0].tag == "Register"
    else:
        if len(reg_or_bit_details) == 3:
            assert reg_or_bit_details[0].tag == "BitField", "'%s' is not expected here" % reg_or_bit_details[0].tag
            assert reg_or_bit_details[1].tag == "BitFieldRef", "'%s' is not expected here" % reg_or_bit_details[1].tag
            assert reg_or_bit_details[2].tag == "Register", "'%s' is not expected here" % reg_or_bit_details[2].tag
        else:
            raise ValueError("mmap='%s', reg_or_bit='%s' could not be resolved" % (mmap, reg_or_bit))

    access_detail = YodaAccessObject()
    interesting = ["Address", "BitOffset", "RegOffset", "SliceWidth", "Width"]
    for detail in reg_or_bit_details:
        if hasattr(detail, "Name"):
            logger.debug("%s %s" % (detail.tag, detail.Name))

        for item in interesting:
            if hasattr(detail, item):
                setattr(access_detail, item, get_value(detail[item]))

                logger.debug("%12s : 0x%06X" % (item, get_value(detail[item])))
                if item == "BitOffset":
                    # This is the start of slice. We only expect this to be zero
                    assert access_detail.BitOffset == 0, "This should always be 0"
                if item == "SliceWidth":
                    # The slicewidth is expected to be the full portion of the bitfield
                    assert access_detail.SliceWidth == access_detail.Width, "This should always be the same"

        if detail.tag == "Register":
            assert access_detail.Width == 16, "%d" % access_detail.Width
        if hasattr(detail, "Name"):
            setattr(access_detail, "%sName" % detail.tag, str(detail.Name))

    logging.debug(access_detail)
    return access_detail
