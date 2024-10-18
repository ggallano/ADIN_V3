// <copyright file="ADINConfirmBoard.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

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

        public static List<ADINDevice> GetADINBoard(string boardName, IFTDIServices ftdtService, IRegisterService _registerService, object mainLock, bool isMultiChipSupported)
        {
            ADINFirmwareAPI fwAPI = new ADINFirmwareAPI(ftdtService, boardName);

            var adinChip = fwAPI.GetModelNum(0x1E0003, isMultiChipSupported);

            List<ADINDevice> devices = new List<ADINDevice>();

            foreach (var chip in adinChip)
            {
                switch (chip.ModelID)
                {
                    case 0x2: // ADIN1200
                        devices.Add(new ADINDevice(new ADIN1200Model(ftdtService, _registerService, mainLock, chip.PhyAddress), isMultiChipSupported));
                        break;

                    /* // Temporarily disabled to make ADIN1320 be used upon detection of ADIN1300 in the register read
                    case 0x3: // ADIN1300
                        devices.Add(new ADINDevice(new ADIN1300Model(ftdtService, _registerService, mainLock, chip.PhyAddress)));
                        break; */

                    case 0x3: // ADIN1320
                        devices.Add(new ADINDevice(new ADIN1320Model(ftdtService, _registerService, mainLock, chip.PhyAddress)));
                        break;
                    case 0x6: // ADIN1320
                        break;
                    case 0x8: // ADIN1100
                        devices.Add(new ADINDevice(new ADIN1100Model(ftdtService, _registerService, chip.PhyAddress, mainLock), isMultiChipSupported));
                        break;
                    case 0x9: // ADIN1110
                        devices.Add(new ADINDevice(new ADIN1110Model(ftdtService, _registerService, chip.PhyAddress, mainLock)));
                        break;
                    case 0xA: // ADIN2111
                        devices.Add(new ADINDevice(new ADIN2111Model(ftdtService, _registerService, chip.PortNum, chip.PhyAddress, mainLock)));
                        break;
                    default:
                        break;
                }
            }

            return devices;
        }
    }
}
