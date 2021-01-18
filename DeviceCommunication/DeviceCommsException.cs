//-----------------------------------------------------------------------
// <copyright file="DeviceCommsException.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace DeviceCommunication
{
    using System;

    /// <summary>
    /// Device Communication Exception class
    /// </summary>
    public class DeviceCommsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceCommsException" /> class
        /// </summary>
        /// <param name="message">Exception Message</param>
        public DeviceCommsException(string message)
            : base(message)
        {
        }
    }
}
