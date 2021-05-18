// <copyright file="FrameCheckerParameters.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface.Parameters
{
    using static FirmwareAPI;

    /// <summary>
    /// Parameter container for GePhyLoopbackConfig
    /// </summary>
    public class FrameCheckerParameters
    {
        /// <summary>
        /// Gets or sets frame Length
        /// </summary>
        public uint FrameLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets frame Number
        /// </summary>
        public uint FrameNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets enable checker
        /// </summary>
        public bool EnableChecker
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets enable frame content
        /// </summary>
        public FrameType FrameContent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets enable continuous
        /// </summary>
        public bool EnableContinuous
        {
            get;
            set;
        }

        /// <summary>
        /// gets or sets enable MAC Address
        /// </summary>
        public bool EnableMacAddress
        {
            get;
            set;
        }

        /// <summary>
        /// gets or sets destination MAC Address
        /// </summary>
        public string DestMacAddress
        {
            get;
            set;
        }

        /// <summary>
        /// gets or sets source MAC Address
        /// </summary>
        public string SourceMacAddress
        {
            get;
            set;
        }
    }
}
