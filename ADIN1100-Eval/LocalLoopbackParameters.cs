// <copyright file="LocalLoopbackParameters.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using static TargetInterface.FirmwareAPI;

    /// <summary>
    /// Parameter container for GePhyLoopbackConfig
    /// </summary>
    public class LocalLoopbackParameters
    {
        /// <summary>
        /// Local loopback mode selection
        /// </summary>
        public LoopBackMode gePhyLb_selt
        {
            get;
            set;
        }

        /// <summary>
        /// Isolate RX
        /// </summary>
        public bool isolateRx_st
        {
            get;
            set;
        }

        /// <summary>
        /// Isolate TX
        /// </summary>
        public bool lbTxSup_st
        {
            get;
            set;
        }
    }
}
