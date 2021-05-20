// <copyright file="DeviceModel.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using DeviceCommunication;
    using TargetInterface;
    using Utilities.Feedback;

    /// <summary>
    /// Device Model
    /// </summary>
    public class DeviceModel : FeedbackPropertyChange
    {
        private bool isPresent = false;
        private string boardName;
        private string serialNumber;
        private bool lpbkmode;//loopback
        private DeviceConnection deviceConnection;

        private FirmwareAPI fwAPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceModel"/> class.
        /// </summary>
        /// <param name="serialNumber">Device id</param>
        /// <param name="propertyChange">Property change handler to set immediately</param>
        public DeviceModel(string serialNumber, string boardName, PropertyChangedEventHandler propertyChange)
        {
            this.PropertyChanged += propertyChange;
            this.serialNumber = serialNumber;

            this.boardName = boardName;

            this.deviceConnection = new DeviceConnection(this.serialNumber);
            this.deviceConnection.PropertyChanged += this.Feedback_PropertyChanged;

            this.fwAPI = new FirmwareAPI();
            this.fwAPI.PropertyChanged += this.FWAPI_PropertyChanged;
            //this.fwAPI.UpdateFromScriptJSON();

            this.fwAPI.AttachDevice(this.deviceConnection);
            this.isPresent = true;
        }
 
        public bool InLoopback
        {
            get
            {
                return this.lpbkmode;
            }

            set
            {
                if (this.lpbkmode != value)
                {
                    this.lpbkmode = value;
                    this.RaisePropertyChanged("LoopBack");//do I need this
                }
            }
        }
        /// <summary>
        /// Gets access to the firmware API
        /// </summary>
        public FirmwareAPI FwAPI
        {
            get
            {
                return this.fwAPI;
            }
        }

        /// <summary>
        /// gets or sets the board name
        /// </summary>
        public string BoardName
        {
            get
            {
                return this.boardName;
            }

            set
            {
                if (this.boardName != value)
                {
                    this.boardName = value;
                    this.RaisePropertyChanged("BoardName");
                }
            }
        }


        /// <summary>
        /// Gets or sets iD of the connected device
        /// </summary>
        public string SerialNumber
        {
            get
            {
                return this.serialNumber;
            }

            set
            {
                if (this.serialNumber != value)
                {
                    this.serialNumber = value;
                    this.RaisePropertyChanged("SerialNumber");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the device is present
        /// </summary>
        public bool IsPresent
        {
            get
            {
                return this.isPresent;
            }

            set
            {
                if (this.isPresent != value)
                {
                    this.isPresent = value;
                    this.RaisePropertyChanged("IsPresent");
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.serialNumber;
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void Feedback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    {
                        /* This is just some text...pass it up along the hierarchy */
                        FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;
                        feedback.FeedbackOfActions.FeedbackMessage = this.SerialNumber + " " + feedback.FeedbackOfActions.FeedbackMessage;
                        this.FeedbackOfActions = feedback.FeedbackOfActions;
                        break;
                    }
            }
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void FWAPI_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    {
                        /* This is just some text...pass it up along the hierarchy */
                        FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;
                        feedback.FeedbackOfActions.FeedbackMessage = this.SerialNumber + " " + feedback.FeedbackOfActions.FeedbackMessage;
                        this.FeedbackOfActions = feedback.FeedbackOfActions;
                        break;
                    }

                case "DeviceSettings":
                    {
                        /* The device status settings from the firmware has changed. Notify the DeviceViewModel */
                        this.RaisePropertyChanged(e.PropertyName);
                        break;
                    }
            }
        }

        private void SetBoolProperty(string property, bool value)
        {
            var firmwareAPIclass = typeof(FirmwareAPI);

            PropertyInfo pr = firmwareAPIclass.GetProperty(property);

            if (pr != null)
            {
                firmwareAPIclass.GetProperty(property).SetValue(this.fwAPI, value);
            }
            else
            {
                throw new ArgumentException(property);
            }
        }

        private bool GetBoolProperty(string property)
        {
            bool value = false;

            var firmwareAPIclass = typeof(FirmwareAPI);

            PropertyInfo pr = firmwareAPIclass.GetProperty(property);

            if (pr != null)
            {
                var rawValue = firmwareAPIclass.GetProperty(property).GetValue(this.fwAPI);

                if (rawValue is bool)
                {
                    value = (bool)rawValue;
                }
                else
                {
                    throw new ArgumentException(property);
                }
            }
            else
            {
                throw new ArgumentException(property);
            }

            return value;
        }
    }
}
