// <copyright file="Loopback.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval
{
    public class Loopback
    {
        /// <summary>
        /// Gets or sets the loopback item
        /// </summary>
        public LoopbackItem LoopbackItem { get; set; }

        /// <summary>
        /// gets or sets the Tx Supression
        /// </summary>
        public bool TxSupression { get; set; }

        /// <summary>
        /// gets or sets the Rx Supression
        /// </summary>
        public bool RxSuspression { get; set; }
    }
}
