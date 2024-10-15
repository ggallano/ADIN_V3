using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1100
{
    public class LoopbackADIN1100 : ILoopback
    {
        public LoopbackADIN1100()
        {
            LoopbackModel LpBck_None = new LoopbackModel();
            LpBck_None.Name = "None";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            //LpBck_None.ImagePath = new Bitmap(@"../Images/loopback/NoLoopback.png");

            LoopbackModel LpBck_MacRemote = new LoopbackModel();
            LpBck_MacRemote.Name = "MAC I/F Remote";
            LpBck_MacRemote.EnumLoopbackType = LoopBackMode.MacRemote;
            //LpBck_MacRemote.ImagePath = @"../Images/loopback/MACRemoteLoopback.png";

            LoopbackModel LpBck_Mac = new LoopbackModel();
            LpBck_Mac.Name = "MAC I/F";
            LpBck_Mac.EnumLoopbackType = LoopBackMode.MAC;
            //LpBck_Mac.ImagePath = @"../Images/loopback/MACLoopback.png";

            LoopbackModel LpBck_Digital = new LoopbackModel();
            LpBck_Digital.Name = "PCS";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            //LpBck_Digital.ImagePath = @"../Images/loopback/PCSLoopback.png";

            LoopbackModel LpBck_LineDriver = new LoopbackModel();
            LpBck_LineDriver.Name = "PMA";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            //LpBck_LineDriver.ImagePath = @"../Images/loopback/PMALoopback.png";

            LoopbackModel LpBck_ExtCable = new LoopbackModel();
            LpBck_ExtCable.Name = "External MII/RMII";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            //LpBck_ExtCable.ImagePath = @"../Images/loopback/ExternalLoopback.png";

            Loopbacks = new ObservableCollection<LoopbackModel>()
            {
                LpBck_None,
                LpBck_MacRemote,
                LpBck_Mac,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable
            };

            SelectedLoopback = Loopbacks[0];
            //SelectedLoopback.TxSuppression = true;
            //SelectedLoopback.RxSuppression = false;
        }

        public LoopbackModel SelectedLoopback { get; set; }
        public ObservableCollection<LoopbackModel> Loopbacks { get; set; }

        public bool RxSuppression { get; set; }
        public bool TxSuppression { get; set; }
        public string ImagePath_RxSuppression { get; set; }
        public string ImagePath_TxSuppression { get; set; }
    }
}
