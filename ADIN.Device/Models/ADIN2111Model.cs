using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ADIN2111Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;

        public ADIN2111Model(IFTDIServices  ftdiService)
        {
            _ftdiService = ftdiService;
            FirmwareAPI = new ADIN2111FirmwareAPI(_ftdiService);
        }
        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
