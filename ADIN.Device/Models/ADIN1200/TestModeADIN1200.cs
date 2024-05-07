using ADIN.Device.Services;
using ADIN.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADIN.Device.Models.ADIN1200
{
    public class TestModeADIN1200 : ITestMode
    {
        public TestModeADIN1200()
        {
            TM100BaseTxVod = new TestModeListingModel();
            TM100BaseTxVod.Name1 = "100BASE-TX VOD";
            TM100BaseTxVod.Name2 = "100BASE-TX VOD";
            TM100BaseTxVod.Description = "100BASE-TX VOD measurements";

            TM10BaseTLinkPulse = new TestModeListingModel();
            TM10BaseTLinkPulse.Name1 = "10BASE-T Link Pulse";
            TM10BaseTLinkPulse.Name2 = "10BASE-T Link Pulse";
            TM10BaseTLinkPulse.Description = "10BASE-T forced mode in loopback with Tx suppression disabled, for link pulse measurements.";

            TM10BaseTTx5MHzDim1 = new TestModeListingModel();
            TM10BaseTTx5MHzDim1.Name1 = "10BASE-T TX 5 MHz DIM 1";
            TM10BaseTTx5MHzDim1.Name2 = "10BASE-T TX 5 MHz DIM 1";
            TM10BaseTTx5MHzDim1.Description = "Transmit 5MHz square wave on dimension 1";

            TM10BaseTTx10MHzDim1 = new TestModeListingModel();
            TM10BaseTTx10MHzDim1.Name1 = "10BASE-T TX 10 MHz DIM 1";
            TM10BaseTTx10MHzDim1.Name2 = "10BASE-T TX 10 MHz DIM 1";
            TM10BaseTTx10MHzDim1.Description = "Transmit 10MHz square wave on dimension 1";

            TM10BaseTTx5MHzDim0 = new TestModeListingModel();
            TM10BaseTTx5MHzDim0.Name1 = "10BASE-T TX 5 MHz DIM 0";
            TM10BaseTTx5MHzDim0.Name2 = "10BASE-T TX 5 MHz DIM 0";
            TM10BaseTTx5MHzDim0.Description = "Transmit 5MHz square wave on dimension 0";

            TM10BaseTTx10MHzDim0 = new TestModeListingModel();
            TM10BaseTTx10MHzDim0.Name1 = "10BASE-T TX 10 MHz DIM 0";
            TM10BaseTTx10MHzDim0.Name2 = "10BASE-T TX 10 MHz DIM 0";
            TM10BaseTTx10MHzDim0.Description = "Transmit 10MHz square wave on dimension 0";



            TestModes = new List<TestModeListingModel>()
            {
                TM100BaseTxVod,
                TM10BaseTLinkPulse,
                TM10BaseTTx5MHzDim1,
                TM10BaseTTx10MHzDim1,
                TM10BaseTTx5MHzDim0,
                TM10BaseTTx10MHzDim0
            };
            TestMode = TestModes[0];
        }

        public List<TestModeListingModel> TestModes { get; set; }
        public TestModeListingModel TestMode { get; set; }
        public TestModeListingModel TM100BaseTxVod { get; set; }
        public TestModeListingModel TM10BaseTLinkPulse { get; set; }
        public TestModeListingModel TM10BaseTTx5MHzDim1 { get; set; }
        public TestModeListingModel TM10BaseTTx10MHzDim1 { get; set; }
        public TestModeListingModel TM10BaseTTx5MHzDim0 { get; set; }
        public TestModeListingModel TM10BaseTTx10MHzDim0 { get; set; }
    }
}
