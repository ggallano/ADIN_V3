// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Helper.Feedback
{
    public class FeedbackModel
    {
        public event Action LogActivityChanged;
        public string Message { get; set; }
        public FeedbackType FeedBackType { get; set; }
    }
}
