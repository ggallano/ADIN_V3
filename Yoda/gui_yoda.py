from helper import change_logging_file
import YodaHelper
import logging
import os
from shutil import copyfile
change_logging_file("runner.log")

logger = logging.getLogger(os.path.basename(__file__))

logger.info("Reading Yoda files.")
with open("MdioMap_GESubsys_1300.sp1", "r") as yda1, open("MdioMap_GEPhy_1300.sp1", "r") as yda2:
    YodaHelper.process_yoda_files([yda1, yda2])
logger.info("Yoda file processing done.")

YodaHelper.export_json_file("../DeviceCommunication/registers_adin1300.json","adin1300",src_img="H:/temp/10SPE/1300_Images/design/html2/",img_dst="adin1300")
copyfile("../DeviceCommunication/registers_adin1300.json", "../DeviceCommunication/registers_adin1301.json")


logger.info("Reading Yoda files.")
with open("MdioMap_GESubsys_1200.sp1", "r") as yda3, open("MdioMap_GEPhy_1200.sp1", "r") as yda4:
    YodaHelper.process_yoda_files([yda3, yda4])
logger.info("Yoda file processing done.")

YodaHelper.export_json_file("../DeviceCommunication/registers_adin1200.json","adin1200",src_img="H:/temp/10SPE/1200_Images/design/html2/",img_dst="adin1200")

logger.info("Reading Yoda files.")
with open("MdioMap_SPEPhy.sp1", "r") as yda5:
    YodaHelper.process_yoda_files([yda5])
logger.info("Yoda file processing done.")

YodaHelper.export_json_file("../DeviceCommunication/registers_adin1100.json","adin1100",src_img="H:/temp/10SPE/1100_Images/design/html2/",img_dst="adin1100")