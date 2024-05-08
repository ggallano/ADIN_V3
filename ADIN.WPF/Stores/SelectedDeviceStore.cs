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
        public event Action SelectedDeviceChanged;
        public event Action<FeedbackModel> ProcessCompleted;
        public event Action<FeedbackModel> ErrorOccured;
        public event Action<FeedbackModel> ViewModelErrorOccured;

        public ADINDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set 
            { 
                _selectedDevice = value;
                _selectedDevice.FwAPI.WriteProcessCompleted += FirmwareAPI_WriteProcessCompleted;
                SelectedDeviceChanged?.Invoke();
            }
        }

        public void OnRegistersValueChanged()
        {
            RegisterListingValueChanged?.Invoke();
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