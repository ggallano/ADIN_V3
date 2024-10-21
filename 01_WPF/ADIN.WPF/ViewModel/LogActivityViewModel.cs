// <copyright file="LogActivityViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using Helper.Feedback;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ADIN.WPF.ViewModel
{
    public class LogActivityViewModel : ViewModelBase
    {
        private static object _syncLock = new object();
        private DispatcherTimer _myDispatcherTimer;
        private SelectedDeviceStore _selectedDeviceStore;

        public LogActivityViewModel(SelectedDeviceStore selectedDeviceStore)
        {
            _selectedDeviceStore = selectedDeviceStore;

            _myDispatcherTimer = new DispatcherTimer();
            LogMessages = new ObservableCollection<string>();
            // Enable the cross access to this collection elsewhere
            //System.Windows.Data.BindingOperations.EnableCollectionSynchronization(LogMessages, _syncLock);

            LogWindowClearCommand = new ClearLogCommand(this);
            LogWindowSaveCommand = new LogWindowSaveCommand(this, selectedDeviceStore);

            _selectedDeviceStore.ProcessCompleted += _selectedDeviceStore_ProcessCompleted;
            _selectedDeviceStore.ViewModelErrorOccured += _selectedDeviceStore_ViewModelErrorOccured;
        }

        /// <summary>
        /// Gets the color of the feedback text based on the feedback type
        /// </summary>
        public SolidColorBrush FeedbackColor
        {
            get
            {
                switch (FeedbackType)
                {
                    case FeedbackType.Error:
                        return new SolidColorBrush(Colors.Red);

                    case FeedbackType.Warning:
                        return new SolidColorBrush(Colors.Yellow);

                    case FeedbackType.Info:
                        return new SolidColorBrush(Colors.Green);

                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }
        }

        public FeedbackType FeedbackType { get; set; }

        public string LogMessage { get; set; }

        public ObservableCollection<string> LogMessages { get; set; }

        public ICommand LogWindowClearCommand { get; set; }

        public ICommand LogWindowSaveCommand { get; set; }

        /// <summary>
        /// The method that clears the feedback message
        /// </summary>
        public void ClearFeedback()
        {
            LogMessage = string.Empty;
            OnPropertyChanged("LogMessage");
        }

        /// <summary>
        /// The method that sets the feedback message and feedback type
        /// </summary>
        /// <param name="type">Type of message which denotes severity of type <see cref="FeedbackType"/></param>
        /// <param name="message">The text message to be displayed as feedback</param>
        /// <param name="seconds">The number of milliseconds for much the message to be displayed, default is 5 seconds</param>
        public void SetFeedback(FeedbackModel feedback, bool setSerialNumber = true, int seconds = 5000)
        {
            string message = feedback.Message;
            FeedbackType type = feedback.FeedBackType;

            if (this._myDispatcherTimer.IsEnabled)
                this._myDispatcherTimer.Stop();

            FeedbackType = type;
            LogMessage = message;
            OnPropertyChanged(nameof(FeedbackColor));

            if (seconds > 0)
            {
                this.SetTimeOut(delegate (object s, EventArgs args) { this.ClearFeedback(); }, seconds);
            }

            if (message != string.Empty && (message.IndexOf("Undelete") != 1))
            {
                if(setSerialNumber)
                {
                    message = _selectedDeviceStore.SelectedDevice.SerialNumber + " " + message;
                }

                switch (type)
                {
                    case FeedbackType.Error:
                        message = "[Error] " + message;
                        break;

                    case FeedbackType.Info:
                        message = "[Info] " + message;
                        break;

                    case FeedbackType.Warning:
                        message = "[Warning] " + message;
                        break;

                    case FeedbackType.Verbose:
                        message = "[VerboseInfo] " + message;
                        break;
                }

                message = DateTime.Now.ToString("T") + " " + message;
                try
                {
                    LogMessages.Insert(0, message);
                }
                catch (NotSupportedException)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        lock (_syncLock)
                        {
                            LogMessages.Insert(0, message);
                        }
                    }));
                }
            }

            if (this.LogMessages.Count > 100)
            {
                try
                {
                    LogMessages.RemoveAt(100);
                }
                catch (NotSupportedException)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        lock (_syncLock)
                        {
                            LogMessages.RemoveAt(100);
                        }
                    }));
                }
            }
        }

        //protected override void Dispose()
        //{
        //    _selectedDeviceStore.ProcessCompleted -= _selectedDeviceStore_ProcessCompleted;
        //    _selectedDeviceStore.ErrorOccured -= _selectedDeviceStore_ErrorOccured;
        //    base.Dispose();
        //}

        private void _selectedDeviceStore_ErrorOccured(FeedbackModel errorFeedback)
        {
            SetFeedback(errorFeedback);
        }

        private void _selectedDeviceStore_ProcessCompleted(FeedbackModel feedback)
        {
            SetFeedback(feedback);
        }

        private void _selectedDeviceStore_ViewModelErrorOccured(FeedbackModel errorFeedback)
        {
            SetFeedback(errorFeedback);
        }

        /// <summary>
        /// Method that sets timeout for which calls an event after specified time
        /// </summary>
        /// <param name="doWork">The logic to be called after the timeout</param>
        /// <param name="time">The timeout in milliseconds</param>
        private void SetTimeOut(EventHandler doWork, int time)
        {
            _myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, time);
            _myDispatcherTimer.Tick += delegate (object s, EventArgs args) { this._myDispatcherTimer.Stop(); };
            _myDispatcherTimer.Tick += doWork;

            _myDispatcherTimer.Start();
        }
    }
}