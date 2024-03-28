using ADIN.Device.Models.ADIN1300;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ADIN1300Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private ILinkProperties linkProperties;

        public ADIN1300Model(IFTDIServices ftdiService)
        {
            _ftdiService = ftdiService;
            FirmwareAPI = new ADIN1300FirmwareAPI(_ftdiService);

            LinkProperties = new LinkPropertiesADIN1300();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }

        public ILinkProperties LinkProperties
        {
            get { return linkProperties; }
            set { linkProperties = value; }
        }
    }
}
