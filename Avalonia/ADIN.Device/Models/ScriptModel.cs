// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class ScriptModel
    {
        public string Name { get; set; }
        public List<RegisterAccessModel> RegisterAccesses { get; set; }
    }
}
