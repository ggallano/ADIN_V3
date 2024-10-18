// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

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
