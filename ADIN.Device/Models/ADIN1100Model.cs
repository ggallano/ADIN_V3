using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ADIN1100Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;

        public ADIN1100Model(IFTDIServices ftdiService)
        {
            _ftdiService = ftdiService;
            FirmwareAPI = new ADIN1100FirmwareAPI(_ftdiService);
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
