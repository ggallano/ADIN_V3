using ADI.Register.Services;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System.Globalization;
using System.Threading;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, IRegisterService registerService, object thisLock, ScriptService scriptService)
        {
            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("fr-FR");

            FrameGenCheckerViewModel = new FrameGenCheckerViewModel(selectedDeviceStore, thisLock, ftdiService);
            DeviceStatusViewModel = new DeviceStatusViewModel(selectedDeviceStore, ftdiService, thisLock, FrameGenCheckerViewModel);
            LogActivityViewModel = new LogActivityViewModel(selectedDeviceStore);

            DeviceListingViewModel = new DeviceListingViewModel(selectedDeviceStore, ftdiService, registerService, LogActivityViewModel, thisLock);
            LinkPropertiesViewModel = new LinkPropertiesViewModel(selectedDeviceStore, ftdiService);
            RegisterViewModel = new RegisterViewModel(selectedDeviceStore, ftdiService);
            TestModeViewModel = new TestModeViewModel(selectedDeviceStore);
            LoopbackViewModel = new LoopbackViewModel(selectedDeviceStore, ftdiService, LogActivityViewModel);
            ExtraCommandsViewModel = new ExtraCommandsViewModel(selectedDeviceStore, ftdiService);
            FaultDetectorViewModel = new FaultDetectorViewModel(selectedDeviceStore, thisLock);
            ActiveLinkMonitoringViewModel = new ActiveLinkMonitoringViewModel(selectedDeviceStore);
            MenuItemViewModel = new MenuItemViewModel();
            StatusStripViewModel = new StatusStripViewModel(selectedDeviceStore, scriptService);
            RegisterAccessViewModel = new RegisterAccessViewModel(selectedDeviceStore);
        }

        public ActiveLinkMonitoringViewModel ActiveLinkMonitoringViewModel { get; set; }
        public DeviceListingViewModel DeviceListingViewModel { get; }
        public DeviceStatusViewModel DeviceStatusViewModel { get; set; }
        public ExtraCommandsViewModel ExtraCommandsViewModel { get; set; }
        public FaultDetectorViewModel FaultDetectorViewModel { get; set; }
        public FrameGenCheckerViewModel FrameGenCheckerViewModel { get; set; }
        public LinkPropertiesViewModel LinkPropertiesViewModel { get; }
        public LogActivityViewModel LogActivityViewModel { get; set; }
        public LoopbackViewModel LoopbackViewModel { get; set; }
        public MenuItemViewModel MenuItemViewModel { get; set; }
        public RegisterAccessViewModel RegisterAccessViewModel { get; set; }
        public RegisterViewModel RegisterViewModel { get; set; }
        public StatusStripViewModel StatusStripViewModel { get; set; }
        public TestModeViewModel TestModeViewModel { get; set; }
    }
}