// <copyright file="RegisterInfo.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Utilities.JSONParser.JSONClasses;

    /// <summary>
    /// Register Information
    /// </summary>
    public class RegisterInfo
    {
        private RegisterDetails registerDetails;
        private FieldDetails fieldDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterInfo"/> class.
        /// Get information about the specified register or bit
        /// </summary>
        /// <param name="registerDetails">Name of memory map that the register is in</param>
        public RegisterInfo(RegisterDetails registerDetails)
        {
            this.fieldDetails = null;
            this.registerDetails = registerDetails;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterInfo"/> class.
        /// </summary>
        /// <param name="regdetail">Details of the register</param>
        /// <param name="fieldDetails">Bifield name</param>
        public RegisterInfo(RegisterDetails regdetail, FieldDetails fieldDetails)
        {
            this.fieldDetails = fieldDetails;
            this.registerDetails = regdetail;
        }

        /// <summary>
        /// Gets a value indicating the address
        /// </summary>
        public uint Address
        {
            get { return this.registerDetails.Address; }
        }

        /// <summary>
        /// Gets a value indicating whether this a sub field or not
        /// </summary>
        public bool IsSubField
        {
            get { return this.fieldDetails != null; }
        }

        /// <summary>
        /// Extract the field value from the full contents
        /// </summary>
        /// <param name="full_reg_contents">Full register content</param>
        /// <returns>Field Value</returns>
        public uint ExtractFieldValue(uint full_reg_contents)
        {
            uint fieldContents = full_reg_contents;
            if (this.IsSubField)
            {
                uint mask = (1U << (int)this.fieldDetails.Width) - 1;
                fieldContents = (full_reg_contents >> (int)this.fieldDetails.Start) & mask;
            }

            return fieldContents;
        }

        /// <summary>
        /// Insert the field value from the full contents
        /// </summary>
        /// <param name="full_reg_contents">Full register content</param>
        /// <param name="field_value">field content</param>
        /// <returns>Field Value</returns>
        public uint InsertFieldValue(uint full_reg_contents, uint field_value)
        {
            uint fieldContents = full_reg_contents;
            if (this.fieldDetails != null)
            {
                uint mask = (1U << (int)this.fieldDetails.Width) - 1;
                fieldContents &= ~(mask << (int)this.fieldDetails.Start);
                fieldContents |= (field_value & mask) << (int)this.fieldDetails.Start;
            }

            return fieldContents;
        }
    }
}
