using ADI.Register.Services;
using ADIN.Device.Models;
using FTDIChip.Driver.Services;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;

namespace ADIN.Device.Services
{
    public static class ADINConfirmBoard
    {
        private static List<string> AcceptedBoardNames = new List<string>()
        {
            "EVAL-ADIN1100FMCZ",

            "EVAL-ADIN1100EBZ",
            "DEMO-ADIN1100-DIZ",
            "DEMO-ADIN1100D2Z",

            "EVAL-ADIN1110EBZ",
            "EVAL-ADIN2111EBZ",

            "ADIN1300 MDIO DONGLE",
            "ADIN1200 MDIO DONGLE",
        };

        public static bool ConfirmADINBoard(string boardName)
        {
            if (AcceptedBoardNames.Contains(boardName))
                return true;

            return false;
        }

        public static List<ADINDevice> GetADINBoard(string BoardName, IFTDIServices ftdtService, IRegisterService _registerService, object mainLock, bool isMultiChipSupported)
        {
            List<ADINDevice> devices = new List<ADINDevice>();

            switch (BoardName)
            {
                case "EVAL-ADIN1100FMCZ":
                    devices.Add(new ADINDevice(new ADIN1100Model(ftdtService, _registerService, mainLock)));
                    break;
                case "EVAL-ADIN1100EBZ":
                case "DEMO-ADIN1100-DIZ":
                case "DEMO-ADIN1100D2Z":
                    devices.Add(new ADINDevice(new ADIN1100Model(ftdtService, _registerService, mainLock), isMultiChipSupported));
                    if (isMultiChipSupported)
                        devices.Add(new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock), isMultiChipSupported));
                    break;
                case "ADIN1200 MDIO DONGLE":
                    devices.Add(new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock)));
                    break;
                case "ADIN1300 MDIO DONGLE":
                    devices.Add(new ADINDevice(new ADIN1300Model(ftdtService, _registerService, mainLock)));
                    break;
                case "EVAL-ADIN1110EBZ":
                    devices.Add(new ADINDevice(new ADIN1110Model(ftdtService, _registerService, mainLock)));
                    break;
                case "EVAL-ADIN2111EBZ":
                    devices.Add(new ADINDevice(new ADIN2111Model(ftdtService, _registerService, mainLock)));
                    break;
                default:
                    // No board matching the list
                    break;
            }

            return devices;
        }
    }
}