using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class RegisterAccessViewModel : ViewModelBase
    {
        public ICommand NavigateLinkPorpCommand { get; set; }

        public NavigationBarViewModel NavigationBarViewModel { get; }

        public RegisterAccessViewModel(NavigationStore navigationStore, NavigationBarViewModel navigationBarViewModel)
        {
            NavigateLinkPorpCommand = new NavigateCommand<LinkPropertiesViewModel>(new NavigationService<LinkPropertiesViewModel>(navigationStore, () => new LinkPropertiesViewModel(navigationStore, navigationBarViewModel)));
            NavigationBarViewModel = navigationBarViewModel;
        }
    }
}