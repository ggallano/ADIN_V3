# <Gen>
#   <Description>HTML Generator (Version 2).</Description>
#   <XMLFormat>Simple1</XMLFormat>
#   <Alias>HTML_NEW</Alias>
#   <Alias>HTML2</Alias>
#   <Interpreter>python</Interpreter>
#   <InterpreterVersion>3.7.3</InterpreterVersion>
#   <Author>
#     <Name>Bill Crocker</Name>
#     <Phone>6500-1382</Phone>
#     <EMail>william.crocker@analog.com</EMail>
#   </Author>
#   <Documentation><![CDATA[
#     Generates a browsable hardware (or software) view of the subject Yoda module.
#     Output is written to output2/design/html2/<modname> and the root file is index.html.
#   ]]></Documentation>
#   <ConfigUI>
#     <Group>
#       <Name>HTML</Name>
#       <Label>HTML generator configuration</Label>
#       <ToolTip>Options for the HTML generator.</ToolTip>
#
#       <CheckBox>
#         <Name>GenSingleFile</Name>
#         <Label>Generate a single file</Label>
#         <DefaultValue>False</DefaultValue>
#         <ToolTip>Generate one big HTML file as opposed
#                  to a separate file for each instance.</ToolTip>
#       </CheckBox>
#
#       <CheckBox>
#         <Name>GenImages</Name>
#         <Label>Generate Images</Label>
#         <DefaultValue>False</DefaultValue>
#         <ToolTip>Generate object images
#                  and add them to the HTML.</ToolTip>
#       </CheckBox>
#
#       <TextBox>
#         <Name>MaxRegPartWidth</Name>
#         <Label>Max register width</Label>
#         <DefaultValue>16</DefaultValue>
#         <ToolTip>Maximum register segment bit-width
#                  when broken across multiple lines.</ToolTip>
#         <Indent>20</Indent>
#         <Visibility>../GenImages</Visibility>
#       </TextBox>
#
#       <TextBox>
#         <Name>Subdirectory</Name>
#         <Label>Subdirectory</Label>
#         <DefaultValue>html2</DefaultValue>
#         <ToolTip>Output subdirectory (under the .../design directory).</ToolTip>
#       </TextBox>
#
#       <Group>
#         <Name>RegTableStyle</Name>
#         <Label>Register summary table style</Label>
#         <Orientation>Horizontal</Orientation>
#         <ToolTip>Style of the register summary table.</ToolTip>
#         <CheckBox>
#           <Name>Full</Name>
#           <Label>Full table</Label>
#           <InitialValue>False</InitialValue>
#           <RadioSet>SummaryTableType</RadioSet>
#           <ToolTip>Create a full summary table
#                    with bit field columns.</ToolTip>
#         </CheckBox>
#         <CheckBox>
#           <Name>Simple</Name>
#           <Label>Simplified table</Label>
#           <InitialValue>True</InitialValue>
#           <RadioSet>SummaryTableType</RadioSet>
#           <ToolTip>Create a simplified summary table without bit
#                    field columns. This format also adds a register
#                    description column.</ToolTip>
#         </CheckBox>
#       </Group>		<!-- Style of the summary table. -->
#
#       <Group>
#         <Name>HardSoftContext</Name>
#         <Label>Context</Label>
#         <Orientation>Horizontal</Orientation>
#         <ToolTip>Select a Hardware or Software context.</ToolTip>
#         <CheckBox>
#           <Name>Hardware</Name>
#           <Label>Hardware</Label>
#           <InitialValue>True</InitialValue>
#           <RadioSet>HardSoftContext</RadioSet>
#           <ToolTip>Generate a view in the Hardware context.</ToolTip>
#         </CheckBox>
#         <CheckBox>
#           <Name>Software</Name>
#           <Label>Software</Label>
#           <InitialValue>False</InitialValue>
#           <RadioSet>HardSoftContext</RadioSet>
#           <ToolTip>Generate a view in the Software context.
#                    Set a proper DocumentationSetToUse property
#                    on your top level module !</ToolTip>
#         </CheckBox>
#       </Group>                <!-- Hardware or software context. -->
#     </Group>
#   </ConfigUI>
# </Gen>

from __future__ import print_function

import os
import sys

sys.path.insert( 0, os.path.join(os.path.dirname(__file__),'..','common') )
import YodaLoader

# Coverage requires use of Python 3.
collect_coverage = 0
var_name = "RUNNING_GEN_PYTHON_COVERAGE"
if var_name in os.environ and os.environ[var_name]=="1":
    collect_coverage = 1
if collect_coverage == 1:
    import coverage
    cov = coverage.coverage(auto_data=True,timid=True)
    cov.start()

import html2_core
html2_core.TopRun( collect_coverage )

# def dump2(TheModule):
#     for i in range(1,100):
#         dump(TheModule)
# profile.run('dump2(TheModule)','profile.dat')
# p = pstats.Stats('profile.dat')
# p.sort_stats('time').print_stats(100)
# p.print_callees()

if collect_coverage == 1:
    cov.stop()
    cov.save()

if YodaLoader.ErrorCount() == 0:
    print("*I* generator exits normally.")
    exit(0)
else:
    print("*W* generator exits with errors.")
    exit(1)
