// <copyright file="ADINFirmwareAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public class ADINFirmwareAPI
    {
        private IFTDIServices _ftdiService;
        private List<ADINChip> adinChipPresent;
        private string response = string.Empty;
        private uint modelNum = 0;
        private string boardName = string.Empty;

        public ADINFirmwareAPI(IFTDIServices ftdtService, string boardName)
        {
            _ftdiService = ftdtService;
            this.boardName = boardName;
            adinChipPresent = new List<ADINChip>();
        }

        public List<ADINChip> GetModelNum(uint regAddress)
        {
            uint phyAddress = 0;


            switch (boardName)
            {
                case "EVAL-ADIN1100FMCZ":
                    break;
                case "EVAL-ADIN1100EBZ":
                case "DEMO-ADIN1100-DIZ":
                case "DEMO-ADIN1100D2Z":
                    Clause22CheckModelNum();
                    break;
                case "EVAL-ADIN1110EBZ":
                    break;
                case "EVAL-ADIN2111EBZ":
                case "EVAL-ADIN2111D1Z":
                    break;
                case "ADIN1300 MDIO DONGLE":
                case "ADIN1200 MDIO DONGLE":
                    Clause22CheckModelNum();
                    break;
                default:
                    break;
            }

            return adinChipPresent;
        }

        //private void PhyReadCheckModelNum()
        //{
        //    for (int phyAddress = 0; phyAddress < 7; phyAddress++)
        //    {
        //        var command = $"mdiord_cl45 {phyAddress},0x1E0003\n";

        //        _ftdiService.Purge();
        //        _ftdiService.SendData(command);

        //        response = _ftdiService.ReadCommandResponse().Trim();

        //        if (response.Contains("ERROR"))
        //            continue;

        //        modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

        //        Debug.WriteLine($"Command:{command.TrimEnd()}");
        //        Debug.WriteLine($"Response:{response}");

        //        if (modelNum == 0x08)
        //        {
        //            adinChipPresent.Add(phyAddress, modelNum);
        //        }
        //    }
        //}

        private void Clause22CheckModelNum()
        {
            for (int phyAddress = 0; phyAddress < 7; phyAddress++)
            {
                //var command = $"mdiowrite {phyAddress},0x10,0x1E0003\n";

                //_ftdiService.Purge();
                //_ftdiService.SendData(command);

                //response = _ftdiService.ReadCommandResponse().Trim();

                //if (response.Contains("ERROR"))
                //    continue;

                var command2 = $"mdioread {phyAddress},0x1E0003\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command2);

                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                    continue;

                modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

                Debug.WriteLine($"Command:{command2.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                if (modelNum == 0x02 || modelNum == 0x03 || modelNum == 0x06 || modelNum == 0x08)
                {
                    adinChipPresent.Add(new ADINChip() { PhyAddress = phyAddress, ModelID = modelNum };
                }
            }
        }
    }
}
