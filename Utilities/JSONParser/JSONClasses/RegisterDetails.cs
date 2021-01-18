//-----------------------------------------------------------------------
// <copyright file="RegisterDetails.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The class that contains the register details
    /// </summary>
    [DataContract]
    public class RegisterDetails : RegisterBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include in dump
        /// </summary>
        [DataMember(Name = "IncludeInDump")]
        public bool IncludeInDump { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include in dump
        /// </summary>
        [DataMember(Name = "Visibility")]
        public string Visibility { get; set; }

        /// <summary>
        /// Gets or sets the address of the register
        /// </summary>
        [DataMember(Name = "Address")]
        public uint Address { get; set; }

        /// <summary>
        /// Gets or sets the default values for the registers
        /// </summary>
        [DataMember(Name = "ResetValue")]
        public uint ResetValue { get; set; }

        /// <summary>
        /// Gets or sets the description of the register
        /// </summary>
        [DataMember(Name = "Documentation")]
        public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the Image of the register
        /// </summary>
        [DataMember(Name = "Image")]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the Group of the register
        /// </summary>
        [DataMember(Name = "Group")]
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the list of fields in the register
        /// </summary>
        [DataMember(Name = "BitFields")]
        public FieldDetails[] Fields { get; set; }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>string </returns>
        public override string ToString()
        {
            return string.Format("0x{0:X4}", this.Value);
        }
    }
}
