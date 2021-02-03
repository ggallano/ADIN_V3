//-----------------------------------------------------------------------
// <copyright file="ScriptJSONStructure.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser
{
    using System.Runtime.Serialization;
    using JSONClasses;

    /// <summary>
    /// This class contains information about the registers_script.JSON being parsed
    /// </summary>
    [DataContract]
    public class ScriptJSONStructure
    {
        /// <summary>
        /// Gets or sets the scripts
        /// </summary>
        [DataMember(Name = "Script")]
        public ScriptDetails Script { get; set; }
    }
}
