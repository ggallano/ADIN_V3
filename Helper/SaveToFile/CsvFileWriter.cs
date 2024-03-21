using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ADI.Register.Models;
using System.Collections.ObjectModel;

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
