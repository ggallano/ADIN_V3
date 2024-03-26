using ADIN.Device.Models;
using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class LinkPropertiesViewModel : ViewModelBase
    {
        public ICommand NavigateRegisterAccessCommand { get; }

        public NavigationBarViewModel NavigationBarViewModel { get; }

        public LinkPropertiesViewModel(NavigationStore navigationStore, NavigationBarViewModel navigationBarViewModel)
        {
            NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(new NavigationService<RegisterAccessViewModel>(navigationStore, () => new RegisterAccessViewModel(navigationStore, navigationBarViewModel)));
            NavigationBarViewModel = navigationBarViewModel;
        }
    }
}