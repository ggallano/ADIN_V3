using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RegisterAccessViewModel : ViewModelBase
    {
        protected SelectedDeviceStore _selectedDeviceStore;
        private bool _isEnable;
        private string _readInput = string.Empty;
        private string _readOutput = string.Empty;
        private string _writeInput = string.Empty;
        private string _writeValue = string.Empty;

        public RegisterAccessViewModel(SelectedDeviceStore selectedDeviceStore, NavigationStore navigationStore)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _isEnable = false;

            ReadRegisterCommand = new ReadRegisterCommand(this, selectedDeviceStore);
            WriteRegisterCommand = new WriteRegisterCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                OnPropertyChanged(nameof(IsEnable));
            }
        }

        public string ReadInput
        {
            get { return _readInput; }
            set
            {
                _readInput = value;
                OnPropertyChanged(nameof(ReadInput));
            }
        }

        public string ReadOutput
        {
            get { return _readOutput; }
            set
            {
                _readOutput = value;
                OnPropertyChanged(nameof(ReadOutput));
            }
        }

        public ICommand ReadRegisterCommand { get; set; }

        public string WriteInput
        {
            get { return _writeInput; }
            set { _writeInput = value; }
        }

        public ICommand WriteRegisterCommand { get; set; }

        public string WriteValue
        {
            get { return _writeValue; }
            set { _writeValue = value; }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            IsEnable = true;
            OnPropertyChanged(nameof(IsDeviceSelected));
        }
    }
}