// <copyright file="IPhyMode.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public interface IPhyMode
    {
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }
        public ObservableCollection<string> PhyModes { get; set; }
    }
}
