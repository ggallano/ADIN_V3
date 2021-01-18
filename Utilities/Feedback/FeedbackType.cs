//-----------------------------------------------------------------------
// <copyright file="FeedbackType.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace Utilities.Feedback
{
    /// <summary>
    /// The enumeration stating the type of Feedback given to the UI
    /// </summary>
    public enum FeedBackType
    {
        /// <summary>
        /// The error type of feedback which is severe
        /// </summary>
        Error,

        /// <summary>
        /// The warning type of feedback which is of medium priority
        /// </summary>
        Warning,

        /// <summary>
        /// The information type of feedback
        /// </summary>
        Info,

        /// <summary>
        /// The verbose information type of feedback
        /// </summary>
        InfoVerbose
    }
}
