// <copyright file="ADINFirmwareAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

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
        private Dictionary<int, uint> adinChipPresent;
        private string response = string.Empty;
        private uint modelNum = 0;

        public ADINFirmwareAPI(IFTDIServices ftdtService)
        {
            _ftdiService = ftdtService;
            adinChipPresent = new Dictionary<int, uint>();
        }

        public Dictionary<int, uint> GetModelNum(uint regAddress)
        {
            uint phyAddress = 0;

            CheckT1LChip();
            CheckGeChip();

            return adinChipPresent;
        }

        private void CheckT1LChip()
        {
            for (int phyAddress = 0; phyAddress < 7; phyAddress++)
            {
                var command = $"mdiord_cl45 {phyAddress},0x1E0003\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                    continue;

                modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                if (modelNum == 0x08)
                {
                    adinChipPresent.Add(phyAddress, modelNum);
                }
            }
        }

        private void CheckGeChip()
        {
            for (int phyAddress = 0; phyAddress < 7; phyAddress++)
            {
                var command = $"mdiowrite {phyAddress},0x10,0x1E0003\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                    continue;

                var command2 = $"mdioread {phyAddress},11\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command2);

                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                    continue;

                modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                if (modelNum == 0x02 || modelNum == 0x03)
                {
                    adinChipPresent.Add(phyAddress, modelNum);
                }
            }
        }
    }
}
