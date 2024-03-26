using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class NavigationBarViewModel : ViewModelBase
    {
        public ICommand NavigateLinkPropertiesCommand { get; }
        public ICommand NavigateRegisterAccessCommand { get; }

        public NavigationBarViewModel(NavigationService<LinkPropertiesViewModel> linkNavigationService, 
            NavigationService<RegisterAccessViewModel> registerAccessService)
        {
            NavigateLinkPropertiesCommand = new NavigateCommand<LinkPropertiesViewModel>(linkNavigationService);
            NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(registerAccessService);
        }
    }
}
