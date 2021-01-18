//-----------------------------------------------------------------------
// <copyright file="ScriptDetails.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The class that contains the Script details
    /// </summary>
    [DataContract]
    public class ScriptDetails
    {
        /// <summary>
        /// Gets or sets a value indicating the script description
        /// </summary>
        [DataMember(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the script Name
        /// </summary>
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of register accesses in the script
        /// </summary>
        [DataMember(Name = "RegisterAccesses")]
        public RegisterAccess[] RegisterAccesses { get; set; }
    }
}
