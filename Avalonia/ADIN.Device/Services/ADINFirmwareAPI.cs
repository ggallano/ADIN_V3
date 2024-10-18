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
            adinChipPresent = new List<ADINChip>();
            this.boardName = boardName;
        }

        public List<ADINChip> GetModelNum(uint regAddress, bool isMultiChipSupported)
        {
            switch (boardName)
            {
#if !DISABLE_T1L
                case "EVAL-ADIN1100FMCZ":
                    Cl45CheckModelNum();
                    break;
                case "EVAL-ADIN1100EBZ":
                case "DEMO-ADIN1100-DIZ":
                case "DEMO-ADIN1100D1Z":
                case "DEMO-ADIN1100D2Z":
                    Cl22CheckModelNum();
                    if (!isMultiChipSupported)
                        adinChipPresent.RemoveAll(x => x.ModelID != 0x08);
                    break;
                case "EVAL-ADIN1110EBZ":
                    PhyReadCheckModelNum();
                    break;
                case "EVAL-ADIN2111EBZ":
                case "EVAL-ADIN2111D1Z":
                    PhyReadCheckModelNum(true);
                    break;
#endif
#if !DISABLE_TSN
                case "ADIN1300 MDIO DONGLE":
                case "ADIN1200 MDIO DONGLE":
                    Cl22CheckModelNum();
                    break;
#endif
                default:
                    break;
            }

            return adinChipPresent;
        }

        private void PhyReadCheckModelNum(bool hasPort = false)
        {
            if (!hasPort)
            {
                var command = $"phyread 0x1E0003\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                    return;

                modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                if (modelNum == 0x09)
                {
                    adinChipPresent.Add(new ADINChip() { PhyAddress = -1, ModelID = modelNum, PortNum = -1 });
                }
            }
            else
            {
                for (int portNumber = 1; portNumber < 3; portNumber++)
                {
                    var command = $"phyread {portNumber},0x1E0003\n";

                    _ftdiService.Purge();
                    _ftdiService.SendData(command);

                    response = _ftdiService.ReadCommandResponse().Trim();

                    if (response.Contains("ERROR"))
                        continue;

                    modelNum = (Convert.ToUInt32(response, 16) & 0x3F0) >> 4;

                    Debug.WriteLine($"Command:{command.TrimEnd()}");
                    Debug.WriteLine($"Response:{response}");

                    if (modelNum == 0x0A)
                    {
                        adinChipPresent.Add(new ADINChip() { PhyAddress = -1, ModelID = modelNum, PortNum = portNumber });
                    }
                }
            }
        }

        private void Cl22CheckModelNum()
        {
            for (int phyAddress = 0; phyAddress < 7; phyAddress++)
            {
                var command2 = $"mdioread {phyAddress},0x1e0003\n";

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
                    adinChipPresent.Add(new ADINChip() { PhyAddress = phyAddress, ModelID = modelNum, PortNum = -1 });
                }
            }
        }

        private void Cl45CheckModelNum()
        {
            for (int phyAddress = 0; phyAddress < 7; phyAddress++)
            {
                var command2 = $"mdiord_cl45 {phyAddress},0x1e0003\n";

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
                    adinChipPresent.Add(new ADINChip() { PhyAddress = phyAddress, ModelID = modelNum, PortNum = -1 });
                }
            }
        }
    }
}
