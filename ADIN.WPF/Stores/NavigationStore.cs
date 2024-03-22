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
