using ADIN.Avalonia.Stores;
using ADIN.Avalonia.ViewModels;
using ADIN.Register.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FTDIChip.Driver.Services;
using System.Threading;

namespace ADIN.Avalonia.Views;

public partial class DeviceListingView : UserControl
{
    public DeviceListingView()
    {
        InitializeComponent();
    }
}