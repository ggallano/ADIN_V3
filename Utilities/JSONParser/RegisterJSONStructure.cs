//-----------------------------------------------------------------------
// <copyright file="RegisterJSONStructure.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser
{
    using System.Runtime.Serialization;
    using JSONClasses;

    /// <summary>
    /// This class contains information about the register.JSON being parsed
    /// </summary>
    [DataContract]
    public class RegisterJSONStructure
    {
        /// <summary>
        /// Gets or sets the list of registers under that block
        /// </summary>
        [DataMember(Name = "Registers")]
        public RegisterDetails[] Registers { get; set; }
    }
}
