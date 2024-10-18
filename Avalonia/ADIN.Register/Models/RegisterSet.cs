// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Register.Models
{
    public class RegisterSet
    {
        public ObservableCollection<RegisterModel> Registers { get; set; }
    }
}
