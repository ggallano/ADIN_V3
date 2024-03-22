using ADIN.Device.Models;
using ADIN.WPF.Commands;
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

        public LinkPropertiesViewModel(NavigationStore navigationStore)
        {
            NavigateRegisterAccessCommand = new NavigateCommand<RegisterAccessViewModel>(navigationStore, ()=> new RegisterAccessViewModel(navigationStore));
        }
    }
}