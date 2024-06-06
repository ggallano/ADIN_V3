using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1100
{
    public class LoopbackADIN1100 : ILoopback
    {
        public LoopbackADIN1100()
        {
            LoopbackListingModel LpBck_None = new LoopbackListingModel();
            LpBck_None.Name = "None";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            LpBck_None.ImagePath = @"../Images/loopback/NoLoopback.png";

            LoopbackListingModel LpBck_MacRemote = new LoopbackListingModel();
            LpBck_MacRemote.Name = "MAC I/F Remote";
            LpBck_MacRemote.EnumLoopbackType = LoopBackMode.MacRemote;
            LpBck_MacRemote.ImagePath = @"../Images/loopback/MACRemoteLoopback.png";

            LoopbackListingModel LpBck_Mac = new LoopbackListingModel();
            LpBck_Mac.Name = "MAC I/F";
            LpBck_Mac.EnumLoopbackType = LoopBackMode.MAC;
            LpBck_Mac.ImagePath = @"../Images/loopback/MACLoopback.png";

            LoopbackListingModel LpBck_Digital = new LoopbackListingModel();
            LpBck_Digital.Name = "PCS";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            LpBck_Digital.ImagePath = @"../Images/loopback/PCSLoopback.png";

            LoopbackListingModel LpBck_LineDriver = new LoopbackListingModel();
            LpBck_LineDriver.Name = "PMA";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            LpBck_LineDriver.ImagePath = @"../Images/loopback/PMALoopback.png";

            LoopbackListingModel LpBck_ExtCable = new LoopbackListingModel();
            LpBck_ExtCable.Name = "External MII/RMII";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            LpBck_ExtCable.ImagePath = @"../Images/loopback/ExternalLoopback.png";

            Loopbacks = new List<LoopbackListingModel>()
            {
                LpBck_None,
                LpBck_MacRemote,
                LpBck_Mac,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable
            };

            SelectedLoopback = Loopbacks[0];
            SelectedLoopback.TxSuppression = true;
            SelectedLoopback.RxSuppression = false;
        }

        public LoopbackListingModel SelectedLoopback { get; set; }
        public List<LoopbackListingModel> Loopbacks { get; set; }
    }
}
