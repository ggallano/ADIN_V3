using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ILoopback
    {
        LoopbackModel SelectedLoopback { get; set; }
        ObservableCollection<LoopbackModel> Loopbacks { get; set; }

        bool RxSuppression { get; set; }
        bool TxSuppression { get; set; }
        string ImagePath_RxSuppression { get; set; }
        string ImagePath_TxSuppression { get; set; }
    }
}
