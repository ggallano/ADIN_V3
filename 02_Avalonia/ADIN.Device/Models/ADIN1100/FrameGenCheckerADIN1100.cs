// <copyright file="FrameGenCheckerADIN1100.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Device.Models.ADIN1100
{
    public class FrameGenCheckerADIN1100 : IFrameGenChecker
    {
        public FrameGenCheckerADIN1100()
        {
            EnableContinuousMode = false;
            EnableMacAddress = false;
            FrameBurst = 64001;
            FrameLength = 1250;

            FrameContents = new ObservableCollection<FrameContentModel>()
            {
                new FrameContentModel()
                {
                    Name = "Random",
                    FrameContentType = FrameType.Random
                },
                new FrameContentModel()
                {
                    Name = "All 0s",
                    FrameContentType = FrameType.All0s
                },
                new FrameContentModel()
                {
                    Name = "All 1s",
                    FrameContentType = FrameType.All1s
                },
                new FrameContentModel()
                {
                    Name = "All 10s",
                    FrameContentType = FrameType.Alt10s
                }
            };
            FrameContent = FrameContents[0];

            SrcMacAddress = null;
            DestMacAddress = null;
            FrameGeneratorButtonText = "Generate";
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
