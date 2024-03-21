using ADIN.WPF.ViewModel;
using System.Threading.Tasks;

namespace ADIN.WPF.Commands
{
    public class RefreshCommand : CommandBase
    {
        private DeviceListingViewModel _viewModel;

        public RefreshCommand(DeviceListingViewModel deviceListingViewModel)
        {
            _viewModel = deviceListingViewModel;
        }

        public override void Execute(object parameter)
        {
            Task.Run(() =>
            {
                _viewModel.CheckConnectedDevice();
            });
        }
    }
}