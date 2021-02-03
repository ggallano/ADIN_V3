//-----------------------------------------------------------------------
// <copyright file="JSONParserEngine.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace Utilities.JSONParser
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;

    /// <summary>
    /// The class which parses the JSON files
    /// </summary>
    public class JSONParserEngine
    {
        /// <summary>
        /// Gets or sets registerJSON contents in class
        /// </summary>
        public RegisterJSONStructure RegisterFieldMapping { get; set; }

        /// <summary>
        /// Gets or sets registerJSON contents in class
        /// </summary>
        public ScriptJSONStructure Scripts { get; set; }

        /// <summary>
        /// Calls the function that parses the JSON file
        /// </summary>
        /// <param name="jsonFileName">Name of file to parse</param>
        public void ParseJSONData(string jsonFileName)
        {
            this.ParseJSONFile(jsonFileName);
        }

        /// <summary>
        /// Calls the function that parses the scripts JSON file
        /// </summary>
        /// <param name="jsonFileName">Name of file to parse</param>
        public ScriptJSONStructure ParseScriptData(string jsonFileName)
        {
            return this.ParseScriptsFile(jsonFileName);
        }

        /// <summary>
        /// Converts the JSON string to object of the given type
        /// </summary>
        /// <param name="measurementResponseJSON">The source JSON string</param>
        /// <param name="toType">The target object type</param>
        /// <returns>The converted object</returns>
        public object StringToObject(string measurementResponseJSON, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(measurementResponseJSON);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// This method converts the object passed into a JSON string
        /// </summary>
        /// <typeparam name="T">The type of the object to be converted</typeparam>
        /// <param name="objectToBeConverted">The object to be converted</param>
        /// <returns>The JSON string of the object</returns>
        public string ConvertObjectToString<T>(T objectToBeConverted)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, objectToBeConverted);
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Parsing the JSON files into the respective classes
        /// </summary>
        /// <param name="jsonFileName">Name of JSON file to parse</param>
        private void ParseJSONFile(string jsonFileName)
        {
            string configFilesPath = string.Empty;
            Stream jsonStream = File.OpenRead(Path.Combine(configFilesPath, jsonFileName));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RegisterJSONStructure));
            this.RegisterFieldMapping = (RegisterJSONStructure)serializer.ReadObject(jsonStream);
        }

        /// <summary>
        /// Parsing the JSON files into the respective classes
        /// </summary>
        /// <param name="jsonFileName">Name of JSON file to parse</param>
        private ScriptJSONStructure ParseScriptsFile(string jsonFileName)
        {
            string configFilesPath = string.Empty;
            Stream jsonStream = File.OpenRead(Path.Combine(configFilesPath, jsonFileName));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ScriptJSONStructure));
            return (ScriptJSONStructure)serializer.ReadObject(jsonStream);
        }
    }
}
