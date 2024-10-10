using ADIN.Register.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.SaveToFile
{
    public abstract class AbstractFileWriter : IFileWriter
    {
        public abstract void WriteContent(string fileName, StringBuilder contents);
        public abstract void WriteContent(string fileName, ObservableCollection<RegisterModel> registers);
    }
}
