using System;
using System.Collections.Generic;
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
            LpBck_None.ImagePath = @"../Images/loopback/Lb_ADIN1320_None.png";

            LpBck_Digital = new LoopbackModel();
            LpBck_Digital.Name = "Digital";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            LpBck_Digital.ImagePath = @"../Images/loopback/Lb_ADIN1320_AllDigital.png";

            LpBck_LineDriver = new LoopbackModel();
            LpBck_LineDriver.Name = "LineDriver";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            LpBck_LineDriver.ImagePath = @"../Images/loopback/Lb_ADIN1320_LineDriver.png";

            LpBck_ExtCable = new LoopbackModel();
            LpBck_ExtCable.Name = "ExtCable";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            LpBck_ExtCable.ImagePath = @"../Images/loopback/Lb_ADIN1320_ExtCable.png";

            LpBck_Remote = new LoopbackModel();
            LpBck_Remote.Name = "Remote";
            LpBck_Remote.EnumLoopbackType = LoopBackMode.MacRemote;
            LpBck_Remote.ImagePath = @"../Images/loopback/Lb_ADIN1320_Remote.png";

            LpBck_SerDesDigital = new LoopbackModel();
            LpBck_SerDesDigital.Name = "SerDesDigital";
            LpBck_SerDesDigital.EnumLoopbackType = LoopBackMode.SerDesDigital;
            LpBck_SerDesDigital.ImagePath = @"../Images/loopback/Lb_ADIN1320_SerDesDigital.png";

            LpBck_SerDes = new LoopbackModel();
            LpBck_SerDes.Name = "SerDes";
            LpBck_SerDes.EnumLoopbackType = LoopBackMode.SerDes;
            LpBck_SerDes.ImagePath = @"../Images/loopback/Lb_ADIN1320_SerDes.png";

            LpBck_LineInterface = new LoopbackModel();
            LpBck_LineInterface.Name = "LineInterface";
            LpBck_LineInterface.EnumLoopbackType = LoopBackMode.LineInterface;
            LpBck_LineInterface.ImagePath = @"../Images/loopback/Lb_ADIN1320_LineInterface.png";

            LpBck_MII = new LoopbackModel();
            LpBck_MII.Name = "MII";
            LpBck_MII.EnumLoopbackType = LoopBackMode.MII;
            LpBck_MII.ImagePath = @"../Images/loopback/Lb_ADIN1320_MII.png";

            Loopbacks = new List<LoopbackModel>()
            {
                LpBck_None,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable,
                LpBck_Remote,
                LpBck_SerDesDigital,
                LpBck_SerDes,
                LpBck_LineInterface,
                LpBck_MII,
            };

            SelectedLoopback = Loopbacks[0];
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
        public List<LoopbackModel> Loopbacks { get; set; }

        public bool RxSuppression { get; set; }
        public bool TxSuppression { get; set; }
    }
}
