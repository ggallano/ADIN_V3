//-----------------------------------------------------------------------
// <copyright file="Fields.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The class showing the name of the fields linked to the sub tool
    /// </summary>
    [DataContract]
    public class Fields
    {
        /// <summary>
        /// Gets or sets the name of the field that is linked to the sub tool
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
