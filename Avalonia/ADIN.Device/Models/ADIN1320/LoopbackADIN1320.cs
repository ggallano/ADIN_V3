using Avalonia;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1320
{
    public class LoopbackADIN1320 : ILoopback
    {
        public LoopbackADIN1320()
        {
            LpBck_None = new LoopbackModel();
            LpBck_None.Name = "OFF";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            LpBck_None.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbNone.png";

            LpBck_Digital = new LoopbackModel();
            LpBck_Digital.Name = "All Digital";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            LpBck_Digital.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbAllDigital.png";
            LpBck_Digital.DisabledModes = new List<string>()
            {
                "Fiber Media Only",
                "Backplane",
                "Auto Media Detect_Fi"
            };

            LpBck_LineDriver = new LoopbackModel();
            LpBck_LineDriver.Name = "Line Driver";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            LpBck_LineDriver.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbLineDriver.png";
            LpBck_LineDriver.DisabledModes = new List<string>()
            {
                "Fiber Media Only",
                "Backplane",
                "Auto Media Detect_Fi"
            };

            LpBck_SerDesDigital = new LoopbackModel();
            LpBck_SerDesDigital.Name = "SerDes Digital";
            LpBck_SerDesDigital.EnumLoopbackType = LoopBackMode.SerDesDigital;
            LpBck_SerDesDigital.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbSerDesDigital.png";
            LpBck_SerDesDigital.DisabledModes = new List<string>()
            {
                "Copper Media Only",
                "Auto Media Detect_Cu"
            };

            LpBck_SerDes = new LoopbackModel();
            LpBck_SerDes.Name = "SerDes";
            LpBck_SerDes.EnumLoopbackType = LoopBackMode.SerDes;
            LpBck_SerDes.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbSerDes.png";
            LpBck_SerDes.DisabledModes = new List<string>()
            {
                "Copper Media Only",
                "Auto Media Detect_Cu"
            };

            LpBck_LineInterface = new LoopbackModel();
            LpBck_LineInterface.Name = "Line Interface";
            LpBck_LineInterface.EnumLoopbackType = LoopBackMode.LineInterface;
            LpBck_LineInterface.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbLineInterface.png";
            LpBck_LineInterface.DisabledModes = new List<string>()
            {
                "Copper Media Only",
                "Auto Media Detect_Cu"
            };

            LpBck_MII = new LoopbackModel();
            LpBck_MII.Name = "MII";
            LpBck_MII.EnumLoopbackType = LoopBackMode.MII;
            LpBck_MII.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbMII.png";
            LpBck_MII.DisabledModes = new List<string>()
            {
                "Copper Media Only",
                "Auto Media Detect_Cu"
            };

            LpBck_ExtCable = new LoopbackModel();
            LpBck_ExtCable.Name = "External Cable";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            LpBck_ExtCable.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbExtCable.png";

            LpBck_Remote = new LoopbackModel();
            LpBck_Remote.Name = "Remote";
            LpBck_Remote.EnumLoopbackType = LoopBackMode.MacRemote;
            LpBck_Remote.ImagePath = "/Images/loopback_ADIN1320/ADIN1320_LbRemote.png";

            Loopbacks = new ObservableCollection<LoopbackModel>()
            {
                LpBck_None,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_SerDesDigital,
                LpBck_SerDes,
                LpBck_LineInterface,
                LpBck_MII,
                LpBck_ExtCable,
                LpBck_Remote,
            };
        }

        public LoopbackModel LpBck_None { get; set; }
        public LoopbackModel LpBck_Digital { get; set; }
        public LoopbackModel LpBck_LineDriver { get; set; }
        public LoopbackModel LpBck_ExtCable { get; set; }
        public LoopbackModel LpBck_Remote { get; set; }
        public LoopbackModel LpBck_SerDesDigital { get; set; }
        public LoopbackModel LpBck_SerDes { get; set; }
        public LoopbackModel LpBck_LineInterface { get; set; }
        public LoopbackModel LpBck_MII { get; set; }

        public LoopbackModel SelectedLoopback { get; set; }
        public ObservableCollection<LoopbackModel> Loopbacks { get; set; }

        public bool RxSuppression { get; set; }
        public bool TxSuppression { get; set; }
        public string ImagePath_RxSuppression { get; set; } = @"/Images/loopback_ADIN1320/ADIN1320_LbRx.png";
        public string ImagePath_TxSuppression { get; set; } = @"/Images/loopback_ADIN1320/ADIN1320_LbTx.png";
    }
}
