using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1200
{
    public class LoopbackADIN1200 : ILoopback
    {
        public LoopbackADIN1200()
        {
            LpBck_None = new LoopbackListingModel();
            LpBck_None.Name = "OFF";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            LpBck_None.TxSuppression = true;
            LpBck_None.RxSuppression = false;

            LpBck_Digital = new LoopbackListingModel();
            LpBck_Digital.Name = "Digital";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;

            LpBck_LineDriver = new LoopbackListingModel();
            LpBck_LineDriver.Name = "LineDriver";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;

            LpBck_ExtCable = new LoopbackListingModel();
            LpBck_ExtCable.Name = "ExtCable";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;

            LpBck_Remote = new LoopbackListingModel();
            LpBck_Remote.Name = "Remote";
            LpBck_Remote.EnumLoopbackType = LoopBackMode.MacRemote;

            Loopbacks = new List<LoopbackListingModel>()
            {
                LpBck_None,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable,
                LpBck_Remote
            };
            Loopback = Loopbacks[0];
        }

        public LoopbackListingModel LpBck_None { get; set; }
        public LoopbackListingModel LpBck_Digital { get; set; }
        public LoopbackListingModel LpBck_LineDriver { get; set; }
        public LoopbackListingModel LpBck_ExtCable { get; set; }
        public LoopbackListingModel LpBck_Remote { get; set; }

        public LoopbackListingModel Loopback { get; set; }
        public List<LoopbackListingModel> Loopbacks { get; set; }
    }
}
