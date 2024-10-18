// <copyright file="SelectedDeviceStore.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using Helper.Feedback;
using System;
using System.Collections.Generic;

namespace ADIN.Avalonia.Stores
{
    public class SelectedDeviceStore
    {
        private ADINDevice _selectedDevice;

        public event Action<string> BusyStateChanged;

        public event Action<FrameType> FrameContentChanged;

        public event Action<string> FrameGenCheckerResetDisplay;

        public event Action<string> FrameGenCheckerErrorResetDisplay;

        public event Action<string> FrameGenCheckerStatusChanged;

        //public event Action<FeedbackModel> ErrorOccured;

        public event Action<List<string>> GigabitCableDiagCompleted;

        public event Action<EthPhyState> LinkStatusChanged;

        public event Action<bool> OnGoingCalibrationStatusChanged;

        public event Action PhyModeChanged;

        public event Action PortNumChanged;

        public event Action<FeedbackModel> ProcessCompleted;

        public event Action RegisterListingValueChanged;

        //public event Action<LoopBackMode> LoopbackChanged;

        //public event Action<LoopbackModel> LoopbackStateChanged;

        public event Action SelectedDeviceChanged;

        public event Action<string> SoftwarePowerDownChanged;

        public event Action<FeedbackModel> ViewModelErrorOccured;

        public ADINDevice SelectedDevice
        {
            get { return _selectedDevice; }

            set
            {
                if (_selectedDevice != null)
                {
                    _selectedDevice.FwAPI.WriteProcessCompleted -= FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FwAPI.FrameGenCheckerTextStatusChanged -= FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FwAPI.ResetFrameGenCheckerStatisticsChanged -= FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    _selectedDevice.FwAPI.FrameContentChanged -= FirmwareAPI_FrameContentChanged;
                    _selectedDevice.FwAPI.GigabitCableDiagCompleted -= FwAPI_GigabitCableDiagCompleted;
                }

                if (value != null)
                {
                    _selectedDevice = value;
                    _selectedDevice.FwAPI.WriteProcessCompleted += FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FwAPI.FrameGenCheckerTextStatusChanged += FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FwAPI.ResetFrameGenCheckerStatisticsChanged += FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    _selectedDevice.FwAPI.ResetFrameGenCheckerErrorStatisticsChanged += FirmwareAPI_ResetFrameGenCheckerErrorStatisticsChanged;
                    _selectedDevice.FwAPI.FrameContentChanged += FirmwareAPI_FrameContentChanged;
                    _selectedDevice.FwAPI.GigabitCableDiagCompleted += FwAPI_GigabitCableDiagCompleted;
                    SelectedDeviceChanged?.Invoke();
                }

                if (value == null)
                {
                    _selectedDevice = value;
                    SelectedDeviceChanged?.Invoke();
                }
            }
        }

        public void OnBusyStateChanged(string busyContent)
        {
            BusyStateChanged?.Invoke(busyContent);
        }

        public void OnLinkStatusChanged(EthPhyState linkStatus)
        {
            LinkStatusChanged?.Invoke(linkStatus);
        }

        public void OnOngoingCalibrationStatusChanged(bool OnGoingCalibrationStatus)
        {
            OnGoingCalibrationStatusChanged?.Invoke(OnGoingCalibrationStatus);
        }

        public void OnPortNumChanged()
        {
            PortNumChanged?.Invoke();
        }

        public void OnRegistersValueChanged()
        {
            RegisterListingValueChanged?.Invoke();
        }

        public void OnSoftwarePowerDownChanged(string linkStatus)
        {
            SoftwarePowerDownChanged?.Invoke(linkStatus);
        }

        public void OnViewModelErrorOccured(string errorMessage, FeedbackType errorType = FeedbackType.Error)
        {
            FeedbackModel errorFeedback = new FeedbackModel() { Message = errorMessage, FeedBackType = errorType };
            ViewModelErrorOccured?.Invoke(errorFeedback);
        }

        public void OnViewModelFeedbackLog(string message, FeedbackType feedbackType = FeedbackType.Info)
        {
            FeedbackModel feedback = new FeedbackModel();
            feedback.Message = message;
            feedback.FeedBackType = feedbackType;
            ViewModelErrorOccured?.Invoke(feedback);
        }

        public void OnPhyModeChanged()
        {
            PhyModeChanged?.Invoke();
        }

        private void FirmwareAPI_FrameContentChanged(object sender, FrameType e)
        {
            FrameContentChanged?.Invoke(e);
        }

        private void FirmwareAPI_FrameGenCheckerStatusCompleted(object sender, string status)
        {
            FrameGenCheckerStatusChanged?.Invoke(status);
        }

        private void FirmwareAPI_ResetFrameGenCheckerStatisticsChanged(object sender, string status)
        {
            FrameGenCheckerResetDisplay?.Invoke(status);
        }

        private void FirmwareAPI_ResetFrameGenCheckerErrorStatisticsChanged(object sender, string status)
        {
            FrameGenCheckerErrorResetDisplay?.Invoke(status);
        }

        private void FirmwareAPI_WriteProcessCompleted(object sender, FeedbackModel feedback)
        {
            ProcessCompleted?.Invoke(feedback);
        }

        private void FwAPI_GigabitCableDiagCompleted(object sender, List<string> results)
        {
            GigabitCableDiagCompleted?.Invoke(results);
        }
    }
}
