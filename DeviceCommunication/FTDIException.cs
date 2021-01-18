//-----------------------------------------------------------------------
// <copyright file="FTDIException.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace DeviceCommunication
{
    using System;
    using FTD2XX_NET;

    /// <summary>
    /// Detailed Exception class
    /// </summary>
    public class FTDIException : DeviceCommsException
    {
        /// <summary>
        /// FTDI API status code
        /// </summary>
        private FTDI.FT_STATUS ftStatus;

        /// <summary>
        /// FTDI API function name
        /// </summary>
        private string ftAPIFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="FTDIException" /> class
        /// </summary>
        /// <param name="ftParam">Paramter to API function</param>
        /// <param name="ftStatus">FTDI API status code</param>
        /// <param name="ftAPIFunc">FTDI API function name</param>
        public FTDIException(string ftParam, FTDI.FT_STATUS ftStatus, string ftAPIFunc)
            : base(ftStatus.ToString() + " " + ftAPIFunc + "(" + ftParam + ")")
        {
            this.ftStatus = ftStatus;
            this.ftAPIFunc = ftAPIFunc;
        }
    }
}
