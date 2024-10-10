using FTD2XX_NET;
using System;

namespace FTDIChip.Driver.Services
{
    public interface IFTDIServices
    {
        event EventHandler<string> ProcessCompleted;

        bool IsComOpen { get; }

        FTDI.FT_DEVICE_INFO_NODE[] GetDeviceList();

        void Open(string serialNumber);

        void Close();

        void SendData(string dataToWrite);

        string ReadCommandResponse();

        void Purge();

        string GetSerialNumber();
    }
}
