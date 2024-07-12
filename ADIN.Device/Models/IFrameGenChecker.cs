// <copyright file="IFrameGenChecker.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface IFrameGenChecker
    {
        string DestMacAddress { get; set; }
        string DestOctet { get; set; }
        bool EnableContinuousMode { get; set; }
        bool EnableMacAddress { get; set; }
        uint FrameBurst { get; set; }
        FrameContentModel FrameContent { get; set; }
        List<FrameContentModel> FrameContents { get; set; }
        uint FrameLength { get; set; }
        FrameType SelectedFrameContent { get; set; }
        string SrcMacAddress { get; set; }
        string SrcOctet { get; set; }
        string FrameGeneratorButtonText { get; set; }
    }
}
