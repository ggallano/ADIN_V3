using ADI.Register.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Helper.SaveToFile
{
    public abstract class AbstractFileWriter : IFileWriter
    {
        public abstract void WriteContent(string fileName, StringBuilder contents);
        public abstract void WriteContent(string fileName, ObservableCollection<RegisterModel> registers);
    }
}