using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ADIN.WPF.ViewModel
{
    public class ActiveLinkMonitoringViewModel : ViewModelBase
    {
        private ObservableCollection<IAnnotationViewModel> _annotations;
        private string _busyContent;
        private string _currentLinkLength;
        private ObservableCollection<IRenderableSeriesViewModel> _graphPlots;
        private bool _isActiveLinkButtonEnable;
        private bool _isActiveLinkEnable;
        private bool _isLinkLengthVisible;
        private bool _isMseBenchmarkVisible;
        private bool _isOngoingCalibration;
        private string _linkLength;
        private string _linkLengthBenchmarkvalue;
        private string _mseBenchmarkValue;
        private IDataSeries<double, double> _mseLineData;
        private LineRenderableSeriesViewModel _mseLineRenderableSeries;
        private string _mseValue;
        private SelectedDeviceStore _selectedDeviceStore;
        private double _yMax = 0;
        private double _yMin = 0;
        private double dt = 1.0d;
        private double t;
        private IRange _xVisibleRange;
        private IRange _yVisibleRange;
        public ActiveLinkMonitoringViewModel(SelectedDeviceStore selectedDeviceStore)
        {
            _selectedDeviceStore = selectedDeviceStore;

            _graphPlots = new ObservableCollection<IRenderableSeriesViewModel>();
            _mseLineData = new XyDataSeries<double, double>();
            _mseLineData.AcceptsUnsortedData = true;
            _mseLineRenderableSeries = new LineRenderableSeriesViewModel();
            _mseLineRenderableSeries.StrokeThickness = 2;
            _mseLineRenderableSeries.Stroke = Colors.Blue;
            _mseLineRenderableSeries.DataSeries = _mseLineData;
            _graphPlots.Add(_mseLineRenderableSeries);

            LinkLengthSetCommand = new LinkLengthSetCommand(this, selectedDeviceStore);
            MseBenchmarkSetCommand = new MseBenchmarkSetCommand(this, selectedDeviceStore);

            _annotations = new ObservableCollection<IAnnotationViewModel>();
            _isMseBenchmarkVisible = false;
            _isLinkLengthVisible = false;

            _isActiveLinkButtonEnable = false;
            _linkLength = "--";

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.MseValueChanged += _selectedDeviceStore_MseValueChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.LinkLengthChanged += _selectedDeviceStore_LinkLengthChanged;
        }

        public ObservableCollection<IAnnotationViewModel> Annotations
        {
            get { return _selectedDevice?.ActiveLink.Annotations ?? null; }
            set
            {
                _annotations = value;
                _activeLink.Annotations = value;
            }
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

        public ObservableCollection<IRenderableSeriesViewModel> GraphPlots
        {
            get { return _graphPlots; }
            set { _graphPlots = value; }
        }

        public bool IsActiveLinkButtonEnable
        {
            get { return _isActiveLinkButtonEnable; }
            set
            {
                _isActiveLinkButtonEnable = value;
                OnPropertyChanged(nameof(IsActiveLinkButtonEnable));
            }
        }

        public bool IsActiveLinkEnable
        {
            get { return _selectedDevice?.ActiveLink.IsActiveLinkEnable ?? false; }
            set
            {
                _isActiveLinkEnable = value;
                _activeLink.IsActiveLinkEnable = value;
                OnPropertyChanged(nameof(IsActiveLinkEnable));
            }
        }

        public bool IsLinkLengthVisible
        {
            get { return _selectedDevice?.ActiveLink.IsLinkLengthVisible ?? false; }
            set
            {
                _isLinkLengthVisible = value;
                _activeLink.IsLinkLengthVisible = value;
                OnPropertyChanged(nameof(IsLinkLengthVisible));
            }
        }

        public bool IsMseBenchmarkVisible
        {
            get { return _selectedDevice?.ActiveLink.IsMseBenchmarkVisible ?? false; ; }
            set
            {
                _isMseBenchmarkVisible = value;
                _activeLink.IsMseBenchmarkVisible = value;
                OnPropertyChanged(nameof(IsMseBenchmarkVisible));
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

        public string LinkLength
        {
            get { return _linkLength; }
            set
            {
                _linkLength = value + " m";
                OnPropertyChanged(nameof(LinkLength));
            }
        }

        public string LinkLengthBenchmarkValue
        {
            get { return _selectedDevice?.ActiveLink.LinkLengthBenchMark + " m" ?? "N/A"; }
            set
            {
                _linkLengthBenchmarkvalue = value;
                _activeLink.LinkLengthBenchMark = value;
                OnPropertyChanged(nameof(LinkLengthBenchmarkValue));
            }
        }

        public ICommand LinkLengthSetCommand { get; set; }
        public string LinkStatus
        {
            get { return _currentLinkLength; }
            set
            {
                _currentLinkLength = value;
                OnPropertyChanged(nameof(LinkStatus));
            }
        }

        public ICommand MseBenchmarkSetCommand { get; set; }
        public string MseBenchmarkValue
        {
            get { return _selectedDevice?.ActiveLink.MseBenchmark ?? "N/A"; }
            set
            {
                _mseBenchmarkValue = value;
                _activeLink.MseBenchmark = value;
                OnPropertyChanged(nameof(MseBenchmarkValue));
            }
        }

        public IDataSeries<double, double> MseLineData
        {
            get { return _mseLineData; }
            set
            {
                _mseLineData = value;
                OnPropertyChanged(nameof(MseLineData));
            }
        }

        public string MseValue
        {
            get { return _mseValue; }
            set
            {
                _mseValue = value;
                OnPropertyChanged(nameof(MseValue));
            }
        }

        public IRange XVisibleRange
        {
            get { return _xVisibleRange; }
            set
            {
                _xVisibleRange = value;
                OnPropertyChanged(nameof(XVisibleRange));
            }
        }

        public IRange YVisibleRange
        {
            get { return _yVisibleRange; }
            set
            {
                _yVisibleRange = value;
                OnPropertyChanged(nameof(YVisibleRange));
            }
        }

        private ActiveLinkModel _activeLink => _selectedDevice.ActiveLink;
        private ADINDeviceModel _selectedDevice => _selectedDeviceStore.SelectedDevice;
        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.MseValueChanged -= _selectedDeviceStore_MseValueChanged;
            _selectedDeviceStore.LinkStatusChanged -= _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.LinkLengthChanged -= _selectedDeviceStore_LinkLengthChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_LinkLengthChanged(string linkLength)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkLength = linkLength;
            }));
        }
        private void _selectedDeviceStore_LinkStatusChanged(string linkStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LinkStatus = linkStatus;
            }));
        }
        private void _selectedDeviceStore_MseValueChanged(string mseValue)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MseValue = mseValue;
                if (!mseValue.Contains("N/A") && !mseValue.Contains("∞"))
                {
                    var paddingPlot = 5;
                    var temp = double.Parse(mseValue.Replace("dB", "").Trim());

                    if (temp < _yMin)
                        _yMin = temp - paddingPlot;
                    if (temp > _yMax)
                        _yMax = temp + paddingPlot;


                    //YVisibleRange = new DoubleRange(_yMin, _yMax);
                    
                    _mseLineData.Append(t += dt, temp);
                    XVisibleRange = new DoubleRange(0, t);
                }
            }));
        }
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(LinkLengthBenchmarkValue));
            OnPropertyChanged(nameof(IsLinkLengthVisible));
            OnPropertyChanged(nameof(MseBenchmarkValue));
            OnPropertyChanged(nameof(IsMseBenchmarkVisible));
            OnPropertyChanged(nameof(Annotations));
            OnPropertyChanged(nameof(IsActiveLinkEnable));
            IsActiveLinkButtonEnable = true;
        }
    }
}