// <copyright file="RegisterParameters.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval
{
    /// <summary>
    /// Parameter container for WriteRegister
    /// </summary>
    public class RegisterParameters
    {
        private uint registerAddress = 0;
        private bool registerAddressOk = false;

        private uint registerValue = 0;
        private bool registerValueOk = false;

        /// <summary>
        /// Gets or sets Register Address
        /// </summary>
        public uint RegisterAddress
        {
            get
            {
                return this.registerAddress;
            }

            set
            {
                this.registerAddress = value;
                this.registerAddressOk = true;
            }
        }

        /// <summary>
        /// Gets or sets Register Value
        /// </summary>
        public uint RegisterValue
        {
            get
            {
                return this.registerValue;
            }

            set
            {
                this.registerValue = value;
                this.registerValueOk = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Register address OK
        /// </summary>
        public bool RegisterAddressOk
        {
            get
            {
                return this.registerAddressOk;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Register Value OK
        /// </summary>
        public bool RegisterValueOk
        {
            get
            {
                return this.registerValueOk;
            }
        }
    }
}
