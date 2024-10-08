// <copyright file="XmlFileWriter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Models;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;

namespace Helper.SaveToFile
{
    public class XmlFileWriter : AbstractFileWriter
    {
        public override void WriteContent(string fileName, ObservableCollection<RegisterModel> registers)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(registers.GetType());
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                xml.Serialize(writer, registers);
            }
        }

        public override void WriteContent(string fileName, StringBuilder contents)
        {
            throw new NotSupportedException();
        }
    }
}