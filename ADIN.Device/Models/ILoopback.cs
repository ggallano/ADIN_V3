using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ILoopback
    {
        LoopbackListingModel SelectedLoopback { get; set; }
        List<LoopbackListingModel> Loopbacks { get; set; }
    }
}
