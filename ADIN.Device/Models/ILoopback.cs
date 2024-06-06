using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ILoopback
    {
        LoopbackModel SelectedLoopback { get; set; }
        List<LoopbackModel> Loopbacks { get; set; }

        bool RxSuppression { get; set; }
        bool TxSuppression { get; set; }
    }
}
