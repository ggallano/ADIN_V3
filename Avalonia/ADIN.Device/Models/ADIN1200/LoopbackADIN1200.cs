// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Device.Models.ADIN1200
{
    public class LoopbackADIN1200 : ILoopback
    {
        public LoopbackADIN1200()
        {
            LpBck_None = new LoopbackModel();
            LpBck_None.Name = "OFF";
            LpBck_None.EnumLoopbackType = LoopBackMode.OFF;
            //LpBck_None.ImagePath = @"../Images/loopback/Loopback_None.png";

            LpBck_Digital = new LoopbackModel();
            LpBck_Digital.Name = "Digital";
            LpBck_Digital.EnumLoopbackType = LoopBackMode.Digital;
            //LpBck_Digital.ImagePath = @"../Images/loopback/Loopback_AllDigital.png";

            LpBck_LineDriver = new LoopbackModel();
            LpBck_LineDriver.Name = "LineDriver";
            LpBck_LineDriver.EnumLoopbackType = LoopBackMode.LineDriver;
            //LpBck_LineDriver.ImagePath = @"../Images/loopback/Loopback_LineDriver.png";

            LpBck_ExtCable = new LoopbackModel();
            LpBck_ExtCable.Name = "ExtCable";
            LpBck_ExtCable.EnumLoopbackType = LoopBackMode.ExtCable;
            //LpBck_ExtCable.ImagePath = @"../Images/loopback/Loopback_ExtCable.png";

            LpBck_Remote = new LoopbackModel();
            LpBck_Remote.Name = "Remote";
            LpBck_Remote.EnumLoopbackType = LoopBackMode.MacRemote;
            //LpBck_Remote.ImagePath = @"../Images/loopback/Loopback_Remote.png";

            Loopbacks = new ObservableCollection<LoopbackModel>()
            {
                LpBck_None,
                LpBck_Digital,
                LpBck_LineDriver,
                LpBck_ExtCable,
                LpBck_Remote
            };
        }
        public LoopbackModel LpBck_None { get; set; }
        public LoopbackModel LpBck_Digital { get; set; }
        public LoopbackModel LpBck_LineDriver { get; set; }
        public LoopbackModel LpBck_ExtCable { get; set; }
        public LoopbackModel LpBck_Remote { get; set; }

        public LoopbackModel SelectedLoopback { get; set; }
        public ObservableCollection<LoopbackModel> Loopbacks { get; set; }

        public bool RxSuppression { get; set; }
        public bool TxSuppression { get; set; }
        public string ImagePath_RxSuppression { get; set; }
        public string ImagePath_TxSuppression { get; set; }
    }
}
