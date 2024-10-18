// <copyright file="IFTDIServices.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using FTD2XX_NET;

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
