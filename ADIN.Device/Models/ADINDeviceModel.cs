using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ADIN.Device.Models
{
    public class ADINDeviceModel
    {
        public ADINDeviceModel(string serialNumber, string name, IFTDIServices ftdiService, IRegisterService registerService)
        {
            SerialNumber = serialNumber;
            Name = name;

            uint phyAddress = 0;

            Registers = new ObservableCollection<RegisterModel>();
            FirmwareAPI = new ADIN2111FirmwareAPI(ftdiService);
            EvalBoardType = FirmwareAPI.GetModelNum(out phyAddress);
            RevNumber = FirmwareAPI.GetRevNum();

            // decide which json file based on the register value
            RegisterJsonFile = FirmwareAPI.GetRegisterJsonFile(RevNumber);

            // extracting the regsiter info in the json file
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", RegisterJsonFile));
            switch (RevNumber)
            {
                case BoardRevision.Rev0:
                    Registers = registerService.GetAdditionalRegisterSetRev0(Registers);
                    break;
                case BoardRevision.Rev1:
                    Registers = registerService.GetAdditionalRegisterSetRev1(Registers);
                    break;
                default:
                    break;
            }

            PhyAddress = phyAddress;
            FirmwareAPI = new ADIN2111FirmwareAPI(ftdiService, phyAddress, Registers, RevNumber, EvalBoardType);

            // get the registers from the board
            //Registers = FirmwareAPI.GetStatusRegisters(PhyAddress);

            LinkProperty = new LinkPropertyModel();
            FrameGenChecker = new FrameGenCheckerModel();
            TestMode = new TestModeModel();
            Loopback = new LoopbackModel();
            FaultDetector = new FaultDetectorModel();
            ActiveLink = new ActiveLinkModel();
        }

        public ActiveLinkModel ActiveLink { get; set; }
        public string AdvertisedSpeed => "10BASE-T1L";
        public string Checker { get; set; }
        public BoardType EvalBoardType { get; set; }
        public FaultDetectorModel FaultDetector { get; set; }
        public IFirmwareAPI FirmwareAPI { get; set; }
        public FrameGenCheckerModel FrameGenChecker { get; set; }
        public LinkPropertyModel LinkProperty { get; set; }
        public LoopbackModel Loopback { get; set; }
        public string Name { get; }
        public uint PhyAddress { get; set; }
        public EthPhyState PhyStateStatus { get; set; }
        public string RegisterJsonFile { get; set; }
        public ObservableCollection<RegisterModel> Registers { get; set; }
        public BoardRevision RevNumber { get; set; }
        public string SerialNumber { get; }
        public TestModeModel TestMode { get; set; }
    }
}