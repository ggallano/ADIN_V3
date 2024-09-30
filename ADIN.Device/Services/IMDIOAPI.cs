// <copyright file="IMDIOAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Services
{
    public interface IMDIOAPI
    {
        string MdioReadCl22(uint regAddress);
        string MdioReadCl45(uint regAddress);
        string MdioWriteCl22(uint regAddress, uint data);
        string MdioWriteCl45(uint regAddress, uint data);
        string RegisterRead(uint regAddress);
        string RegisterRead(string register);
        string RegisterWrite(uint regAddress, uint data);
        void RegisterWrite(string register, uint data);
        string GetValue(string name);
    }
}
