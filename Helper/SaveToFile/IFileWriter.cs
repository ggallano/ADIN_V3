using ADI.Register.Models;
using System.Collections.Generic;
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