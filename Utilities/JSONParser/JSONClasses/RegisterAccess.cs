//-----------------------------------------------------------------------
// <copyright file="RegisterAccess.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The class defined a script register access
    /// </summary>
    [DataContract]
    public class RegisterAccess
    {
        /// <summary>
        /// Gets or sets the access description
        /// </summary>
        [DataMember(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the register access type
        /// </summary>
        [DataMember(Name = "AccessType")]
        public string AccessType { get; set; }

        /// <summary>
        /// Gets or sets the register memory map
        /// </summary>
        [DataMember(Name = "MemoryMap")]
        public string MMap { get; set; }

        /// <summary>
        /// Gets or sets the access name
        /// </summary>
        [DataMember(Name = "RegisterName")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the access name
        /// </summary>
        [DataMember(Name = "RegisterAddress")]
        public string RegisterAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the access mask
        /// </summary>
        [DataMember(Name = "Mask")]
        public uint Mask { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the access value
        /// </summary>
        [DataMember(Name = "Value")]
        public string Value { get; set; }
    }
}
