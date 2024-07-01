// <copyright file="NavigationStore.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.ViewModel;
using System;

namespace ADIN.WPF.Stores
{
	public class NavigationStore
    {
		private ViewModelBase _currentViewModel;

		public ViewModelBase CurrentViewModel
		{
			get { return _currentViewModel; }
			set 
			{ 
				_currentViewModel = value;
				OnCurrentViewModelChanged();
			}
		}

		public event Action CurrentViewModelChanged;

		private void OnCurrentViewModelChanged()
		{
			CurrentViewModelChanged?.Invoke();
		}
	}
}
