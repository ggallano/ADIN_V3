// <copyright file="FeedbackPropertyChange.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace Utilities.Feedback
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains the structure for a feedback class
    /// </summary>
    public class FeedbackPropertyChange : PropertyChangeNotifierBase
    {
        /// <summary>
        /// Stores the feedback to be sent
        /// </summary>
        private Feedback feedbackOfActions;

        /// <summary>
        /// Gets or sets the feedback to be sent
        /// </summary>
        public Feedback FeedbackOfActions
        {
            get
            {
                return this.feedbackOfActions;
            }

            set
            {
                this.feedbackOfActions = value;
                this.RaisePropertyChanged("FeedbackOfActions");
            }
        }

        /// <summary>
        /// Pass information string to handler.
        /// </summary>
        /// <param name="info">Information String</param>
        public void Info(string info)
        {
            this.FeedbackOfActions = new Feedback() { FeedbackType = FeedBackType.Info, FeedbackMessage = info };
        }

        /// <summary>
        /// Pass verbose information string to handler.
        /// </summary>
        /// <param name="info">Information String</param>
        public void VerboseInfo(string info)
        {
            this.FeedbackOfActions = new Feedback() { FeedbackType = FeedBackType.InfoVerbose, FeedbackMessage = info };
        }

        /// <summary>
        /// Pass information string to handler.
        /// </summary>
        /// <param name="info">Information String</param>
        public void Error(string info)
        {
            this.FeedbackOfActions = new Feedback() { FeedbackType = FeedBackType.Error, FeedbackMessage = info };
        }

        /// <summary>
        /// Pass information string to handler.
        /// </summary>
        /// <param name="info">Information String</param>
        public void Warning(string info)
        {
            this.FeedbackOfActions = new Feedback() { FeedbackType = FeedBackType.Warning, FeedbackMessage = info };
        }
    }
}
