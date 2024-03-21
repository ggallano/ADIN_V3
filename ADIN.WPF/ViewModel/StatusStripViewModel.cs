using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using System.Collections.Generic;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class StatusStripViewModel : ViewModelBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private ScriptModel _selectedScript;

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

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedScript));
            OnPropertyChanged(nameof(Scripts));
        }
    }
}