// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Register.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace Helper.SaveToFile
{
    public interface IFileWriter
    {
        void WriteContent(string fileName, StringBuilder contents);
        void WriteContent(string fileName, ObservableCollection<RegisterModel> registers);
    }
}
