using ADIN.Device.Models;
using ADIN.WPF.Commands.CableDiag;
using ADIN.WPF.Stores;
using ADIN.WPF.View;
using System;
using System.Windows;
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
            _selectedDeviceStore.PortNumChanged += _selectedDeviceStore_PortNumChanged;
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
            get 
            { 
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.CableCalibrationMessage; 
                else
                    return _faultDetectorPort2?.CableCalibrationMessage; 
            }
            set
            {
                _cableCalibrationMessage = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.CableCalibrationMessage = value;
                else
                    _faultDetectorPort2.CableCalibrationMessage = value;
                OnPropertyChanged(nameof(CableCalibrationMessage));
            }
        }

        public string CableFileName
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.CableFileName ?? "-"; 
                else
                    return _faultDetectorPort2?.CableFileName ?? "-"; 
            }
            set
            {
                _cableFileName = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.CableFileName = value;
                else
                    _faultDetectorPort2.CableFileName = value;
                OnPropertyChanged(nameof(CableFileName));
            }
        }

        public ICommand CalibrateCommand { get; set; }

        public string DistToFault
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.DistToFault ?? "0.00"; 
                else
                    return _faultDetectorPort2?.DistToFault ?? "0.00"; 
            }
            set
            {
                _distToFault = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.DistToFault = value;
                else
                    _faultDetectorPort2.DistToFault = value;
                OnPropertyChanged(nameof(DistToFault));
            }
        }

        public Brush FaultBackgroundBrush
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.FaultBackgroundBrush ?? new SolidColorBrush(Colors.LightGray); 
                else
                    return _faultDetectorPort2?.FaultBackgroundBrush ?? new SolidColorBrush(Colors.LightGray); 
            }
            set
            {
                _faultBackgroundBrush = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.FaultBackgroundBrush = value;
                else
                    _faultDetectorPort2.FaultBackgroundBrush = value;
                OnPropertyChanged(nameof(FaultBackgroundBrush));
            }
        }

        public ICommand FaultDetectCommand { get; set; }

        public string FaultState
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.FaultState ?? ""; 
                else
                    return _faultDetectorPort2?.FaultState ?? ""; 
            }
            set
            {
                _faultState = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.FaultState = value;
                else
                    _faultDetectorPort2.FaultState = value;
                OnPropertyChanged(nameof(FaultState));
            }
        }

        public ICommand InitializedCommand { get; set; }

        public bool IsFaultVisibility
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.IsFaultVisibility ?? false; 
                else
                    return _faultDetectorPort2?.IsFaultVisibility ?? false; 
            }
            set
            {
                _isFaultVisibility = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.IsFaultVisibility = value;
                else
                    _faultDetectorPort2.IsFaultVisibility = value;
                OnPropertyChanged(nameof(IsFaultVisibility));
            }
        }

        public bool IsOngoingCalibration
        {
            get { return _isOngoingCalibration; }
            set
            {
                _isOngoingCalibration = value;
                if (_isOngoingCalibration)
                    _selectedDeviceStore.OnOngoingCalibrationStatusChanged(value);
                else
                    _selectedDeviceStore.OnOngoingCalibrationStatusChanged(value);
                OnPropertyChanged(nameof(IsOngoingCalibration));
            }
        }

        public bool IsVisibleCableCalibration
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.IsVisibleCableCalibration == true; 
                else
                    return _faultDetectorPort2?.IsVisibleCableCalibration == true; 
            }
            set
            {
                _isVisibleCableCalibration = value;
                if (_selectedDevice?.PortNumber == 1)
                    _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.IsVisibleCableCalibration = value;
                else
                    _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.IsVisibleCableCalibration = value;
                OnPropertyChanged(nameof(IsVisibleCableCalibration));
            }
        }

        public bool IsVisibleOffsetCalibration
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.IsVisibleOffsetCalibration == true; 
                else
                    return _faultDetectorPort2?.IsVisibleOffsetCalibration == true; 
            }
            set
            {
                _isVisibleOffsetCalibration = value;
                if (_selectedDevice?.PortNumber == 1)
                    _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort1.IsVisibleOffsetCalibration = value;
                else
                    _selectedDeviceStore.SelectedDevice.TimeDomainReflectometryPort2.IsVisibleOffsetCalibration = value;
                OnPropertyChanged(nameof(IsVisibleOffsetCalibration));
            }
        }
        public ICommand LoadCommand { get; set; }

        public ICommand ManualCommand { get; set; }

        public decimal NvpValue
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _cableDiagnosticPort1?.NVP ?? 0.0M; 
                else
                    return _cableDiagnosticPort2?.NVP ?? 0.0M; 
            }
            set
            {
                if (_selectedDevice != null)
                {
                    _nvpValue = value;
                    if (_selectedDevice?.PortNumber == 1)
                        _cableDiagnosticPort1.NVP = value;
                    else
                        _cableDiagnosticPort2.NVP = value;
                }
                OnPropertyChanged(nameof(NvpValue));
            }
        }

        public string OffsetCalibrationMessage
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.OffsetCalibrationMessage; 
                else
                    return _faultDetectorPort2?.OffsetCalibrationMessage; 
            }
            set
            {
                _offsetCalibrationMessage = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.OffsetCalibrationMessage = value;
                else
                    _faultDetectorPort2.OffsetCalibrationMessage = value;
                OnPropertyChanged(nameof(OffsetCalibrationMessage));
            }
        }

        public string OffsetFileName
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _faultDetectorPort1?.OffsetFileName ?? "-"; 
                else
                    return _faultDetectorPort2?.OffsetFileName ?? "-"; 
            }
            set
            {
                _offsetFileName = value;
                if (_selectedDevice?.PortNumber == 1)
                    _faultDetectorPort1.OffsetFileName = value;
                else
                    _faultDetectorPort2.OffsetFileName = value;
                OnPropertyChanged(nameof(OffsetFileName));
            }
        }

        public decimal OffsetValue
        {
            get
            {
                if (_selectedDevice?.PortNumber == 1)
                    return _cableDiagnosticPort1?.CableOffset ?? 0.0M; 
                else
                    return _cableDiagnosticPort2?.CableOffset ?? 0.0M; 
            }
            set
            {
                if (_selectedDevice != null)
                {
                    _offsetValue = value;
                    if (_selectedDevice?.PortNumber == 1)
                        _cableDiagnosticPort1.CableOffset = value;
                    else
                        _cableDiagnosticPort2.CableOffset = value;
                }
                OnPropertyChanged(nameof(OffsetValue));
            }
        }

        public ICommand SaveCommand { get; set; }

        private TDRModel _cableDiagnosticPort1 => _selectedDevice?.TimeDomainReflectometryPort1?.TimeDomainReflectometry;

        private TDRModel _cableDiagnosticPort2 => _selectedDevice?.TimeDomainReflectometryPort2?.TimeDomainReflectometry;

        private ITimeDomainReflectometry _faultDetectorPort1 => _selectedDevice?.TimeDomainReflectometryPort1;

        private ITimeDomainReflectometry _faultDetectorPort2 => _selectedDevice?.TimeDomainReflectometryPort2;

        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.PortNumChanged -= _selectedDeviceStore_PortNumChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_PortNumChanged()
        {
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
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
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
