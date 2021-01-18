//-----------------------------------------------------------------------
// <copyright file="FeedbackViewModel.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace ADIN1300_Eval.ViewModel
{
    using Commands;
    using Microsoft.Win32;
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Utilities.Feedback;

    /// <summary>
    /// This class handles the feedback updates to the UI
    /// </summary>
    public class FeedbackViewModel : Utilities.PropertyChangeNotifierBase
    {
        private const int MaxLogLen = 999;

        /// <summary>
        /// Stores the lock object for synchronization of Feedback logs in the UI
        /// </summary>
        private static object syncLock = new object();

        /// <summary>
        /// Stores the timer, that trigger an event after specified time
        /// </summary>
        private System.Windows.Threading.DispatcherTimer myDispatcherTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackViewModel"/> class
        /// </summary>
        public FeedbackViewModel()
        {
            this.myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            this.FeedbackLogs = new System.Collections.ObjectModel.ObservableCollection<string>();

            this.LogWindowCopyCommand = new BindingCommand(this.DoCopyCommand, this.CanDoCopyCommand);
            this.LogWindowClearCommand = new BindingCommand(this.DoClearCommand, this.CanDoClearCommand);
            this.LogWindowSaveCommand = new BindingCommand(this.DoSaveCommand, this.CanDoSaveCommand);

            // Enable the cross access to this collection elsewhere
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this.FeedbackLogs, syncLock);
        }

        /// <summary>
        /// Gets or sets the function to be called when copying from log window
        /// </summary>
        public BindingCommand LogWindowCopyCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when clearing log window
        /// </summary>
        public BindingCommand LogWindowClearCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when saving content
        /// </summary>
        public BindingCommand LogWindowSaveCommand { get; set; }

        /// <summary>
        /// Gets or sets the type of the feedback
        /// </summary>
        public FeedBackType FeedbackType { get; set; }

        /// <summary>
        /// Gets or sets the feedback text to be displayed
        /// </summary>
        public string FeedbackMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of log messages
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> FeedbackLogs { get; set; }

        /// <summary>
        /// Gets the color of the feedback text based on the feedback type
        /// </summary>
        public SolidColorBrush FeedbackColor
        {
            get
            {
                switch (this.FeedbackType)
                {
                    case FeedBackType.Error: return new SolidColorBrush(Colors.Red);
                    case FeedBackType.Warning: return new SolidColorBrush(Colors.Yellow);
                    case FeedBackType.Info: return new SolidColorBrush(Colors.Green);
                    default: return new SolidColorBrush(Colors.Black);
                }
            }
        }

        /// <summary>
        /// The method that sets the feedback message and feedback type
        /// </summary>
        /// <param name="type">Type of message which denotes severity of type <see cref="FeedbackType"/></param>
        /// <param name="message">The text message to be displayed as feedback</param>
        /// <param name="seconds">The number of milliseconds for much the message to be displayed, default is 5 seconds</param>
        public void SetFeedback(FeedBackType type, string message, int seconds = 5000)
        {
            if (this.myDispatcherTimer.IsEnabled)
            {
                this.myDispatcherTimer.Stop();
            }

            this.FeedbackType = type;
            this.FeedbackMessage = message;
            this.RaisePropertyChanged("FeedbackColor");
            this.RaisePropertyChanged("FeedbackMessage");
            if (seconds > 0)
            {
                this.SetTimeOut(delegate (object s, EventArgs args) { this.ClearFeedback(); }, seconds);
            }

            if (message != string.Empty && (message.IndexOf("Undelete") != 1))
            {
                switch (type)
                {
                    case FeedBackType.Error:
                        message = "[Error] " + message;
                        break;
                    case FeedBackType.InfoVerbose:
                        message = "[VerboseInfo] " + message;
                        break;
                    case FeedBackType.Info:
                        message = "[Info] " + message;
                        break;
                    case FeedBackType.Warning:
                        message = "[Warning] " + message;
                        break;
                }

                // if (type != FeedBackType.InfoVerbose)
                {
                    message = DateTime.Now.ToString("T") + " " + message;
                    try
                    {
                        this.FeedbackLogs.Insert(0, message);
                    }
                    catch (NotSupportedException)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            lock (syncLock)
                            {
                                this.FeedbackLogs.Insert(0, message);
                            }
                        }));
                    }
                }
            }

            if (this.FeedbackLogs.Count > FeedbackViewModel.MaxLogLen)
            {
                try
                {
                    this.FeedbackLogs.RemoveAt(FeedbackViewModel.MaxLogLen);
                }
                catch (NotSupportedException)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        lock (syncLock)
                        {
                            this.FeedbackLogs.RemoveAt(FeedbackViewModel.MaxLogLen);
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// The method that clears the feedback message
        /// </summary>
        public void ClearFeedback()
        {
            this.FeedbackMessage = string.Empty;
            this.RaisePropertyChanged("FeedbackMessage");
        }

        /// <summary>
        /// Method that sets timeout for which calls an event after specified time
        /// </summary>
        /// <param name="doWork">The logic to be called after the timeout</param>
        /// <param name="time">The timeout in milliseconds</param>
        private void SetTimeOut(EventHandler doWork, int time)
        {
            this.myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, time);
            this.myDispatcherTimer.Tick += delegate (object s, EventArgs args) { this.myDispatcherTimer.Stop(); };
            this.myDispatcherTimer.Tick += doWork;

            this.myDispatcherTimer.Start();
        }


        /// <summary>
        /// Copy Command
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoCopyCommand(object obj)
        {
        }

        /// <summary>
        /// Returns if log copying can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoCopyCommand(object arg)
        {
            return true;
        }

        /// <summary>
        /// Clear Command
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoClearCommand(object obj)
        {
            try
            {
                this.FeedbackLogs.Clear();
            }
            catch (NotSupportedException)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    lock (syncLock)
                    {
                        this.FeedbackLogs.Clear();
                    }
                }));
            }
        }

        /// <summary>
        /// Returns if log clearing can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoClearCommand(object arg)
        {
            return true;
        }

        /// <summary>
        /// Save Command
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoSaveCommand(object obj)
        {
            var timeNow = DateTime.Now;
            string filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ActivityLog_" + timeNow.ToLongDateString() + "_" + timeNow.Hour + "_ " + timeNow.Minute + "_" + timeNow.Second);

            var saveFileDialog = new SaveFileDialog { Filter = "LOG | *.log", FileName = filePath };
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;

                try
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(filePath, false))
                    {
                        foreach (string line in this.FeedbackLogs)
                        {
                            file.WriteLine(line);
                        }
                    }

                    this.SetFeedback(FeedBackType.Info, string.Format("Activity log saved to {0:s}", filePath));
                }
                catch (System.IO.IOException e)
                {
                    this.SetFeedback(FeedBackType.Error, string.Format("Activity log NOT saved to {0:s} {1:s}", filePath, e.Message));
                }
            }
            else
            {
                filePath = string.Empty;
            }
        }

        /// <summary>
        /// Returns if log saving can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoSaveCommand(object arg)
        {
            return true;
        }
    }
}
