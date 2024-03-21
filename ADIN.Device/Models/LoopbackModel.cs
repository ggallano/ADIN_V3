using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class LoopbackModel
    {
        public LoopbackModel()
        {
            Loopbacks = new List<LoopbackListingModel>();
        }

        public LoopbackListingModel Loopback { get; set; }
        public List<LoopbackListingModel> Loopbacks { get; set; }
    }
}