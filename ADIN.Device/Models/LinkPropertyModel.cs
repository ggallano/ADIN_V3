using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class LinkPropertyModel
    {
        public LinkPropertyModel()
        {
            AutoNegMasterSlaveAdvertisements = new List<AutoNegMasterSlaveAdvertisementModel>();
            AutoNegTxLevelAdvertisements = new List<AutoNegTxLevelAdvertisementModel>();
        }

        public AutoNegMasterSlaveAdvertisementModel AutoNegMasterSlaveAdvertisement { get; set; }
        public List<AutoNegMasterSlaveAdvertisementModel> AutoNegMasterSlaveAdvertisements { get; set; }
        public AutoNegTxLevelAdvertisementModel AutoNegTxLevelAdvertisement { get; set; }
        public List<AutoNegTxLevelAdvertisementModel> AutoNegTxLevelAdvertisements { get; set; }
    }
}