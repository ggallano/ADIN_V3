// <copyright file="TestModeADIN1300.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Models;
using System.Collections.Generic;

namespace ADIN.Device.Models.ADIN1300
{
    public class TestModeADIN1300 : ITestMode
    {
        public TestModeADIN1300()
        {
            TM100BaseTxVod = new TestModeListingModel();
            TM100BaseTxVod.Name1 = "100BASE-TX VOD";
            TM100BaseTxVod.Name2 = "";
            TM100BaseTxVod.Description = "100BASE-TX VOD measurements";
            TM100BaseTxVod.IsRequiringFrameLength = false;

            TM10BaseTLinkPulse = new TestModeListingModel();
            TM10BaseTLinkPulse.Name1 = "10BASE-T Link Pulse";
            TM10BaseTLinkPulse.Name2 = "";
            TM10BaseTLinkPulse.Description = "10BASE-T forced mode in loopback with Tx suppression disabled, for link pulse measurements.";
            TM10BaseTLinkPulse.IsRequiringFrameLength = false;

            TM10BaseTTxRndFrm = new TestModeListingModel();
            TM10BaseTTxRndFrm.Name1 = "10BASE-T TX Random Frames";
            TM10BaseTTxRndFrm.Name2 = "";
            TM10BaseTTxRndFrm.Description = "10BASE-T forced mode in loopback with Tx suppression disabled, with Tx of random payloads.";
            TM10BaseTTxRndFrm.IsRequiringFrameLength = true;

            TM10BaseTtx0xffFrm = new TestModeListingModel();
            TM10BaseTtx0xffFrm.Name1 = "10BASE-T TX 0xFF Frames";
            TM10BaseTtx0xffFrm.Name2 = "";
            TM10BaseTtx0xffFrm.Description = "10BASE-T forced mode in loopback with Tx suppression disabled, with Tx of 0xFF payloads.";
            TM10BaseTtx0xffFrm.IsRequiringFrameLength = true;

            TM10BaseTtx0x00Frm = new TestModeListingModel();
            TM10BaseTtx0x00Frm.Name1 = "10BASE-T TX 0x00 Frames";
            TM10BaseTtx0x00Frm.Name2 = "";
            TM10BaseTtx0x00Frm.Description = "10BASE-T forced mode in loopback with Tx suppression disabled, with Tx of 0x00 payloads.";
            TM10BaseTtx0x00Frm.IsRequiringFrameLength = true;

            TM10BaseTTx5MHzDim1 = new TestModeListingModel();
            TM10BaseTTx5MHzDim1.Name1 = "10BASE-T TX 5 MHz DIM 1";
            TM10BaseTTx5MHzDim1.Name2 = "";
            TM10BaseTTx5MHzDim1.Description = "Transmit 5MHz square wave on dimension 1";
            TM10BaseTTx5MHzDim1.IsRequiringFrameLength = false;

            TM10BaseTTx10MHzDim1 = new TestModeListingModel();
            TM10BaseTTx10MHzDim1.Name1 = "10BASE-T TX 10 MHz DIM 1";
            TM10BaseTTx10MHzDim1.Name2 = "";
            TM10BaseTTx10MHzDim1.Description = "Transmit 10MHz square wave on dimension 1";
            TM10BaseTTx10MHzDim1.IsRequiringFrameLength = false;

            TM10BaseTTx5MHzDim0 = new TestModeListingModel();
            TM10BaseTTx5MHzDim0.Name1 = "10BASE-T TX 5 MHz DIM 0";
            TM10BaseTTx5MHzDim0.Name2 = "";
            TM10BaseTTx5MHzDim0.Description = "Transmit 5MHz square wave on dimension 0";
            TM10BaseTTx5MHzDim0.IsRequiringFrameLength = false;

            TM10BaseTTx10MHzDim0 = new TestModeListingModel();
            TM10BaseTTx10MHzDim0.Name1 = "10BASE-T TX 10 MHz DIM 0";
            TM10BaseTTx10MHzDim0.Name2 = "";
            TM10BaseTTx10MHzDim0.Description = "Transmit 10MHz square wave on dimension 0";
            TM10BaseTTx10MHzDim0.IsRequiringFrameLength = false;

            TestModes = new List<TestModeListingModel>()
            {
                TM100BaseTxVod,
                TM10BaseTLinkPulse,
                TM10BaseTTxRndFrm,
                TM10BaseTtx0xffFrm,
                TM10BaseTtx0x00Frm,
                TM10BaseTTx5MHzDim1,
                TM10BaseTTx10MHzDim1,
                TM10BaseTTx5MHzDim0,
                TM10BaseTTx10MHzDim0
            };
        }

        public List<TestModeListingModel> TestModes { get; set; }
        public TestModeListingModel TestMode { get; set; }
        public uint TestModeFrameLength { get; set; }
        public TestModeListingModel TM100BaseTxVod { get; set; }
        public TestModeListingModel TM10BaseTLinkPulse { get; set; }
        public TestModeListingModel TM10BaseTTx5MHzDim1 { get; set; }
        public TestModeListingModel TM10BaseTTx10MHzDim1 { get; set; }
        public TestModeListingModel TM10BaseTTx5MHzDim0 { get; set; }
        public TestModeListingModel TM10BaseTTx10MHzDim0 { get; set; }
        public TestModeListingModel TM10BaseTTxRndFrm { get; set; }
        public TestModeListingModel TM10BaseTtx0xffFrm { get; set; }
        public TestModeListingModel TM10BaseTtx0x00Frm { get; set; }
    }
}
