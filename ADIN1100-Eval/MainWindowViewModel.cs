// <copyright file="MainWindowViewModel.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1300_Eval
{
    using System.ComponentModel;
    using DeviceCommunication;
    using Model;
    using Utilities.Feedback;
    using ViewModel;

    /// <summary>
    /// Main Window View Model, conatins the other view models
    /// </summary>
    public class MainWindowViewModel
    {
        private FeedbackViewModel feedbackViewModel;

        private DeviceViewModel deviceViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            this.feedbackViewModel = new FeedbackViewModel();
            this.deviceViewModel = new DeviceViewModel();
            this.deviceViewModel.PropertyChanged += this.Feedback_PropertyChanged;
        }

        /// <summary>
        /// Gets the Device View Model
        /// </summary>
        public DeviceViewModel DeviceViewModel
        {
            get
            {
                return this.deviceViewModel;
            }
        }

        /// <summary>
        /// Gets the Feedback View Model
        /// </summary>
        public FeedbackViewModel FeedbackViewModel
        {
            get
            {
                return this.feedbackViewModel;
            }
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void Feedback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    {
                        this.feedbackViewModel.SetFeedback(feedback.FeedbackOfActions.FeedbackType, feedback.FeedbackOfActions.FeedbackMessage);
                        break;
                    }

                case "LinkStatus":
                    {
                    }

                    break;
            }
        }
    }
}
