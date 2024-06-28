using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class StatusStripViewModel : ViewModelBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ScriptModel _selectedScript;
        private bool _enableButton = true;

        public StatusStripViewModel(SelectedDeviceStore selectedDeviceStore, ScriptService scriptService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            Scripts = new List<ScriptModel>();

            ScriptApplyCommand = new ScriptApplyCommand(this, selectedDeviceStore);

            var ScriptJsonFiles = scriptService.GetScripJsonFile();
            foreach (var file in ScriptJsonFiles)
            {
                var scr = scriptService.GetScriptSet(file);
                Scripts.Add(scr);
            }

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        public ICommand ScriptApplyCommand { get; set; }

        public List<ScriptModel> Scripts { get; set; }

        public ScriptModel SelectedScript
        {
            get { return _selectedScript; }
            set
            {
                _selectedScript = value;
                OnPropertyChanged(nameof(SelectedScript));
            }
        }

        public bool EnableButton
        {
            get { return _enableButton; }
            set
            {
                _enableButton = value;
                OnPropertyChanged(nameof(EnableButton));
            }
        }

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged -= _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableButton = !onGoingCalibrationStatus;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedScript));
            OnPropertyChanged(nameof(Scripts));
            OnPropertyChanged(nameof(EnableButton));
        }
    }
}