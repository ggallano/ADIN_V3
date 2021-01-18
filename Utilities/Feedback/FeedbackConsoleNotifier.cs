// <copyright file="FeedbackConsoleNotifier.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace Utilities.Feedback
{
    /// <summary>
    /// Feedback Console Notifier
    /// </summary>
    public class FeedbackConsoleNotifier
    {
        /// <summary>
        /// Prefix for the command line
        /// </summary>
        private string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackConsoleNotifier"/> class.
        /// </summary>
        /// <param name="prefix">Prefix</param>
        public FeedbackConsoleNotifier(string prefix = "")
        {
            this.prefix = prefix;
        }

        /// <summary>
        /// Gets Prefix for the command line
        /// </summary>
        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void Feedback_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;
                    System.Console.WriteLine(this.prefix + feedback.FeedbackOfActions.FeedbackMessage);
                    break;
            }
        }
    }
}
