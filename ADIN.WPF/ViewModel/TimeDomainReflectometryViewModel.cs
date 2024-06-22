using ADIN.Device.Models;
using ADIN.WPF.Commands.CableDiag;
using ADIN.WPF.Stores;
using System.Windows.Input;
using System.Windows.Media;

namespace ADIN.WPF.ViewModel
{
    public class TimeDomainReflectometryViewModel : ViewModelBase
    {
        private string _busyContent;
        private Brush _cableBackgroundBrush;
        private string _cableCalibrationMessage;
        private string _cableFileName;
        private string _distToFault;
        private Brush _faultBackgroundBrush = new SolidColorBrush(Colors.LightGray);
        private string _faultState;
        private bool _isFaultVisibility;
        private bool _isOngoingCalibration;
        private bool _isVisibleCableCalibration = false;
        private bool _isVisibleOffsetCalibration = false;
        private decimal _nvpValue;
        private Brush _offsetBackgroundBrush;
        private string _offsetCalibrationMessage;
        private string _offsetFileName;
        private decimal _offsetValue;
        private SelectedDeviceStore _selectedDeviceStore;
        private object _thisLock;


        public TimeDomainReflectometryViewModel(SelectedDeviceStore selectedDeviceStore, object thisLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _thisLock = thisLock;

            IsOngoingCalibration = false;

            InitializedCommand = new InitializedCommand(this, _selectedDeviceStore);
            CalibrateCommand = new CalibrationCommand(this, _selectedDeviceStore, thisLock);
            SaveCommand = new TDRSaveCommand(this, _selectedDeviceStore);
            LoadCommand = new TDRLoadCommand(this, _selectedDeviceStore);
            ManualCommand = new TDRManualCommand(this, _selectedDeviceStore);
            FaultDetectCommand = new TDRFaultDetectorCommand(this, _selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set
            {
                _busyContent = value;
                OnPropertyChanged(nameof(BusyContent));
            }
        }
        public string CableCalibrationMessage
        {
            get { return _selectedDevice?.TimeDomainReflectometry.CableCalibrationMessage; }
            set
            {
                _cableCalibrationMessage = value;
                _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.CableCalibrationMessage = value;
                OnPropertyChanged(nameof(CableCalibrationMessage));
            }
        }

        public string CableFileName
        {
            get { return _selectedDevice?.TimeDomainReflectometry.CableFileName ?? "-"; }
            set
            {
                _cableFileName = value;
                _faultDetector.CableFileName = value;
                OnPropertyChanged(nameof(CableFileName));
            }
        }

        public ICommand CalibrateCommand { get; set; }

        public string DistToFault
        {
            get { return _selectedDevice?.TimeDomainReflectometry.DistToFault ?? "0.00"; }
            set
            {
                _distToFault = value;
                _faultDetector.DistToFault = value;
                OnPropertyChanged(nameof(DistToFault));
            }
        }

        public Brush FaultBackgroundBrush
        {
            get { return _selectedDevice?.TimeDomainReflectometry.FaultBackgroundBrush ?? new SolidColorBrush(Colors.LightGray); }
            set
            {
                _faultBackgroundBrush = value;
                _faultDetector.FaultBackgroundBrush = value;
                OnPropertyChanged(nameof(FaultBackgroundBrush));
            }
        }

        public ICommand FaultDetectCommand { get; set; }

        public string FaultState
        {
            get { return _selectedDevice?.TimeDomainReflectometry.FaultState ?? ""; }
            set
            {
                _faultState = value;
                _faultDetector.FaultState = value;
                OnPropertyChanged(nameof(FaultState));
            }
        }

        public ICommand InitializedCommand { get; set; }

        public bool IsFaultVisibility
        {
            get { return _selectedDevice?.TimeDomainReflectometry.IsFaultVisibility ?? false; }
            set
            {
                _isFaultVisibility = value;
                _faultDetector.IsFaultVisibility = value;
                OnPropertyChanged(nameof(IsFaultVisibility));
            }
        }

        public bool IsOngoingCalibration
        {
            get { return _isOngoingCalibration; }
            set
            {
                _isOngoingCalibration = value;
                OnPropertyChanged(nameof(IsOngoingCalibration));
            }
        }

        public bool IsVisibleCableCalibration
        {
            get { return _selectedDevice?.TimeDomainReflectometry.IsVisibleCableCalibration ?? false; }
            set
            {
                _isVisibleCableCalibration = value;
                _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.IsVisibleCableCalibration = value;
                OnPropertyChanged(nameof(IsVisibleCableCalibration));
            }
        }

        public bool IsVisibleOffsetCalibration
        {
            get { return _selectedDevice?.TimeDomainReflectometry.IsVisibleOffsetCalibration ?? false; }
            set
            {
                _isVisibleOffsetCalibration = value;
                _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.IsVisibleOffsetCalibration = value;
                OnPropertyChanged(nameof(IsVisibleOffsetCalibration));
            }
        }
        public ICommand LoadCommand { get; set; }

        public ICommand ManualCommand { get; set; }

        public decimal NvpValue
        {
            get { return _cableDiagnostic?.NVP ?? 0.0M; }
            set
            {
                if (_selectedDevice != null)
                {
                    _nvpValue = value;
                    _cableDiagnostic.NVP = value;
                }
                OnPropertyChanged(nameof(NvpValue));
            }
        }

        public string OffsetCalibrationMessage
        {
            get { return _selectedDevice?.TimeDomainReflectometry.OffsetCalibrationMessage ?? string.Empty; }
            set
            {
                _offsetCalibrationMessage = value;
                _selectedDeviceStore.SelectedDevice.TimeDomainReflectometry.OffsetCalibrationMessage = value;
                OnPropertyChanged(nameof(OffsetCalibrationMessage));
            }
        }

        public string OffsetFileName
        {
            get { return _selectedDevice?.TimeDomainReflectometry.OffsetFileName ?? "-"; }
            set
            {
                _offsetFileName = value;
                _faultDetector.OffsetFileName = value;
                OnPropertyChanged(nameof(OffsetFileName));
            }
        }

        public decimal OffsetValue
        {
            get { return _cableDiagnostic?.CableOffset ?? 0.0M; }
            set
            {
                if (_selectedDevice != null)
                {
                    _offsetValue = value;
                    _cableDiagnostic.CableOffset = value;
                }
                OnPropertyChanged(nameof(OffsetValue));
            }
        }

        public ICommand SaveCommand { get; set; }
        private TDRModel _cableDiagnostic => _selectedDevice?.TimeDomainReflectometry.TimeDomainReflectometry;
        private ITimeDomainReflectometry _faultDetector => _selectedDevice.TimeDomainReflectometry;
        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            if ((_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1200)
             || (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1300))
                return;

            OnPropertyChanged(nameof(FaultBackgroundBrush));
            OnPropertyChanged(nameof(IsVisibleCableCalibration));
            OnPropertyChanged(nameof(CableCalibrationMessage));
            OnPropertyChanged(nameof(IsVisibleOffsetCalibration));
            OnPropertyChanged(nameof(OffsetCalibrationMessage));

            OnPropertyChanged(nameof(OffsetValue));
            OnPropertyChanged(nameof(NvpValue));
            OnPropertyChanged(nameof(IsFaultVisibility));
            OnPropertyChanged(nameof(IsOngoingCalibration));
            OnPropertyChanged(nameof(BusyContent));
            OnPropertyChanged(nameof(CableFileName));
            OnPropertyChanged(nameof(OffsetFileName));
            OnPropertyChanged(nameof(DistToFault));
            OnPropertyChanged(nameof(FaultState));
        }
    }
}
