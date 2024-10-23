// <copyright file="AbstractFileWriter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Register.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace ADIN.Helper.SaveToFile
{
    public abstract class AbstractFileWriter : IFileWriter
    {
        public abstract void WriteContent(string fileName, StringBuilder contents);
        public abstract void WriteContent(string fileName, ObservableCollection<RegisterModel> registers);
    }
}
