//-----------------------------------------------------------------------
// <copyright file="RegisterBase.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace Utilities.JSONParser.JSONClasses
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The class that contains the register details
    /// </summary>
    [DataContract]
    public class RegisterBase : PropertyChangeNotifierBase
    {
        private uint value;

        /// <summary>
        /// Gets or sets the name of the field
        /// </summary>
        [DataMember(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the memory map of the register
        /// </summary>
        [DataMember(Name = "MMap")]
        public string MMap { get; set; }

        /// <summary>
        /// Gets or sets the value of the field
        /// </summary>
        [DataMember(Name = "Value")]
        public uint Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    this.RaisePropertyChanged("Value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Access of the register
        /// </summary>
        [DataMember(Name = "Access")]
        public string Access { get; set; }
    }
}
