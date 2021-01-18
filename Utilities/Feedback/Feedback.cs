//-----------------------------------------------------------------------
// <copyright file="Feedback.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace Utilities.Feedback
{
    /// <summary>
    /// This class contains the structure for the feedback details
    /// </summary>
    public class Feedback : PropertyChangeNotifierBase
    {
        /// <summary>
        /// Stores the feedback text to be displayed
        /// </summary>
        private string feedbackMessage;

        /// <summary>
        /// Gets or sets the feedback text to be displayed
        /// </summary>
        public string FeedbackMessage
        {
            get
            {
                return this.feedbackMessage;
            }

            set
            {
                this.feedbackMessage = value;
                this.RaisePropertyChanged("FeedbackMessage");
            }
        }

        /// <summary>
        /// Gets or sets the type of the feedback
        /// </summary>
        public FeedBackType FeedbackType { get; set; }
    }
}
