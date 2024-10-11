using ADIN.Device.Models;
using ADIN.Register.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public static class ADINConfirmBoard
    {
        private static List<string> AcceptedBoardNames = new List<string>()
        {
#if !DISABLE_T1L
            "EVAL-ADIN1100FMCZ",
            "EVAL-ADIN1100EBZ",
            "DEMO-ADIN1100-DIZ",
            "DEMO-ADIN1100D2Z",
            "EVAL-ADIN1110EBZ",
            "EVAL-ADIN2111EBZ",
            "EVAL-ADIN2111D1Z",
#endif

#if !DISABLE_TSN
            "ADIN1300 MDIO DONGLE",
            "ADIN1200 MDIO DONGLE",
#endif
            "EVAL-ADIN1320"
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
#if !DISABLE_T1L
                case "EVAL-ADIN1100FMCZ":
                    devices.Add(new ADINDevice(new ADIN1100Model(ftdtService, _registerService, mainLock)));
                    break;
                case "EVAL-ADIN1100EBZ":
                case "DEMO-ADIN1100-DIZ":
                case "DEMO-ADIN1100D2Z":
                    devices.Add(new ADINDevice(new ADIN1100Model(ftdtService, _registerService, mainLock), isMultiChipSupported));
                    if (isMultiChipSupported)
                        devices.Add(new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock, 4), isMultiChipSupported));
                    break;
                case "EVAL-ADIN1110EBZ":
                    devices.Add(new ADINDevice(new ADIN1110Model(ftdtService, _registerService, mainLock)));
                    break;
                case "EVAL-ADIN2111EBZ":
                    devices.Add(new ADINDevice(new ADIN2111Model(ftdtService, _registerService, 1, mainLock)));
                    devices.Add(new ADINDevice(new ADIN2111Model(ftdtService, _registerService, 2, mainLock)));
                    break;
#endif
#if !DISABLE_TSN
                case "ADIN1200 MDIO DONGLE":
                    devices.Add(new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock, 0)));
                    break;
                case "ADIN1300 MDIO DONGLE":
                    devices.Add(new ADINDevice(new ADIN1300Model(ftdtService, _registerService, mainLock)));
                    break;
#endif
                case "EVAL-ADIN1320":
                    devices.Add(new ADINDevice(new ADIN1320Model(ftdtService, _registerService, mainLock)));
                    break;
                default:
                    // No board matching the list
                    break;
            }

            return devices;
        }
    }
}
