// <copyright file="XmlFileLoader.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Register.Models;
using System.Collections.ObjectModel;
using System.Xml;

namespace ADIN.Helper.FileToLoad
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
