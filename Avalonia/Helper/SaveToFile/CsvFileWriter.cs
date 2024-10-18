// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Register.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace Helper.SaveToFile
{
    public class CsvFileWriter : AbstractFileWriter
    {
        public override void WriteContent(string fileName, ObservableCollection<RegisterModel> registers)
        {
            throw new NotSupportedException("This function is not support");
        }

        public override void WriteContent(string fileName, StringBuilder contents)
        {
            try
            {
                using (StreamWriter wr = new StreamWriter(fileName))
                {
                    wr.Write(contents);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
    }
}
