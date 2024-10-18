// <copyright file="ADINChip.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public  class ADINChip
    {
        public int PhyAddress { get; set; }
        public uint ModelID { get; set; }
        public int PortNum { get; set; }
    }
}
