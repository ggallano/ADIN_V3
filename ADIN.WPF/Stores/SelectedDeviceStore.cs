using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Windows;
using System.Windows.Forms;

namespace ADIN.WPF.Stores
{
    public class SelectedDeviceStore
    {
        private ADINDevice _selectedDevice;

        public event Action<FrameType> FrameContentChanged;
        public event Action<string> FrameGenCheckerResetDisplay;
        public event Action<string> FrameGenCheckerStatusChanged;
        public event Action RegisterListingValueChanged;
        public event Action<string> LinkStatusChanged;
        public event Action<bool> OnGoingCalibrationStatusChanged;
        public event Action PortNumChanged;
        //public event Action<LoopBackMode> LoopbackChanged;
        //public event Action<LoopbackModel> LoopbackStateChanged;
        public event Action SelectedDeviceChanged;
        public event Action<string> SoftwarePowerDownChanged;
        public event Action<FeedbackModel> ProcessCompleted;
        //public event Action<FeedbackModel> ErrorOccured;
        public event Action<FeedbackModel> ViewModelErrorOccured;

        public ADINDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set 
            {
                if(_selectedDevice != null)
                {
                    _selectedDevice.FwAPI.WriteProcessCompleted -= FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FwAPI.FrameGenCheckerTextStatusChanged -= FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FwAPI.ResetFrameGenCheckerStatisticsChanged -= FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    //_selectedDevice.FwAPI.ErrorOccured -= FirmwareAPI_ErrorOccured;
                    _selectedDevice.FwAPI.FrameContentChanged -= FirmwareAPI_FrameContentChanged;
                }

                if(value != null)
                {
                    _selectedDevice = value;
                    _selectedDevice.FwAPI.WriteProcessCompleted += FirmwareAPI_WriteProcessCompleted;
                    _selectedDevice.FwAPI.FrameGenCheckerTextStatusChanged += FirmwareAPI_FrameGenCheckerStatusCompleted;
                    _selectedDevice.FwAPI.ResetFrameGenCheckerStatisticsChanged += FirmwareAPI_ResetFrameGenCheckerStatisticsChanged;
                    //_selectedDevice.FwAPI.ErrorOccured += FirmwareAPI_ErrorOccured;
                    _selectedDevice.FwAPI.FrameContentChanged += FirmwareAPI_FrameContentChanged;
                    SelectedDeviceChanged?.Invoke();
                }

                if(value == null)
                {
                    _selectedDevice = value;
                    SelectedDeviceChanged?.Invoke();
                }
            }
        }

        public void OnRegistersValueChanged()
        {
            RegisterListingValueChanged?.Invoke();
        }
        public void OnLinkStatusChanged(string linkStatus)
        {
            LinkStatusChanged?.Invoke(linkStatus);
        }
        public void OnSoftwarePowerDownChanged(string linkStatus)
        {
            SoftwarePowerDownChanged?.Invoke(linkStatus);
        }
        public void OnOngoingCalibrationStatusChanged(bool OnGoingCalibrationStatus)
        {
            OnGoingCalibrationStatusChanged?.Invoke(OnGoingCalibrationStatus);
        }
        public void OnPortNumChanged()
        {
            PortNumChanged?.Invoke();
        }
        private void FirmwareAPI_ResetFrameGenCheckerStatisticsChanged(object sender, string status)
        {
            FrameGenCheckerResetDisplay?.Invoke(status);
        }
        private void FirmwareAPI_FrameContentChanged(object sender, FrameType e)
        {
            FrameContentChanged?.Invoke(e);
        }
        private void FirmwareAPI_FrameGenCheckerStatusCompleted(object sender, string status)
        {
            FrameGenCheckerStatusChanged?.Invoke(status);
        }
        private void FirmwareAPI_WriteProcessCompleted(object sender, FeedbackModel feedback)
        {
            ProcessCompleted?.Invoke(feedback);
        }

        public void OnViewModelFeedbackLog(string message, FeedbackType feedbackType = FeedbackType.Info)
        {
            FeedbackModel feedback = new FeedbackModel();
            feedback.Message = message;
            feedback.FeedBackType = feedbackType;
            ViewModelErrorOccured?.Invoke(feedback);
        }
        public void OnViewModelErrorOccured(string errorMessage, FeedbackType errorType = FeedbackType.Error)
        {
            FeedbackModel errorFeedback = new FeedbackModel() { Message = errorMessage, FeedBackType = errorType };
            ViewModelErrorOccured?.Invoke(errorFeedback);
        }
    }
}