using ADI.Register.Services;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using ADIN.WPF.ViewModel;
using FTDIChip.Driver.Services;
using System.Windows;

namespace ADIN.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly SelectedDeviceStore _selectedDeviceStore;
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private ScriptService _scriptService;
        private object _mainLock = new object();
        private readonly NavigationStore _navigationStore;

        public App()
        {
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(
@"<LicenseContract>
  <Customer>Analog Devices</Customer>
  <OrderId>ABTSOFT-2101</OrderId>
  <LicenseCount>2</LicenseCount>
  <IsTrialLicense>false</IsTrialLicense>
  <SupportExpires>04/06/2021 00:00:00</SupportExpires>
  <ProductCode>SC-WPF-2D-PRO</ProductCode>
  <KeyCode>lwABAQEAAADDGDoIhKvYAQIAgQBDdXN0b21lcj1BbmFsb2cgRGV2aWNlcztPcmRlcklkPUFCVFNPRlQtMjEwMTtTdWJzY3JpcHRpb25WYWxpZFRvPTA2LUFwci0yMDIxO1Byb2R1Y3RDb2RlPVNDLVdQRi0yRC1QUk87TnVtYmVyRGV2ZWxvcGVyc092ZXJyaWRlPTIvbfxtBnZQ1onPo/zk58nxBov5AWYi5eCgBLbb8G0jrt6nKRQ/2ilMkx7+vFUOQIM=</KeyCode>
</LicenseContract>");

            _selectedDeviceStore = new SelectedDeviceStore();
            _navigationStore = new NavigationStore();
            _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);
            _ftdiService = new FTDIServices();
            _scriptService = new ScriptService();
            _registerService = new RegisterService();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainWindow()
            {
                Title = "Explore your Ethernet PHY!",
                WindowStartupLocation=WindowStartupLocation.CenterScreen,
                DataContext = new OperationViewModel(_selectedDeviceStore, _ftdiService, _navigationStore, _registerService, _scriptService, _mainLock)
            };

            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
