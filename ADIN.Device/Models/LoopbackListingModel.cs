using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class LoopbackListingModel
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public bool RxSuppression { get; set; }
        public bool TxSuppression { get; set; }
        public LoopBackMode EnumLoopbackType { get; set; }
    }
}
