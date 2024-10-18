using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Avalonia.ViewModels
{
    public class DeviceListingItemViewModel : ViewModelBase
    {
        public DeviceListingItemViewModel(ADINDevice device)
        {
            Device = device;
            ImagePath = @"..\Images\icons\Applications-Industrial-Automation-Ethernet-Icon.png";
        }

        public BoardType BoardType => Device.DeviceType;

        public ADINDevice Device { get; }

        public string DeviceHeader
        {
            get
            {
                if (IsMultichipBoard)
                    return SerialNumber + " - " + BoardType.ToString();
                else if (BoardType == BoardType.ADIN2111)
                    return $"{SerialNumber} - Port {PortNum}";
                else
                    return SerialNumber;
            }
        }

        public string ImagePath { get; }

        public bool IsMultichipBoard => Device.IsMultichipBoard;

        public string Name => Device.Device.BoardName;

        public int PortNum => Device.PortNumber;

        public string SerialNumber => Device.Device.SerialNumber;
    }
}
