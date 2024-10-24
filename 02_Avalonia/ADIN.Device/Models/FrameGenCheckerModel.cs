// <copyright file="FrameGenCheckerModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public class FrameGenCheckerModel : IFrameGenChecker
    {
        public FrameGenCheckerModel()
        {
            FrameContents = new ObservableCollection<FrameContentModel>();
        }

        public string DestMacAddress { get; set; }
        public string DestOctet { get; set; }
        public bool EnableContinuousMode { get; set; }
        public bool EnableMacAddress { get; set; }
        public uint FrameBurst { get; set; }
        public FrameContentModel FrameContent { get; set; }
        public ObservableCollection<FrameContentModel> FrameContents { get; set; }
        public string FrameGeneratorButtonText { get; set; }
        public uint FrameLength { get; set; }
        public FrameType SelectedFrameContent { get; set; }
        public string SrcMacAddress { get; set; }
        public string SrcOctet { get; set; }
    }
}
