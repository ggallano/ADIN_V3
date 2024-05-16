using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Windows;

namespace ADIN.WPF.Stores
{
    public class SelectedDeviceStore
    {
        private ADINDevice _selectedDevice;

        public event Action RegisterListingValueChanged;
        public event Action<string> LinkStatusChanged;
        public event Action SelectedDeviceChanged;
        public event Action<string> SoftwarePowerDownChanged;
        public event Action<FeedbackModel> ProcessCompleted;
        public event Action<FeedbackModel> ErrorOccured;
        public event Action<FeedbackModel> ViewModelErrorOccured;

        public ADINDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set 
            {
                if(_selectedDevice != null)
                {
                    _selectedDevice.FwAPI.WriteProcessCompleted -= FirmwareAPI_WriteProcessCompleted;
                }

                if(value != null)
                {
                    _selectedDevice = value;
                    _selectedDevice.FwAPI.WriteProcessCompleted += FirmwareAPI_WriteProcessCompleted;
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
        private void FirmwareAPI_WriteProcessCompleted(object sender, FeedbackModel feedback)
        {
            ProcessCompleted?.Invoke(feedback);
        }

        public void OnViewModelErrorOccured(string errorMessage, FeedbackType errorType = FeedbackType.Error)
        {
            FeedbackModel errorFeedback = new FeedbackModel() { Message = errorMessage, FeedBackType = errorType };
            ViewModelErrorOccured?.Invoke(errorFeedback);
        }
    }
}