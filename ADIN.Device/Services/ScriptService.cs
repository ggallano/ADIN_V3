// <copyright file="ScriptService.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ADIN.Device.Services
{
    public class ScriptService
    {
        public object GetScriptJsonFile { get; set; }

        public List<string> GetScripJsonFile()
        {
            return Directory.GetFiles(@".\Scripts").ToList();
        }

        public ScriptModel GetScriptSet(string scriptFileName)
        {
            ScriptModel script = new ScriptModel();
            var scriptContent = JsonConvert.DeserializeObject<ScriptSet>(File.ReadAllText(scriptFileName));

            foreach (var data in scriptContent.Scripts)
            {
                try
                {
                    script.Name = data.Name;
                    foreach (var subData in data.RegisterAccesses)
                    {
                        if (subData.RegisterAddress != null)
                            subData.RegisterAddress = ValidateValue(subData.RegisterAddress.ToLower());
                        subData.Value = ValidateValue(subData.Value.ToLower());
                    }
                    script.RegisterAccesses = data.RegisterAccesses;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return script;
        }

        private string ValidateValue(string value)
        {
            string hexPrefix = "0x";
            string binPrefix = "0b";
            string decPrefix = "0d";

            if (hexPrefix == value.Substring(0, 2))
            {
                return Convert.ToUInt32(value.Replace(hexPrefix, ""), 16).ToString();
            }

            if (binPrefix == value.Substring(0, 2))
            {
                return BitConverter.ToUInt32(Encoding.ASCII.GetBytes(value.Replace(binPrefix, "")), 0).ToString();
            }

            if (decPrefix == value.Substring(0, 2))
            {
                return Convert.ToUInt32(value.Replace(decPrefix, "")).ToString();
            }

            return Convert.ToUInt32(value).ToString();
        }
    }
}