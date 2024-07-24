// <copyright file="ActiveLinkMonViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADIN.WPF.Stores;

namespace ADIN.WPF.ViewModel
{
    public class ActiveLinkMonViewModel : ViewModelBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private object mainLock;

        public ActiveLinkMonViewModel(SelectedDeviceStore selectedDeviceStore, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            mainLock = mainLock;
        }
    }
}
