//-----------------------------------------------------------------------
// <copyright file="FieldDetails.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The class that specify the UI parameters for the fields to be displayed in GUI
    /// </summary>
    [DataContract]
    public class FieldDetails : RegisterBase
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
        /// Gets or sets the start of the field
        /// </summary>
        [DataMember(Name = "Start")]
        public uint Start { get; set; }

        /// <summary>
        /// Gets or sets the Width of the field
        /// </summary>
        [DataMember(Name = "Width")]
        public uint Width { get; set; }

        /// <summary>
        /// Gets or sets the ResetValue of the field
        /// </summary>
        [DataMember(Name = "ResetValue")]
        public uint ResetValue { get; set; }

        /// <summary>
        /// Gets or sets the description of the field
        /// </summary>
        [DataMember(Name = "Documentation")]
        public string Documentation { get; set; }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>string </returns>
        public override string ToString()
        {
            return string.Format("0x{0:X}", this.Value);
        }
    }
}
