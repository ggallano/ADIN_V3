using ADIN.Register.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Helper.FileToLoad
{
    public class XmlFileLoader
    {
        public ObservableCollection<RegisterModel> XmlFileLoadContent(string fileName)
        {
            ObservableCollection<RegisterModel> registers = new ObservableCollection<RegisterModel>();

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(registers.GetType());
                registers = (ObservableCollection<RegisterModel>)x.Deserialize(reader);
            }

            return registers;
        }
    }
}
