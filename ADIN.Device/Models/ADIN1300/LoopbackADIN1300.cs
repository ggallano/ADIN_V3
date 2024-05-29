using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1300
{
    public class LoopbackADIN1300 : ILoopback
    {
        public LoopbackADIN1300()
        {
            LpBck_None = new LoopbackListingModel();
            LpBck_None.Name = "OFF";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            LpBck_None.ImagePath = @"../Images/loopback/loopback.png";

            LpBck_Digital = new LoopbackListingModel();
            LpBck_Digital.Name = "Digital";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            LpBck_Digital.ImagePath = @"../Images/loopback/loopback.png";

            LpBck_LineDriver = new LoopbackListingModel();
            LpBck_LineDriver.Name = "LineDriver";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            LpBck_LineDriver.ImagePath = @"../Images/loopback/loopback.png";

            LpBck_ExtCable = new LoopbackListingModel();
            LpBck_ExtCable.Name = "ExtCable";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            LpBck_ExtCable.ImagePath = @"../Images/loopback/loopback.png";

            LpBck_Remote = new LoopbackListingModel();
            LpBck_Remote.Name = "Remote";
            LpBck_Remote.EnumLoopbackType = LoopBackMode.MacRemote;
            LpBck_Remote.ImagePath = @"../Images/loopback/loopback.png";

            Loopbacks = new List<LoopbackListingModel>()
            {
                LpBck_None,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable,
                LpBck_Remote
            };

            SelectedLoopback = Loopbacks[0];
            SelectedLoopback.TxSuppression = true;
            SelectedLoopback.RxSuppression = false;
        }

        public LoopbackListingModel LpBck_None { get; set; }
        public LoopbackListingModel LpBck_Digital { get; set; }
        public LoopbackListingModel LpBck_LineDriver { get; set; }
        public LoopbackListingModel LpBck_ExtCable { get; set; }
        public LoopbackListingModel LpBck_Remote { get; set; }

        public LoopbackListingModel SelectedLoopback { get; set; }
        public List<LoopbackListingModel> Loopbacks { get; set; }
    }
}
