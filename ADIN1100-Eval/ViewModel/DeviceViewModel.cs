// <copyright file="DeviceViewModel.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using Commands;
    using DeviceCommunication;
    using Model;
    using TargetInterface;
    using Utilities.Feedback;
    using static TargetInterface.FirmwareAPI;
    using Utilities.JSONParser.JSONClasses;
    using System.Windows.Media;
    using Utilities.JSONParser;
    using System.IO;
    using TargetInterface.Parameters;
    using TargetInterface.CableDiagnostics;
    using Microsoft.Win32;
    using System.Text;

    /// <summary>
    /// Device View Model
    /// </summary>
    public class DeviceViewModel : FeedbackPropertyChange
    {
        /// <summary>
        /// Stores the lock object for synchronization variables between UI and worker thread
        /// </summary>
        private static object syncThreadVarLock = new object();
        private bool workerinvalidateRequerySuggested = false;
        private bool newDevicesAdded = false;
        private uint testModeFrameLength = 1500;
        private BackgroundWorker workerPhyStatus;
        private BackgroundWorker workerRefreshRegisters;
        private DeviceModel selectedDevice;
        private TestModeItem selectedTestModeItem;
        private TargetSettings deviceSettings = new TargetSettings();
        private ObservableCollection<DeviceModel> devices = new ObservableCollection<DeviceModel>();
        private ObservableCollection<TestModeItem> testmodeitemsADIN1200 = new ObservableCollection<TestModeItem>();
        private ObservableCollection<TestModeItem> testmodeitemsADIN1300 = new ObservableCollection<TestModeItem>();
        private ObservableCollection<TestModeItem> testmodeitemsADIN1100 = new ObservableCollection<TestModeItem>();
        private ObservableCollection<LoopbackItem> loopbackItemsADIN1100 = new ObservableCollection<LoopbackItem>();
        private RegisterDetails selectedRegister;
        private JSONParserEngine jsonParser = new JSONParserEngine();
        private ScriptJSONStructure selectedScript1;
        private string selectedScript2 = "Please Choose";
        private string selectedScript3 = "Please Choose";
        private string selectedScript4 = "Please Choose";
        private FieldDetails selectedField;
        private string writeRegisterAddress;
        private string writeRegisterValue;
        private string readRegisterAddress;
        private string readRegisterValue;
        private int frameBurst;
        private int frameLength;
        private int selectedIndexFrameContent;
        private bool enableMacAddress;
        private string srcMacAddress;
        private string destMacAddress;
        private LoopbackItem selectedLoopbackItem;
        private bool txSuppression;
        private bool rxSuppression;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewModel"/> class.
        /// </summary>
        public DeviceViewModel()
        {
            DeviceConnection.DeviceSerialNumbersChanged += this.DeviceConnection_DeviceSerialNumbersChanged;
            this.RunScriptCommand = new BindingCommand(this.DoRunScript, this.CanDoRunScript);
            this.ExecuteTestCommand = new BindingCommand(this.DoExecuteTest, this.CanDoExecuteTest);
            this.ExecuteFrameCheckerCommand = new BindingCommand(this.DoExecuteFrameChecker, this.CanDoExecuteFrameChecker);
            this.ResetFrameCheckerCommand = new BindingCommand(this.DoResetFrameChecker, this.CanDoResetFrameChecker);
            this.RegisterActionCommand = new BindingCommand(this.DoRegisterAction, this.CanDoRegisterAction);
            this.RestartAutoNegotiationCommand = new BindingCommand(this.DoRestartAutoNeg, this.CanDoRestartAutoNeg);
            this.RunCableDiagnosticsCommand = new BindingCommand(this.DoRunCableDiagnostics, this.CanDoRunCableDiagnostics);
            this.EnableLinkingCommand = new BindingCommand(this.DoEnableLinking, this.CanDoEnableLinking);
            this.SoftwarePowerDownCommand = new BindingCommand(this.DoSoftwarePowerDown, this.CanDoSoftwarePowerDown);
            this.ReadRegisterCommand = new BindingCommand(this.ReadRegister, this.CanReadRegister);
            this.WriteRegisterCommand = new BindingCommand(this.WriteRegister, this.CanWriteRegister);
            this.SoftwareResetCommand = new BindingCommand(this.DoSoftwareReset, this.CanDoSoftwareReset);
            this.GP_CLKPinCommand = new BindingCommand(this.DoGP_CLKPin, this.CanDoGP_CLKPin);
            this.CLK25_REFPinCommand = new BindingCommand(this.DoCLK25_REFPin, this.CanDoCLK25_REFPin);
            this.RemoteLoopbackCommand = new BindingCommand(this.DoRemoteLoopback, this.CanDoRemoteLoopback);
            this.LocalLoopbackCommand = new BindingCommand(this.DoLocalLoopback, this.CanDoLocalLoopback);
            this.RxSuppressionCommand = new BindingCommand(this.DoRxSuppression);
            this.TxSuppressionCommand = new BindingCommand(this.DoTxSuppression);
            this.RunFaultDetectionCommand = new BindingCommand(this.DoFaultDetection);
            this.ResetFaultDetectorCommand = new BindingCommand(this.DoResetFaultDetection);
            this.FaultDetectionCalibrateCommand = new BindingCommand(this.DoFaultDetectionCalibrate);
            this.CalibrateSaveCommand = new BindingCommand(this.DoCalibrateSave);
            this.CalibrateLoadCommand = new BindingCommand(this.DoCalibrateLoad);


            this.deviceSettings.ClearPropertiesChangedList();
            this.deviceSettings.PropertyChanged += this.DeviceSettings_PropertyChanged;

            this.InitializedWorkerPhyStatus();
            this.InitializedWorkerRefreshRegisters();

            this.testmodeitemsADIN1100.Add(new TestModeItem("10BASE-T1L Normal mode", "", "PHY is in normal operation", true));
            this.testmodeitemsADIN1100.Add(new TestModeItem("10BASE-T1L Test mode 1: ", "Tx output voltage, Tx clock frequency and jitter.", "PHY repeatedly transmit the data symbol sequence (+1, -1).", true));
            this.testmodeitemsADIN1100.Add(new TestModeItem("10BASE-T1L Test mode 2: ", "Tx output droop", "PHY transmit ten '+ 1' symbols followed by ten ' - 1' symbols.", true));
            this.testmodeitemsADIN1100.Add(new TestModeItem("10BASE-T1L Test mode 3: ", "Power Spectral Density (PSD) and power level", "PHY transmit as in non-test operation and in the MASTER data mode with data set to normal Inter-Frame idle signals.", true));
            this.testmodeitemsADIN1100.Add(new TestModeItem("10BASE-T1L Transmit Disable: ", "MDI Return Loss", "PHY's receive and transmit paths remain as in normal operation but PHY transmits 0 symbols continuously.", true));

            this.testmodeitemsADIN1200.Add(new TestModeItem("100BASE-TX VOD", "", "100BASE-TX VOD measurements.", false));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T Link Pulse", "", "10BASE-T forced mode in loopback with Tx suppression disabled, for link pulse measurements.", false));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX Random Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of random payloads.", true));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 0xFF Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of 0xFF payloads", true));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 0x00 Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of 0x00 payloads", true));

            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 5 MHz DIM 1", "", "Transmit 5MHz square wave on dimension 1", false));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 10 MHz DIM 1", "", "Transmit 10MHz square wave on dimension 1", false));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 5 MHz DIM 0", "", "Transmit 5MHz square wave on dimension 0", false));
            this.testmodeitemsADIN1200.Add(new TestModeItem("10BASE-T TX 10 MHz DIM 0", "", "Transmit 10MHz square wave on dimension 1", false));

            this.testmodeitemsADIN1300.Add(new TestModeItem("100BASE-TX VOD", "", "100BASE-TX VOD measurements.", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("1000BASE-T Test mode 1", "", "Transmit waveform test", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("1000BASE-T Test mode 2", "", "Transmit jitter test in MASTER mode", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("1000BASE-T Test mode 3", "", "Transmit jitter test in SLAVE mode", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("1000BASE-T Test mode 4", "", "Transmitter distortion test", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T Link Pulse", "", "10BASE-T forced mode in loopback with Tx suppression disabled, for link pulse measurements.", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX Random Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of random payloads.", true));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 0xFF Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of 0xFF payloads", true));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 0x00 Frames", "", "10BASE-T forced mode in loopback with Tx suppression disabled,with TX of 0x00 payloads", true));

            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 5 MHz DIM 1", "", "Transmit 5MHz square wave on dimension 1", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 10 MHz DIM 1", "", "Transmit 10MHz square wave on dimension 1", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 5 MHz DIM 0", "", "Transmit 5MHz square wave on dimension 0", false));
            this.testmodeitemsADIN1300.Add(new TestModeItem("10BASE-T TX 10 MHz DIM 0", "", "Transmit 10MHz square wave on dimension 1", false));

            this.selectedTestModeItem = this.testmodeitemsADIN1100[0];

            // Loopback Items
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "None", Name = "OFF" });
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "MAC I/F Remote", Name = "MacRemote" });
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "MAC I/F", Name = "MAC" });
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "PCS", Name = "Digital" });
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "PMA", Name = "LineDriver" });
            this.loopbackItemsADIN1100.Add(new LoopbackItem() { Content = "External MII/RMII", Name = "ExtCable" });
            this.LoopbackItems = this.loopbackItemsADIN1100;

            // Calibration.
            this.FaultState = "-";
        }

        
        /// <summary>
        /// Gets or sets the function to be called when running a script
        /// </summary>
        public BindingCommand RunScriptCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when executing test
        /// </summary>
        public BindingCommand ExecuteTestCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when executing FrameChecker
        /// </summary>
        public BindingCommand ExecuteFrameCheckerCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when resetting FrameChecker
        /// </summary>
        public BindingCommand ResetFrameCheckerCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing register action
        /// </summary>
        public BindingCommand RegisterActionCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when restarting auto-negotiaton
        /// </summary>
        public BindingCommand RestartAutoNegotiationCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when running cable diagnostics
        /// </summary>
        public BindingCommand RunCableDiagnosticsCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when enabling or disabling linking
        /// </summary>
        public BindingCommand EnableLinkingCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when enabling or disabling linking
        /// </summary>
        public BindingCommand SoftwarePowerDownCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when reading registers
        /// </summary>
        public BindingCommand ReadRegisterCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when writing registers
        /// </summary>
        public BindingCommand WriteRegisterCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing a software reset
        /// </summary>
        public BindingCommand SoftwareResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing an output to the GP_CLK pin
        /// </summary>
        public BindingCommand GP_CLKPinCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing a remote loopback
        /// </summary>
        public BindingCommand RemoteLoopbackCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing a local loopback
        /// </summary>
        public BindingCommand LocalLoopbackCommand { get; set; }

        /// <summary>
        /// Gets or sets the Rx Supression in loopback
        /// </summary>
        public BindingCommand RxSuppressionCommand { get; set; }

        /// <summary>
        /// Gets or sets the Tx Supression in loopback
        /// </summary>
        public BindingCommand TxSuppressionCommand { get; set; }

        /// <summary>
        /// Gets or sets the function to be called when performing an output to the CLK25_REF pin
        /// </summary>
        public BindingCommand CLK25_REFPinCommand { get; set; }

        /// <summary>
        /// Gets or sets the Fault Detector Command
        /// </summary>
        public BindingCommand RunFaultDetectionCommand { get; set; }

        /// <summary>
        /// Gets or sets the Reset Fault Detector Command
        /// </summary>
        public BindingCommand ResetFaultDetectorCommand { get; set; }

        /// <summary>
        /// Gets or sets the Calibrations
        /// </summary>
        public BindingCommand FaultDetectionCalibrateCommand { get; set; }

        /// <summary>
        /// Gets or sets the Calibrations to save.
        /// </summary>
        public BindingCommand CalibrateSaveCommand { get; set; }

        /// <summary>
        /// Gets or sets the Calibrations to load.
        /// </summary>
        public BindingCommand CalibrateLoadCommand { get; set; }

        
        /// <summary>
        /// Gets or sets the value to write to the register in the manual register window
        /// </summary>
        public string WriteRegisterValue
        {
            get
            {
                return this.writeRegisterValue;
            }

            set
            {
                this.writeRegisterValue = value;
                this.RaisePropertyChanged("WriteRegisterValue");
            }
        }

        /// <summary>
        /// Gets the value which should be displayed on the Frane Generator Button
        /// </summary>
        public string FrameGeneratorButtonText
        {
            get
            {
                if (this.deviceSettings.Link.FrameGenRunning)
                {
                    return "Terminate";
                }
                else
                {
                    return "Generate";
                }
            }
        }

        /// <summary>
        /// Gets or sets the value which was read in the manual register window
        /// </summary>
        public string ReadRegisterValue
        {
            get
            {
                return this.readRegisterValue;
            }

            set
            {
                this.readRegisterValue = value;
                this.RaisePropertyChanged("ReadRegisterValue");
            }
        }

        public string WriteRegisterAddress
        {
            get
            {
                return this.writeRegisterAddress;
            }

            set
            {
                if (value != null)
                {
                    this.writeRegisterAddress = value;
                    this.RaisePropertyChanged("WriteRegisterAddress");
                }
            }
        }

        public string ReadRegisterAddress
        {
            get
            {
                return this.readRegisterAddress;
            }

            set
            {
                this.readRegisterAddress = value;
                this.RaisePropertyChanged("ReadRegisterAddress");
            }
        }

        /// <summary>
        /// Test Mode Frame Length
        /// </summary>
        public uint TestModeFrameLength
        {
            get
            {
                return this.testModeFrameLength;
            }

            set
            {
                this.testModeFrameLength = value;
            }
        }

        /// <summary>
        /// Gets link button color
        /// </summary>
        public object LinkButtonColor
        {
            get
            {
                switch (this.deviceSettings.PhyState)
                {
                    case EthPhyState.Powerdown:
                        return new SolidColorBrush(Colors.Black);
                    case EthPhyState.Standby:
                        return new SolidColorBrush(Colors.Blue);
                    case EthPhyState.LinkDown:
                        return new SolidColorBrush(Colors.Orange);
                    case EthPhyState.LinkUp:
                        return new SolidColorBrush(Colors.Green);
                    default:
                        return new SolidColorBrush(Colors.Red);
                }
            }
        }

        /// <summary>
        /// Gets link foreground color
        /// </summary>
        public Brush ForegroundColor
        {
            get
            {
                switch (this.deviceSettings.PhyState)
                {
                    case EthPhyState.Powerdown:
                        return new SolidColorBrush(Colors.Black);
                    case EthPhyState.Standby:
                        return new SolidColorBrush(Colors.Blue);
                    case EthPhyState.LinkDown:
                        return new SolidColorBrush(Colors.Orange);
                    case EthPhyState.LinkUp:
                        return new SolidColorBrush(Colors.Green);
                    default:
                        return new SolidColorBrush(Colors.Red);
                }
            }
        }

        public ObservableCollection<RegisterDetails> Registers
        {
            get
            {
                if (this.SelectedDevice == null || this.SelectedDevice.FwAPI.Registers == null)
                {
                    return null;
                }

                try
                {
                    return this.SelectedDevice.FwAPI.Registers;
                }
                catch (FTDIException exc)
                {
                    this.Error(exc.Message);
                    return null;
                }
                catch (ApplicationException exc)
                {
                    this.Error(exc.Message);
                    return null;
                }
                catch (Exception exc)
                {
                    this.Error(exc.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of Scripts available
        /// </summary>
        public ObservableCollection<ScriptJSONStructure> Scripts { get; set; }

        /// <summary>
        /// Gets or sets selected register in the register window
        /// The register window is composed of Registers and sub fields and
        /// This will be called when the REGISTER changes
        /// </summary>
        public RegisterDetails SelectedRegister
        {
            get
            {
                return this.selectedRegister;
            }

            set
            {
                this.selectedRegister = value;
                this.RaisePropertyChanged("SelectedRegister");
            }
        }

        /// <summary>
        /// Gets or sets selected script 1
        /// </summary>
        public ScriptJSONStructure SelectedScript1
        {
            get
            {
                return this.selectedScript1;
            }

            set
            {
                if (this.selectedScript1 != value)
                {
                    this.selectedScript1 = value;
                    this.RaisePropertyChanged("SelectedScript1");
                }
            }
        }

        /// <summary>
        /// Gets or sets selected script 2
        /// </summary>
        public string SelectedScript2
        {
            get
            {
                return this.selectedScript2;
            }

            set
            {
                if (this.selectedScript2 != value)
                {
                    this.selectedScript2 = value;
                    this.RaisePropertyChanged("SelectedScript2");
                }
            }
        }

        /// <summary>
        /// Gets or sets selected script 3
        /// </summary>
        public string SelectedScript3
        {
            get
            {
                return this.selectedScript3;
            }

            set
            {
                if (this.selectedScript3 != value)
                {
                    this.selectedScript3 = value;
                    this.RaisePropertyChanged("SelectedScript3");
                }
            }
        }

        /// <summary>
        /// Gets or sets selected script 4
        /// </summary>
        public string SelectedScript4
        {
            get
            {
                return this.selectedScript4;
            }

            set
            {
                if (this.selectedScript4 != value)
                {
                    this.selectedScript4 = value;
                    this.RaisePropertyChanged("SelectedScript4");
                }
            }
        }

        /// <summary>
        /// Gets or sets selected field in the register window
        /// The register window is composed of Registers and sub fields and
        /// This will be called when the FIELD changes
        /// </summary>
        public FieldDetails SelectedField
        {
            get
            {
                return this.selectedField;
            }

            set
            {
                this.selectedField = value;
                this.RaisePropertyChanged("SelectedField");
            }
        }

        /// <summary>
        /// Gets or sets selected test mode item
        /// </summary>
        public TestModeItem SelectedTestModeItem
        {
            get
            {
                if (this.selectedDevice == null)
                {
                    return this.selectedTestModeItem;
                }
                else
                {
                    return this.selectedDevice.TestModeItem;
                }
            }

            set
            {
                this.selectedTestModeItem = value;
                if (this.selectedDevice != null)
                {
                    this.selectedDevice.TestModeItem = value;
                }

                this.RaisePropertyChanged("SelectedTestModeItem");
            }
        }

        /// <summary>
        /// gets or sets the collection of loopback items
        /// </summary>
        public ObservableCollection<LoopbackItem> LoopbackItems { get; set; }

        /// <summary>
        /// gets or sets the selected loopback item for Loopback
        /// </summary>
        public LoopbackItem SelectedLoopbackItem
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.selectedLoopbackItem;
                else
                    return this.selectedDevice.Loopback.LoopbackItem;
            }

            set
            {
                if (this.SelectedLoopbackItem != value)
                {
                    this.selectedLoopbackItem = value;
                    if (this.selectedDevice != null)
                    {
                        this.selectedDevice.Loopback.LoopbackItem = value;
                        this.DoLocalLoopback(null);
                    }

                    this.RaisePropertyChanged("SelectedLoopbackItem");
                }
            }
        }

        /// <summary>
        /// gets or sets the TxSuppression
        /// </summary>
        public bool TxSuppression
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.txSuppression;
                else
                    return this.selectedDevice.Loopback.TxSupression;
            }

            set
            {
                this.txSuppression = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.Loopback.TxSupression = value;
                this.RaisePropertyChanged("TxSuppression");
            }
        }

        /// <summary>
        /// gets or sets the RxSuppression
        /// </summary>
        public bool RxSuppression
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.rxSuppression;
                else
                    return this.selectedDevice.Loopback.RxSuspression;
            }

            set
            {
                this.rxSuppression = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.Loopback.RxSuspression = value;
                this.RaisePropertyChanged("RxSuppression");
            }
        }

        /// <summary>
        /// Gets or sets selected device
        /// </summary>
        public DeviceModel SelectedDevice
        {
            get
            {
                return this.selectedDevice;
            }

            set
            {
                /*
                This could be set in the UI thread when the user selects a device
                or in the background worker thread when the device list is
                */
                lock (this)
                {
                    if (this.selectedDevice != null)
                    {
                        try
                        {
                            this.selectedDevice.FwAPI.Close();
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }

                    this.selectedDevice = value;

                    if (this.selectedDevice != null)
                    {
                        try
                        {
                            this.selectedDevice.FwAPI.Open();
                            this.selectedDevice.FwAPI.DeviceSettings.FlagAllPropertiesChanged();

                            try
                            {
                                float nvpResult = 0.0f;
                                int cableOffsetResult = 0;
                                CalibrationMode modeResult = 0;

                                this.selectedDevice.FwAPI.ResetFaultDetection(out nvpResult, out cableOffsetResult, out modeResult);
                                this.CalibrateOffsetValue.Offset = cableOffsetResult;
                                this.CalibrateCableValue.NVP = nvpResult;
                            }
                            catch (Exception exc)
                            {
                                this.Error(exc.Message);
                            }
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }

                this.RaisePropertyChanged("SelectedDevice");
                this.RaisePropertyChanged("DeviceName");
                this.RaisePropertyChanged("DeviceSerialNumber");
                this.RaisePropertyChanged("Registers");
                this.RaisePropertyChanged("Scripts");
                this.RaisePropertyChanged("FrameBurst");
                this.RaisePropertyChanged("FrameLength");
                this.RaisePropertyChanged("SelectedIndexFrameContent");
                this.RaisePropertyChanged("SrcMacAddress");
                this.RaisePropertyChanged("DestMacAddress");
                this.RaisePropertyChanged("EnableMacAddress");
                this.RaisePropertyChanged("EnableContinuousMode");
                this.RaisePropertyChanged("SelectedTestModeItem");
                this.RaisePropertyChanged("SelectedLoopbackItem");
                this.RaisePropertyChanged("TxSuppression");
                this.RaisePropertyChanged("RxSuppression");
            }
        }

        /// <summary>
        /// Gets or sets device settings
        /// </summary>
        public TargetSettings DeviceSettings
        {
            get
            {
                return this.deviceSettings;
            }

            set
            {
                this.deviceSettings = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the frame generator is running or not
        /// </summary>
        public bool FrameGenRunning
        {
            get
            {
                return this.deviceSettings.Link.FrameGenRunning;
            }
        }

        private bool enableContinuousMode;

        /// <summary>
        /// gets or sets the Continuous Mode for Frame Generator/Checker
        /// </summary>
        public bool EnableContinuousMode
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.enableContinuousMode;
                else
                    return this.selectedDevice.FrameGenerator.IsContinuousMode;
            }

            set
            {
                this.enableContinuousMode = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.IsContinuousMode = value;
                this.RaisePropertyChanged("EnableContinuousMode");
            }
        }

        /// <summary>
        /// gets or sets the Frame Burst for Frame Generator/Checker
        /// </summary>
        public int FrameBurst
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.frameBurst;
                else
                    return this.selectedDevice.FrameGenerator.FramesBurst;
            }

            set
            {
                this.frameBurst = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.FramesBurst = value;
                this.RaisePropertyChanged("FrameBurst");
            }
        }

        /// <summary>
        /// gets or sets the Frame Length for Frame Generator/Checker
        /// </summary>
        public int FrameLength
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.frameLength;
                else
                    return this.selectedDevice.FrameGenerator.FrameLength;
            }

            set
            {
                this.frameLength = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.FrameLength = value;
                this.RaisePropertyChanged("FrameLength");
            }
        }

        /// <summary>
        /// gets or sets the Frame Content for Frame Generator/Checker
        /// </summary>
        public int SelectedIndexFrameContent
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.selectedIndexFrameContent;
                else
                    return this.selectedDevice.FrameGenerator.FrameContent;
            }

            set
            {
                this.selectedIndexFrameContent = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.FrameContent = value;

                this.RaisePropertyChanged("SelectedIndexFrameContent");
            }
        }

        /// <summary>
        /// gets or sets the enable MAC Address for Frame Generator/Checker
        /// </summary>
        public bool EnableMacAddress
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.enableMacAddress;
                else
                    return this.selectedDevice.FrameGenerator.MacAddressEnable;
            }

            set
            {
                this.enableMacAddress = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.MacAddressEnable = value;
                this.RaisePropertyChanged("EnableMacAddress");
            }
        }

        /// <summary>
        /// gets or sets the source MAC Address for Frame Generator/Checker
        /// </summary>
        public string SrcMacAddress
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.srcMacAddress;
                else
                    return this.selectedDevice.FrameGenerator.SourceMacAddress;
            }

            set
            {
                this.srcMacAddress = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.SourceMacAddress = value;

                this.RaisePropertyChanged("SrcMacAddress");
            }
        }

        /// <summary>
        /// gets or sets the destination MAC Address for Frame Generator/Checker
        /// </summary>
        public string DestMacAddress
        {
            get
            {
                if (this.selectedDevice == null)
                    return this.destMacAddress;
                else
                    return this.selectedDevice.FrameGenerator.DestinationMacAddress;
            }

            set
            {
                this.destMacAddress = value;
                if (this.selectedDevice != null)
                    this.selectedDevice.FrameGenerator.DestinationMacAddress = value;
                this.RaisePropertyChanged("DestMacAddress");
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public bool NegotiateSpeeds
        {
            get
            {
                return this.deviceSettings.EthSpeedSelection == EthSpeedMode.Advertised;
            }
        }

        /// <summary>
        /// Gets a value indicating whether negotiation speeds are shown
        /// </summary>
        public bool NegotiationSpeedsShown
        {
            get
            {
                return this.NegotiateSpeeds && !this.TenSPEDevice;
            }
        }

        /// <summary>
        /// Gets a value indicating whether forced speeds are shown
        /// </summary>
        public bool ForcedSpeedsShown
        {
            get
            {
                return !this.NegotiateSpeeds && !this.TenSPEDevice;
            }
        }

        /// <summary>
        /// Gets a value indicating whether 1 GB Speeds is possible
        /// </summary>
        public bool OneGBSpeedsPossible
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1300 || this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1301;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a 10SPE device
        /// </summary>
        public bool TenSPEDevice
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1100;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Clk25 ref pin is available
        /// </summary>
        public bool Clk25RefPinPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1300 || this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1301;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Clock Pin Window is available
        /// </summary>
        public bool ClockPinControlPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType != DeviceType.ADIN1100;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Loopback Window is available
        /// </summary>
        public bool LoopbackPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType != DeviceType.ADIN1100 || true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Test Modes Window is available
        /// </summary>
        public bool TestModesPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType != DeviceType.ADIN1100 || true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Frame Checker Window is available
        /// </summary>
        public bool FrameCheckerPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType != DeviceType.ADIN1100 || true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Cable Diagnostics Window is available
        /// </summary>
        public bool CableDiagnosticsPresent
        {
            get
            {
                return this.deviceSettings.ConnectedDeviceType == DeviceType.ADIN1100;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public bool Negotiate1GSpeeds
        {
            get
            {
                return this.deviceSettings.EthSpeedSelection == EthSpeedMode.Advertised && this.OneGBSpeedsPossible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public string LinkSpeed
        {
            get
            {
                if (this.deviceSettings.Link.LinkEstablished)
                {
                    return this.deviceSettings.Negotiate.SpeedString(this.DeviceSettings.Link.ResolvedHCD);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether link is established
        /// </summary>
        public bool LinkEstablished
        {
            get
            {
                return this.deviceSettings.Link.LinkEstablished;
            }
        }

        /// <summary>
        /// Gets a value indicating whether link is established
        /// </summary>
        public bool LinkModeVisible
        {
            get
            {
                return this.LinkEstablished && !this.TenSPEDevice;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public string Mode
        {
            get
            {
                return this.deviceSettings.EthSpeedSelection.ToString();
            }
        }

        /// <summary>
        /// gets DeviceName
        /// </summary>
        public string DeviceName
        {
            get
            {
                if (this.DeviceConnected)
                {
                    return this.selectedDevice.BoardName;
                }
                else
                {
                    return "N/A";
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public string DeviceSerialNumber
        {
            get
            {
                if (this.DeviceConnected)
                {
                    return this.selectedDevice.SerialNumber;
                }
                else
                {
                    return "N/A";
                }
            }
        }

        /// <summary>
        /// Master Slave preference should only be shown if Negotiation is enabled
        /// and a 1000 option is being advertised.
        /// </summary>
        public bool MasterSlavePreferenceShown
        {
            get
            {
                return this.NegotiateSpeeds && this.OneGBSpeedsPossible && (this.deviceSettings.Negotiate.AdvertiseEEE1000 || this.deviceSettings.Negotiate.Advertise1000FD || this.deviceSettings.Negotiate.Advertise1000HD);
            }
        }

        /// <summary>
        /// Master Slave preference should only be shown if a fixed speed of 1000 is being fixed
        /// and a 1000 option is being advertised.
        /// </summary>
        public bool MasterSlaveFixedShown
        {
            get
            {
                return !this.NegotiateSpeeds && this.OneGBSpeedsPossible && (this.deviceSettings.Fixed.ForcedSpeed == EthernetSpeeds.SPEED_1000BASE_T_FD || this.deviceSettings.Fixed.ForcedSpeed == EthernetSpeeds.SPEED_1000BASE_T_HD || this.deviceSettings.Fixed.ForcedSpeed == EthernetSpeeds.SPEED_1000BASE_T_EEE);
            }
        }

        /// <summary>
        /// Gets a value indicating whether AdvertiseEEE1000 is shown
        /// </summary>
        public bool ShowAdvertiseEEE1000
        {
            get
            {
                return this.NegotiateSpeeds && this.deviceSettings.Negotiate.Advertise1000FD && this.OneGBSpeedsPossible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether AdvertiseEEE100 is shown
        /// </summary>
        public bool ShowAdvertiseEEE100
        {
            get
            {
                return this.NegotiateSpeeds && this.deviceSettings.Negotiate.Advertise100FD;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public bool ShowDownspeed10
        {
            get
            {
                return this.NegotiateSpeeds && this.deviceSettings.Negotiate.DownspeedTo10Possible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public bool ShowDownspeed100
        {
            get
            {
                return this.NegotiateSpeeds && this.deviceSettings.Negotiate.DownspeedTo100Possible;
            }
        }

        /// <summary>
        /// Gets a value indicating whether speed negotiation is active
        /// </summary>
        public bool ShowDownspeedRetires
        {
            get
            {
                return this.ShowDownspeed10 || this.ShowDownspeed100;
            }
        }

        /// <summary>
        /// Gets gets or sets gets Collection of devices
        /// </summary>
        public ObservableCollection<DeviceModel> Devices
        {
            get
            {
                return this.devices;
            }
        }

        /// <summary>
        /// Gets gets or sets gets Collection of test mode items
        /// </summary>
        public ObservableCollection<TestModeItem> TestModeItems
        {
            get
            {
                if (this.TenSPEDevice)
                {
                    return this.testmodeitemsADIN1100;
                }
                else if (this.OneGBSpeedsPossible)
                {
                    return this.testmodeitemsADIN1300;
                }
                else
                {
                    return this.testmodeitemsADIN1200;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a device is connected and powered up
        /// </summary>
        public bool DeviceConnectedAndPoweredUp
        {
            get
            {
                return !this.deviceSettings.InSoftwarePowerDown && this.DeviceConnected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a device is connected
        /// </summary>
        public bool DeviceConnected
        {
            get
            {
                return this.IsDeviceConnected();
            }
        }

        /// <summary>
        /// Gets the text for the power down button
        /// </summary>
        public string SoftwarePowerDownButtonText
        {
            get
            {
                if (this.deviceSettings.InSoftwarePowerDown)
                {
                    return "Software Power Up";
                }
                else
                {
                    return "Software Power Down";
                }
            }
        }

        /// <summary>
        /// Gets the text for the power down button
        /// </summary>
        public string EnableLinkingButtonText
        {
            get
            {
                if (this.deviceSettings.Link.LinkingEnabled)
                {
                    return "Disable Linking";
                }
                else
                {
                    return "Enable Linking";
                }
            }
        }

        private CalibrateCable calibrateCableValue = new CalibrateCable();
        /// <summary>
        /// Gets or sets the calibrate NVP value.
        /// </summary>
        public CalibrateCable CalibrateCableValue
        {
            get
            {
                return calibrateCableValue;
            }

            set
            {
                calibrateCableValue = value;
                if (this.SelectedDevice != null)
                {
                    this.SelectedDevice.Cable = value;
                }

                this.RaisePropertyChanged(nameof(CalibrateCableValue));
            }
        }

        /// <summary>
        /// Fault state.
        /// </summary>
        private string faultState;
        /// <summary>
        /// Gets or sets the fault state.
        /// </summary>
        public string FaultState
        {
            get
            {
                return this.faultState;
            }

            set
            {
                this.faultState = value;
                this.RaisePropertyChanged(nameof(FaultState));

                if (value.ToLower().Contains("open") || value.ToLower().Contains("close"))
                {
                    // Open/Close.
                    this.FaultTypeColor = "#850000";
                    this.DistToFaultVisibility = Visibility.Visible;
                }
                else if (value.ToLower().Equals("-"))
                {
                    // No data yet.
                    this.FaultTypeColor = "#e0e0e0";
                    this.DistToFaultVisibility = Visibility.Hidden;
                }
                else
                {
                    // No fault.
                    this.FaultTypeColor = "#228B22";
                    this.DistToFaultVisibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// Fault type indicator color.
        /// </summary>
        private string faultTypeColor;
        /// <summary>
        /// Gets or sets the color of the fault type indicator.
        /// </summary>
        public string FaultTypeColor
        {
            get
            {
                return this.faultTypeColor;
            }

            set
            {
                this.faultTypeColor = value;
                this.RaisePropertyChanged(nameof(FaultTypeColor));
            }
        }

        /// <summary>
        /// Distance to fault indicator cisibility.
        /// </summary>
        private Visibility distToFaultVisibility;
        /// <summary>
        /// Gets or sets the distance to fault visibility.
        /// </summary>
        public Visibility DistToFaultVisibility
        {
            get
            {
                return this.distToFaultVisibility;
            }

            set
            {
                this.distToFaultVisibility = value;
                this.RaisePropertyChanged(nameof(DistToFaultVisibility));
            }
        }

        /// <summary>
        /// Distance to fault.
        /// </summary>
        private float distToFault;
        /// <summary>
        /// Gets or sets the distance to fault.
        /// </summary>
        public float DistToFault
        {
            get
            {
                return this.distToFault;
            }

            set
            {
                this.distToFault = value;
                this.RaisePropertyChanged(nameof(DistToFault));
            }
        }

        private CalibrateOffset calibrateOffsetValue = new CalibrateOffset();
        /// <summary>
        /// Gets or sets the calibrate offset value.
        /// </summary>
        public CalibrateOffset CalibrateOffsetValue
        {
            get
            {
                return calibrateOffsetValue;
            }

            set
            {
                calibrateOffsetValue = value;
                if (this.SelectedDevice != null)
                {
                    this.SelectedDevice.Offset = value;
                }

                this.RaisePropertyChanged(nameof(CalibrateOffsetValue));
            }
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void Feedback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;
            DeviceModel deviceModel = (DeviceModel)sender;
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    {
                        /* This is just some text...pass it up along the hierarchy */
                        this.FeedbackOfActions = feedback.FeedbackOfActions;
                        break;
                    }

                case "DeviceSettings":
                    {
                        /* Target settings have changed */
                        FirmwareAPI fwapi = deviceModel.FwAPI;
                        this.deviceSettings.MergeStatusSettings(fwapi.DeviceSettings);
                        break;
                    }
            }
        }

        /// <summary>
        /// This method is called when the feedback is set
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void DeviceSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TargetSettings deviceSettings = (TargetSettings)sender;
            string message;

            switch (e.PropertyName)
            {
                case "EthSpeedSelection":
                    {
                        this.RaisePropertyChanged("NegotiateSpeeds");
                        this.RaisePropertyChanged("Negotiate1GSpeeds");
                        this.RaisePropertyChanged("ShowDownspeed10");
                        this.RaisePropertyChanged("ShowDownspeed100");
                        this.RaisePropertyChanged("ShowDownspeedRetires");
                        this.RaisePropertyChanged("ShowAdvertiseEEE100");
                        this.RaisePropertyChanged("ShowAdvertiseEEE1000");
                        this.RaisePropertyChanged("MasterSlavePreferenceShown");
                        this.RaisePropertyChanged("MasterSlaveFixedShown");
                        this.RaisePropertyChanged("LinkSpeed");
                        this.RaisePropertyChanged("Mode");
                    }

                    break;
                case "PhyState":
                    this.RaisePropertyChanged("LinkButtonColor");
                    this.RaisePropertyChanged("ForegroundColor");
                    break;
                case "LocalAdvSpeeds":
                    {
                        message = "Locally Advertised Speeds :";
                        foreach (var speed in this.deviceSettings.Negotiate.LocalAdvSpeeds)
                        {
                            message += " " + speed.ToString();
                        }

                        if (this.selectedDevice != null)
                        {
                            message = this.selectedDevice.SerialNumber + " " + message;
                        }

                        this.Info(message);
                    }

                    break;
                case "FrameGenRunning":
                    this.RaisePropertyChanged("FrameGenRunning");
                    this.RaisePropertyChanged("FrameGeneratorButtonText");
                    break;
                case "ConnectedDeviceType":
                    this.RaisePropertyChanged("Registers");
                    this.RaisePropertyChanged("Scripts");
                    this.RaisePropertyChanged("MasterSlaveFixedShown");
                    this.RaisePropertyChanged("ShowAdvertiseEEE1000");
                    this.RaisePropertyChanged("Negotiate1GSpeeds");
                    this.RaisePropertyChanged("OneGBSpeedsPossible");
                    this.RaisePropertyChanged("TenSPEDevice");
                    this.RaisePropertyChanged("NegotiationSpeedsShown");
                    this.RaisePropertyChanged("ForcedSpeedsShown");
                    this.RaisePropertyChanged("Clk25RefPinPresent");
                    this.RaisePropertyChanged("ClockPinControlPresent");
                    this.RaisePropertyChanged("LoopbackPresent");
                    this.RaisePropertyChanged("TestModesPresent");
                    this.RaisePropertyChanged("FrameCheckerPresent");
                    this.RaisePropertyChanged("CableDiagnosticsPresent");
                    this.RaisePropertyChanged("MasterSlavePreferenceShown");
                    this.RaisePropertyChanged("MasterSlaveFixedShown");
                    this.SelectedTestModeItem = this.TestModeItems[0];
                    this.RaisePropertyChanged("TestModeItems");
                    break;
                case "RemoteAdvSpeeds":
                    {
                        message = "Remote Advertised Speeds :";
                        foreach (var speed in this.deviceSettings.Negotiate.RemoteAdvSpeeds)
                        {
                            message += " " + speed.ToString();
                        }

                        if (this.selectedDevice != null)
                        {
                            message = this.selectedDevice.SerialNumber + " " + message;
                        }

                        this.Info(message);
                    }

                    break;
                case "ResolvedHCD":
                    {
                        message = "Negotiated Link Speed : " + this.deviceSettings.Link.ResolvedHCD.ToString();

                        if (this.selectedDevice != null)
                        {
                            message = this.selectedDevice.SerialNumber + " " + message;
                        }

                        this.Info(message);
                    }

                    break;
                case "FreqOffsetPpm":
                    /* This seems to fluctuate quite a bit */
                    break;
                case "LocalRcvrOk":
                    message = e.PropertyName + " : " + this.deviceSettings.Link.LocalRcvrOk;

                    if (this.selectedDevice != null)
                    {
                        message = this.selectedDevice.SerialNumber + " " + message;
                    }

                    this.Info(message);
                    break;
                case "RemoteRcvrOk":
                    message = e.PropertyName + " : " + this.deviceSettings.Link.RemoteRcvrOk;

                    if (this.selectedDevice != null)
                    {
                        message = this.selectedDevice.SerialNumber + " " + message;
                    }

                    this.Info(message);

                    break;
                case "AutoNegCompleted":
                    message = e.PropertyName + " : " + this.deviceSettings.Negotiate.AutoNegCompleted;

                    if (this.selectedDevice != null)
                    {
                        message = this.selectedDevice.SerialNumber + " " + message;
                    }

                    this.Info(message);
                    break;
                case "LinkingEnabled":
                    message = e.PropertyName + " : " + this.deviceSettings.Link.LinkingEnabled;

                    if (this.selectedDevice != null)
                    {
                        message = this.selectedDevice.SerialNumber + " " + message;
                    }

                    this.Info(message);
                    break;
                case "Advertise10HD":
                case "Advertise10FD":
                case "Advertise100HD":
                case "Advertise100FD":
                case "Advertise1000HD":
                case "Advertise1000FD":
                case "AdvertiseEEE100":
                case "AdvertiseEEE1000":
                    {
                        /* Downspeed options visibility may be different now */
                        this.RaisePropertyChanged("ShowDownspeed10");
                        this.RaisePropertyChanged("ShowDownspeed100");
                        this.RaisePropertyChanged("ShowAdvertiseEEE100");
                        this.RaisePropertyChanged("ShowAdvertiseEEE1000");
                        this.RaisePropertyChanged("ShowDownspeedRetires");
                        this.RaisePropertyChanged("MasterSlavePreferenceShown");
                        this.RaisePropertyChanged("MasterSlaveFixedShown");
                    }

                    break;
                case "AutoNegLocalPhyEnabled":
                case "LinkEstablished":
                case "InSoftwarePowerDown":
                    {
                        this.RaisePropertyChanged("LinkSpeed");
                        this.RaisePropertyChanged("LinkEstablished");
                        this.RaisePropertyChanged("SoftwarePowerDownButtonText");
                        this.RaisePropertyChanged("EnableLinkingButtonText");
                        lock (syncThreadVarLock)
                        {
                            /* Could set set in worker or UI, and cleared in worker */
                            this.workerinvalidateRequerySuggested = true;
                        }
                    }

                    break;
                default:
                    Trace.WriteLine(e.PropertyName);
                    break;
            }
        }

        /// <summary>
        /// Background worker function progress
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">e</param>
        protected void WorkerPhyStatusProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // No Particular Operation
        }

        /// <summary>
        /// Background worker function completion
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">e</param>
        protected void WorkerPhyStatusRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                System.Console.WriteLine("Phy Status Completed");
            }
            else if (e.Cancelled)
            {
                System.Console.WriteLine("User Cancelled");
            }
            else
            {
                System.Console.WriteLine("An error has occured");
            }
        }

        /// <summary>
        /// Background worker function
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">e</param>
        protected void WorkerRefreshRegistersDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker sendingWorker = (BackgroundWorker)sender;
            Stopwatch stopWatch = new Stopwatch();

            while (!sendingWorker.CancellationPending)
            {
                if (this.Registers == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // Begin timing.
                stopWatch.Start();

                foreach (var registerDetail in this.Registers)
                {
                    lock (this)
                    {
                        if (this.selectedDevice != null)
                        {
                            uint regContent;
                            RegisterInfo fieldInfo;

                            try
                            {
                                regContent = this.selectedDevice.FwAPI.ReadYodaRg(registerDetail.MMap, registerDetail.Name);
                                foreach (FieldDetails fieldDetail in registerDetail.Fields)
                                {
                                    fieldInfo = new RegisterInfo(registerDetail, fieldDetail);
                                    fieldDetail.Value = fieldInfo.ExtractFieldValue(regContent);
                                }

                                registerDetail.Value = regContent;
                            }
                            catch (FTDIException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (ApplicationException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (ArgumentException exc)
                            {
                                if (exc.Message.Contains("Information on register or field"))
                                {
                                    // We must be in the process of switching the JSON files and the
                                    // register is no longer defined.
                                    break;
                                }
                                else
                                {
                                    this.Error(exc.Message);
                                }
                            }
                            catch (Exception exc)
                            {
                                this.Error(exc.Message);
                            }
                        }
                    }
                }

                // Stop timing.
                stopWatch.Stop();

                // Write result.
                Console.WriteLine("Refreshing Registers Time elapsed: {0}", stopWatch.Elapsed);
                stopWatch.Reset();
            }

            e.Cancel = true;
        }

        /// <summary>
        /// Background worker function
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">e</param>
        protected void WorkerPhyStatusDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker sendingWorker = (BackgroundWorker)sender;
            Stopwatch stopWatch = new Stopwatch();
            long searchDevicesTimeout = 10000;

            while (!sendingWorker.CancellationPending)
            {
                lock (this)
                {
                    if (this.selectedDevice != null)
                    {
                        try
                        {
                            this.selectedDevice.FwAPI.ReportPhyStatus();
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }

                if (this.workerinvalidateRequerySuggested)
                {
                    /* Could set set in worker or UI, and cleared in worker */
                    this.workerinvalidateRequerySuggested = false;

                    // Forcing the CommandManager to raise the RequerySuggested event
                    // on the UI thread. This will be running in
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    }));
                }

                lock (this)
                {
                    if (this.selectedDevice != null)
                    {
                        if (this.deviceSettings.PropertiesChangedList.Count > 0)
                        {
                            try
                            {
                                this.selectedDevice.FwAPI.DeviceSettings = this.deviceSettings;
                            }
                            catch (FTDIException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (ApplicationException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (Exception exc)
                            {
                                this.Error(exc.Message);
                            }

                            this.deviceSettings.ClearPropertiesChangedList();
                        }
                    }
                }

                if (this.SelectedDevice == null)
                {
                    /* Decrease the timeout to 2 seconds if we are not already connected to one.*/
                    searchDevicesTimeout = 2000;
                }
                else
                {
                    searchDevicesTimeout = 10000;
                }

                if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > searchDevicesTimeout)
                {
                    lock (this)
                    {
                        /* Check to see if Have any of our USB devices been connected or removed ?*/
                        try
                        {
                            /* Any call to an FTDI function could throw an exception at any time !*/
                            DeviceConnection.RefreshConnectedDevices();
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }

                        if (!stopWatch.IsRunning)
                        {
                            stopWatch.Start();
                        }
                        else
                        {
                            stopWatch.Restart();
                        }
                    }

                    lock (syncThreadVarLock)
                    {
                        if (this.newDevicesAdded)
                        {
                            this.newDevicesAdded = false;

                            Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                bool newDevice;

                                // Add new devices on the dispatcher thread
                                /* Check for devices that have been added */
                                foreach (var item in DeviceConnection.DeviceSerialNumbers)
                                {
                                    newDevice = true;

                                    foreach (var device in this.devices)
                                    {
                                        if (device.SerialNumber == item.SerialNumber.ToString())
                                        {
                                            newDevice = false;
                                            break;
                                        }
                                    }

                                    if (newDevice)
                                    {
                                        this.devices.Add(new DeviceModel(item.SerialNumber.ToString(), item.Description, this.Feedback_PropertyChanged)
                                        {
                                            FrameGenerator = new FrameGeneratorChecker() { FramesBurst = 64001, FrameLength = 1250, FrameContent = 0, SourceMacAddress = ":::::", DestinationMacAddress = ":::::" },
                                            TestModeItem = this.selectedTestModeItem,
                                            Loopback = new Loopback() { LoopbackItem = this.LoopbackItems[0] }
                                        });
                                    }
                                }

                                if (DeviceConnection.DeviceSerialNumbers.Count > 0 && this.SelectedDevice == null)
                                {
                                    // Should only auto select if it is present...and default to the last one added.
                                    int index = this.devices.Count - 1;

                                    while (index >= 0)
                                    {
                                        if (this.devices[index].IsPresent)
                                        {
                                            this.SelectedDevice = this.devices[index];

                                            // No device selected but we have some devices to talk to
                                            this.Info("Auto-selecting device : " + this.SelectedDevice.SerialNumber);
                                            break;
                                        }
                                        index--;
                                    }
                                }
                            }));
                        }
                    }
                }
            }

            e.Cancel = true;
        }

        /// <summary>
        /// Run a script
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoRunScript(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        //this.selectedDevice.FwAPI.RunScript((string)obj);
                        this.selectedDevice.FwAPI.RunScript(this.SelectedScript1);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if running a script is possible
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoRunScript(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Execute a test mode
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoExecuteTest(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        if (this.TenSPEDevice)
                        {
                            //DB Fix this.Error("TODO_10SPE : What registers are needed to do this test : " + this.selectedTestModeItem.TestName);
                            switch (this.selectedTestModeItem.TestModeName)
                            {

                                case "10BASE-T1L Normal mode":
                                    this.selectedDevice.FwAPI.SetupT1L_NormalMode();
                                    break;
                                case "10BASE-T1L Test mode 1: Tx output voltage, Tx clock frequency and jitter.":
                                    this.selectedDevice.FwAPI.SetupT1L_TestMode1();
                                    break;
                                case "10BASE-T1L Test mode 2: Tx output droop":
                                    this.selectedDevice.FwAPI.SetupT1L_TestMode2();
                                    break;
                                case "10BASE-T1L Test mode 3: Power Spectral Density (PSD) and power level":
                                    this.selectedDevice.FwAPI.SetupT1L_TestMode3();
                                    break;
                                case "10BASE-T1L Transmit Disable: MDI Return Loss":
                                    this.selectedDevice.FwAPI.SetupT1L_TestSilentMode();
                                    break;
                            }
                        }
                        else
                        {
                            switch (this.selectedTestModeItem.TestModeName)
                            {
                                case "100BASE-TX VOD":
                                    this.selectedDevice.FwAPI.SetupB100VOD();
                                    break;
                                case "1000BASE-T Test mode 1":
                                    this.selectedDevice.FwAPI.SetupB1000TestMode1();
                                    break;
                                case "1000BASE-T Test mode 2":
                                    this.selectedDevice.FwAPI.SetupB1000TestMode2();
                                    break;
                                case "1000BASE-T Test mode 3":
                                    this.selectedDevice.FwAPI.SetupB1000TestMode3();
                                    break;
                                case "1000BASE-T Test mode 4":
                                    this.selectedDevice.FwAPI.SetupB1000TestMode4();
                                    break;
                                case "10BASE-T Link Pulse":
                                    this.selectedDevice.FwAPI.SetupB10LbTxEn();
                                    break;
                                case "10BASE-T TX Random Frames":
                                    this.selectedDevice.FwAPI.SetupB10LbTxEnFgRnd(this.testModeFrameLength);
                                    break;
                                case "10BASE-T TX 0xFF Frames":
                                    this.selectedDevice.FwAPI.SetupB10LbTxEnFgAll1s(this.testModeFrameLength);
                                    break;
                                case "10BASE-T TX 0x00 Frames":
                                    this.selectedDevice.FwAPI.SetupB10LbTxEnFgAll0s(this.testModeFrameLength);
                                    break;
                                case "10BASE-T TX 5 MHz DIM 0":
                                    this.selectedDevice.FwAPI.SetupB10TxTst5MHz(0);
                                    break;
                                case "10BASE-T TX 5 MHz DIM 1":
                                    this.selectedDevice.FwAPI.SetupB10TxTst5MHz(1);
                                    break;

                                case "10BASE-T TX 10 MHz DIM 0":
                                    this.selectedDevice.FwAPI.SetupB10TxTst10MHz(0);
                                    break;
                                case "10BASE-T TX 10 MHz DIM 1":
                                    this.selectedDevice.FwAPI.SetupB10TxTst10MHz(1);
                                    break;
                            }
                        }
                    }

                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if re-negotiate the PHY can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoExecuteTest(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Executes Fault Detection
        /// </summary>
        /// <param name="obj"></param>
        private void DoFaultDetection(object obj)
        {
            string faultType = string.Empty;

            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        this.DistToFault = this.selectedDevice.FwAPI.ExecuteFaultDetection(this.calibrateOffsetValue, this.calibrateCableValue, CalibrationMode.AutoRange, out faultType);
                        this.FaultState = faultType;
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Executes faule detection calibration.
        /// </summary>
        /// <param name="obj"></param>
        private void DoFaultDetectionCalibrate(object obj)
        {
            string message;

            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        var type = (Calibrate)Enum.Parse(typeof(Calibrate), obj.ToString());
                        switch (type)
                        {
                            case Calibrate.NVP:
                                message = "Please connect cable at MDI connector and enter the cable \nlength to perform cable calibration.";

                                Views.CalibrateCableDialog cableDialog = new Views.CalibrateCableDialog();
                                cableDialog.txtCableLength.Text = "100.0";
                                cableDialog.Owner = Application.Current.MainWindow;
                                cableDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                cableDialog.ContentMessage = message;

                                if (cableDialog.ShowDialog() == true)
                                {
                                    float cableLengthInput = 0.0f;
                                    float.TryParse(cableDialog.txtCableLength.Text, out cableLengthInput);

                                    calibrateCableValue.NVP = float.Parse(cableDialog.txtCableLength.Text);
                                    float[] value = this.selectedDevice.FwAPI.FaultDetectionCalibration(Calibrate.NVP, cableLength: cableLengthInput, calibrationMode: CalibrationMode.AutoRange);
                                    this.RaisePropertyChanged(nameof(this.CalibrateCableValue));

                                    this.CalibrateCableValue = new CalibrateCable()
                                    {
                                        NVP = value[0],
                                        Coeff0 = value[1],
                                        Coeffi = value[2],
                                    };
                                }
                                else
                                {
                                    this.VerboseInfo("Calibration NVP cancelled.");
                                }
                                break;
                            case Calibrate.Offset:
                                message = "Please disconnect cable from MDI connector and \nclick OK to perform offset calibration.";

                                Views.CalibrateOffsetDialog offsetDialog = new Views.CalibrateOffsetDialog();
                                offsetDialog.Owner = Application.Current.MainWindow;
                                offsetDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                offsetDialog.ContentMessage = message;

                                if (offsetDialog.ShowDialog() == true)
                                {
                                    float[] value = this.selectedDevice.FwAPI.FaultDetectionCalibration(Calibrate.Offset, nvp: 0.0f);
                                    this.RaisePropertyChanged(nameof(this.CalibrateOffsetValue));

                                    this.CalibrateOffsetValue = new CalibrateOffset()
                                    {
                                        Offset = value[0]
                                    };
                                }
                                else
                                {
                                    this.VerboseInfo("Calibration offset cancelled.");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Executes saving of calibration data.
        /// </summary>
        /// <param name="obj"></param>
        private void DoCalibrateSave(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        var type = (Calibrate)Enum.Parse(typeof(Calibrate), obj.ToString());
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        StringBuilder content = new StringBuilder();

                        switch (type)
                        {
                            case Calibrate.NVP:
                                saveFileDialog.Filter = "Calibrate Cable file (*.ccf)|*.ccf";
                                var result = this.selectedDevice.FwAPI.GetCoeff();

                                if (saveFileDialog.ShowDialog() == true)
                                {
                                    //this.viewModel.Error(new NotImplementedException().Message);
                                    content.Append($"{result[0].ToString("f6")},");
                                    content.Append($"{result[1].ToString("f6")},");
                                    content.Append($"{result[2].ToString("f6")},");
                                    this.WriteContent(saveFileDialog.FileName, content);
                                }

                                break;
                            case Calibrate.Offset:
                                saveFileDialog.Filter = "Calibrate Offset file (*.cof)|*.cof";
                                var offsetValue = this.selectedDevice.FwAPI.GetOffset();

                                if (saveFileDialog.ShowDialog() == true)
                                {
                                    //this.viewModel.Error(new NotImplementedException().Message);
                                    content.Append($"{offsetValue},");
                                    this.WriteContent(saveFileDialog.FileName, content);
                                }

                                break;
                            default:
                                this.Error(new NotSupportedException().Message);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Save content data to file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        private void WriteContent(string fileName, StringBuilder content)
        {
            try
            {
                using (StreamWriter wr = new StreamWriter(fileName))
                {
                    wr.Write(content);
                }

                this.Info($"{Path.GetFileName(fileName)} is successfully saved.");
            }
            catch (Exception ex)
            {
                this.Error(ex.Message);
            }
        }

        /// <summary>
        /// Executes loading of calibration data.
        /// </summary>
        /// <param name="obj"></param>
        private void DoCalibrateLoad(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        var type = (Calibrate)Enum.Parse(typeof(Calibrate), obj.ToString());
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        string[] values = null;

                        switch (type)
                        {
                            case Calibrate.NVP:
                                openFileDialog.Filter = "Calibrate Cable file (*.ccf)|*.ccf";
                                if (openFileDialog.ShowDialog() == true)
                                {
                                    //this.viewModel.Error(new NotImplementedException().Message);
                                    values = this.ReadContent(openFileDialog.FileName);
                                    this.CalibrateCableValue = new CalibrateCable()
                                    {
                                        NVP = float.Parse(values[0]),
                                        Coeff0 = float.Parse(values[1]),
                                        Coeffi = float.Parse(values[2]),
                                        FileName = Path.GetFileName(openFileDialog.FileName),
                                    };

                                    this.selectedDevice.FwAPI.SetNvp(this.CalibrateCableValue.NVP);
                                }

                                break;
                            case Calibrate.Offset:
                                openFileDialog.Filter = "Calibrate Offset file (*.cof)|*.cof";
                                if (openFileDialog.ShowDialog() == true)
                                {
                                    //this.viewModel.Error(new NotImplementedException().Message);
                                    values = this.ReadContent(openFileDialog.FileName);
                                    this.CalibrateOffsetValue = new CalibrateOffset()
                                    {
                                        Offset = float.Parse(values[0]),
                                        FileName = Path.GetFileName(openFileDialog.FileName),
                                    };

                                    this.selectedDevice.FwAPI.SetOffset(this.CalibrateOffsetValue.Offset);
                                }

                                break;
                            default:
                                this.Error(new NotSupportedException().Message);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the content of a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string[] ReadContent(string fileName)
        {
            string[] values = null;

            using (StreamReader sr = new StreamReader(fileName))
            {
                string content = sr.ReadToEnd();
                values = content.Split(',');
            }

            return values;
        }

        /// <summary>
        /// Executes the Reset Fault Detection
        /// </summary>
        /// <param name="obj"></param>
        private void DoResetFaultDetection(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        float nvpResult = 0.0f;
                        int cableOffsetResult = 0;
                        CalibrationMode modeResult = 0;

                        this.selectedDevice.FwAPI.ResetFaultDetection(out nvpResult, out cableOffsetResult, out modeResult);
                        this.CalibrateOffsetValue.Offset = cableOffsetResult;
                        this.CalibrateCableValue.NVP = nvpResult;

                        this.RaisePropertyChanged(nameof(CalibrateOffsetValue));
                        this.RaisePropertyChanged(nameof(CalibrateCableValue));
                    }
                    catch (Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Execute a FrameChecker
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoExecuteFrameChecker(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    if (obj is FrameCheckerParameters)
                    {
                        FrameCheckerParameters frameCheckerParameters = (FrameCheckerParameters)obj;
                        try
                        {
                            if (frameCheckerParameters.EnableChecker)
                            {
                                this.selectedDevice.FwAPI.FrameCheckerConfig(FrameChecker.RxSide, frameCheckerParameters.FrameLength);
                            }

                            this.selectedDevice.FwAPI.SendData(frameCheckerParameters);
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns if FrameChecker can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoExecuteFrameChecker(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Execute a FrameChecker
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoResetFrameChecker(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        this.selectedDevice.FwAPI.ResetFrameCheckerStatistics();
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if FrameChecker can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoResetFrameChecker(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// re-negotiate the PHY
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoRestartAutoNeg(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        /* Command the connected PHY to restart auto negotiation */
                        this.selectedDevice.FwAPI.RestartANeg();
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if re-negotiate the PHY can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoRestartAutoNeg(object arg)
        {
            return this.DeviceConnectedAndPoweredUp && this.deviceSettings.Link.LinkingEnabled;
        }

        /// <summary>
        /// Run cable diagnostics
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoRunCableDiagnostics(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        bool bEnable = (bool)obj;
                        this.selectedDevice.FwAPI.RunCableDiagnostics(bEnable);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns cable diagnosticss can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoRunCableDiagnostics(object arg)
        {
            return this.DeviceConnectedAndPoweredUp && this.deviceSettings.PhyState == EthPhyState.Standby;
        }

        /// <summary>
        /// Software Powerdown or PowerUp the PHY
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoSoftwarePowerDown(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        /* Command the connected PHY to power up or down */
                        this.selectedDevice.FwAPI.SoftwarePdEnable(!this.deviceSettings.InSoftwarePowerDown);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if Software Powerdown or PowerUp the PHY can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoSoftwarePowerDown(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Software Reset the PHY
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoSoftwareReset(object obj)
        {
            lock (this)
            {
                string resettype = (string)obj;

                if (this.selectedDevice != null)
                {
                    try
                    {
                        this.selectedDevice.FwAPI.SoftwareReset(resettype);
                        if (this.selectedDevice.FwAPI.DeviceSettings.ConnectedDeviceType == DeviceType.ADIN1100)
                        {
                            this.SelectedTestModeItem = this.testmodeitemsADIN1100[0];
                            this.SelectedLoopbackItem = this.loopbackItemsADIN1100[0];
                        }
                        else if (this.selectedDevice.FwAPI.DeviceSettings.ConnectedDeviceType == DeviceType.ADIN1200)
                        {
                            this.SelectedTestModeItem = this.testmodeitemsADIN1200[0];
                        }
                        else
                        {
                            this.SelectedTestModeItem = this.testmodeitemsADIN1300[0];
                        }
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Perform a register action
        /// </summary>
        /// <param name="obj">Action to perform</param>
        private void DoRegisterAction(object obj)
        {
            lock (this)
            {
                string action = (string)obj;

                if (this.selectedDevice != null)
                {
                    try
                    {
                        switch (action)
                        {
                            case "Export Registers":
                                this.selectedDevice.FwAPI.DumpRegisterContents();
                                break;
                            case "Load Registers":
                                {
                                    var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
                                    var result = ofd.ShowDialog();
                                    if (result == false)
                                    {
                                        return;
                                    }

                                    this.selectedDevice.FwAPI.LoadRegisterContents(ofd.FileName);
                                }

                                break;
                            case "Save Registers":
                                {
                                    var sfd = new Microsoft.Win32.SaveFileDialog() { Filter = "XML Files (*.xml)|*.xml" };
                                    var result = sfd.ShowDialog();
                                    if (result == false)
                                    {
                                        return;
                                    }

                                    this.selectedDevice.FwAPI.SaveRegisterContents(sfd.FileName);
                                }

                                break;
                        }
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if a register action can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoRegisterAction(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Returns if Software Reset the PHY can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoSoftwareReset(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Writes a register value to an address
        /// </summary>
        /// <param name="obj">RegisterParameters object</param>
        private void WriteRegister(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    if (obj is RegisterParameters)
                    {
                        RegisterParameters registerParameters = this.CheckRegisterParameters((RegisterParameters)obj);

                        try
                        {
                            string message = null;
                            if (!registerParameters.RegisterAddressOk || !registerParameters.RegisterValueOk)
                            {
                                if (!registerParameters.RegisterAddressOk)
                                {
                                    this.Error(string.Format("Register address \"{0}\" is not in a valid format", this.WriteRegisterAddress));
                                }

                                if (!registerParameters.RegisterValueOk)
                                {
                                    this.Error(string.Format("Register value \"{0}\" is not in a valid format", this.WriteRegisterValue));
                                }
                            }
                            else
                            {
                                this.SelectedDevice.FwAPI.WriteValueInRegisterAddress(registerParameters.RegisterAddress, registerParameters.RegisterValue);
                                message = $"Write Register Address: 0x{registerParameters.RegisterAddress.ToString("X")}, Write Register Value: 0x{registerParameters.RegisterValue.ToString("X")}";
                                this.VerboseInfo(message);
                            }
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the register parameters based on connected device type.
        /// </summary>
        /// <param name="registerParameters">contains the register details</param>
        /// <returns>returns updated register parameters.</returns>
        private RegisterParameters CheckRegisterParameters(RegisterParameters registerParameters)
        {
            switch (this.deviceSettings.ConnectedDeviceType)
            {
                case DeviceType.ADIN1300:
                case DeviceType.ADIN1301:
                case DeviceType.ADIN1200:
                    if (registerParameters.RegisterAddress >= 32)
                    {
                        registerParameters.RegisterAddress = (30 << 16) | registerParameters.RegisterAddress;
                    }

                    break;
                case DeviceType.ADIN1100:
                    // Do Nothing
                    break;
                default:
                    // Do Nothing
                    break;
            }

            return registerParameters;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        private void ReadRegister(object obj)
        {
            lock (this)
            {
                this.ReadRegisterValue = string.Empty;
                if (this.selectedDevice != null)
                {
                    if (obj is RegisterParameters)
                    {
                        RegisterParameters registerParameters = this.CheckRegisterParameters((RegisterParameters)obj);

                        try
                        {
                            uint regVal = 0;
                            string message = null;
                            if (!registerParameters.RegisterAddressOk)
                            {
                                if (!registerParameters.RegisterAddressOk)
                                {
                                    this.Error(string.Format("Register address \"{0}\" is not in a valid format", this.ReadRegisterAddress));
                                }
                            }
                            else
                            {
                                regVal = this.SelectedDevice.FwAPI.ReadValueInRegisterAddress(registerParameters.RegisterAddress);
                                message = $"Read Register Address: 0x{registerParameters.RegisterAddress.ToString("X")}, Read Register Value: {regVal.ToString("X4")}";
                                this.ReadRegisterValue = regVal.ToString("X4");
                                this.VerboseInfo(message);
                            }
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool CanWriteRegister(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool CanReadRegister(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Output selected clock to the CLK25_REF pin
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoGP_CLKPin(object obj)
        {
            lock (this)
            {
                string clocktype = (string)obj;

                if (clocktype != null)
                {
                    GPClockSel gP_CLK_sel;
                    if (Enum.TryParse(clocktype, out gP_CLK_sel))
                    {
                        if (this.selectedDevice != null)
                        {
                            try
                            {
                                this.selectedDevice.FwAPI.GP_CLKConfig(gP_CLK_sel);
                            }
                            catch (FTDIException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (ApplicationException exc)
                            {
                                this.Error(exc.Message);
                            }
                            catch (Exception exc)
                            {
                                this.Error(exc.Message);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns if clock can be output to GP_CLK pin
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoGP_CLKPin(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Output selected clock to the CLK25_REF pin
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoCLK25_REFPin(object obj)
        {
            lock (this)
            {
                string clocktype = (string)obj;

                if (clocktype != null && this.selectedDevice != null)
                {
                    try
                    {
                        this.selectedDevice.FwAPI.REF_CLKEnable(clocktype == "True");
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if clock can be output to CLK25_REF pin
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoCLK25_REFPin(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Perform remote loopback
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoRemoteLoopback(object obj)
        {
            lock (this)
            {

                if (this.selectedDevice != null)
                {
                    try
                    {
                        if (this.selectedDevice.InLoopback != true)
                        {
                            this.selectedDevice.InLoopback = true;
                        }
                        else
                        {
                            this.selectedDevice.InLoopback = false;
                        }

                        this.selectedDevice.FwAPI.ConfigureForRemoteLoopback(this.selectedDevice.InLoopback);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if we can do a remote loopback...only if we have a link up
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoRemoteLoopback(object arg)
        {
            return this.DeviceConnected;
        }

        /// <summary>
        /// Perform remote loopback
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoLocalLoopback(object obj)
        {
            lock (this)
            {
                //if (obj is LocalLoopbackParameters)
                {
                    LocalLoopbackParameters localLoopbackParameters = new LocalLoopbackParameters(); ;
                    localLoopbackParameters.gePhyLb_selt = (LoopBackMode)Enum.Parse(typeof(LoopBackMode), this.SelectedLoopbackItem.Name);
                    localLoopbackParameters.isolateRx_st = this.RxSuppression;
                    localLoopbackParameters.lbTxSup_st = this.TxSuppression;

                    if (this.selectedDevice != null)
                    {
                        try
                        {
                            this.selectedDevice.FwAPI.PhyLoopbackConfig(
                                localLoopbackParameters.gePhyLb_selt,
                                localLoopbackParameters.isolateRx_st,
                                localLoopbackParameters.lbTxSup_st);
                        }
                        catch (FTDIException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (ApplicationException exc)
                        {
                            this.Error(exc.Message);
                        }
                        catch (Exception exc)
                        {
                            this.Error(exc.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns if we can do a local loopback...only if we have a link down
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoLocalLoopback(object arg)
        {
            return this.DeviceConnected;
        }

        private void DoTxSuppression(object obj)
        {
            bool isSuppress = (bool)obj;

            lock (this)
            {
                this.selectedDevice.FwAPI.SPEPhyLoopbackTxSuppression(isSuppress);
            }
        }

        private void DoRxSuppression(object obj)
        {
            bool isSuppress = (bool)obj;

            lock (this)
            {
                this.selectedDevice.FwAPI.SPEPhyLoopbackRxSuppression(isSuppress);
            }
        }

        /// <summary>
        /// Returns if Enable or disabling linking on the PHY can be done
        /// </summary>
        /// <param name="obj">No value passed</param>
        private void DoEnableLinking(object obj)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        /* Command the connected PHY to power up or down */
                        this.selectedDevice.FwAPI.EnableLinking(!this.deviceSettings.Link.LinkingEnabled);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if enable or disable linkinkg on the PHY can be done
        /// </summary>
        /// <param name="arg">No value passed</param>
        /// <returns>Boolean to denote it can be made</returns>
        private bool CanDoEnableLinking(object arg)
        {
            return this.DeviceConnectedAndPoweredUp && !this.TenSPEDevice;
        }

        /// <summary>
        /// This will be called in the worker thread during the call to RefreshConnectedDevices
        /// if the number of connected MDIO dongles has changed.
        /// </summary>
        private void DeviceConnection_DeviceSerialNumbersChanged(object sender, EventArgs e)
        {
            /* Have we removed any of the devices that we have seen before? */
            foreach (var device in this.devices)
            {
                device.IsPresent = DeviceConnection.DeviceSerialNumbers.Exists(x => x.SerialNumber.ToString() == device.SerialNumber);
            }

            if (this.SelectedDevice != null)
            {
                // We are connected to a device...has it disappeared?
                if (!this.SelectedDevice.IsPresent)
                {
                    // The device we were connected to has disappeared
                    try
                    {
                        this.selectedDevice.FwAPI.Close();
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }

            if (DeviceConnection.DeviceSerialNumbers.Count == 0)
            {
                // No devices currently present
                this.SelectedDevice = null;
            }
            else
            {
                this.newDevicesAdded = true;

                // Is the existing device still there?
                if (this.SelectedDevice != null && !this.SelectedDevice.IsPresent)
                {
                    this.SelectedDevice = null;
                }
            }

            /* Update the selection as our list has changed */
            if (DeviceConnection.DeviceSerialNumbers.Count == 0)
            {
                this.SelectedDevice = null;
                this.Error("Please plug in at least one Evaluation board!");
            }
            else
            {
                string message;

                message = "The following Evaluation boards are present : ";
                foreach (var item in DeviceConnection.DeviceSerialNumbers)
                {
                    message += string.Format("{0}  ", item.SerialNumber);
                }

                this.Info(message);
            }

            this.RaisePropertyChanged("Devices");
            lock (syncThreadVarLock)
            {
                /* Could set set in worker or UI, and cleared in worker */
                this.workerinvalidateRequerySuggested = true;
            }
        }

        private bool IsDeviceConnected()
        {
            return this.selectedDevice != null;
        }

        private void InitializedWorkerPhyStatus()
        {
            this.workerPhyStatus = new BackgroundWorker();
            this.workerPhyStatus.DoWork += new DoWorkEventHandler(this.WorkerPhyStatusDoWork);
            this.workerPhyStatus.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.WorkerPhyStatusRunWorkerCompleted);

            // this.workerPhyStatus.ProgressChanged += new ProgressChangedEventHandler(this.WorkerPhyStatusProgressChanged);
            this.workerPhyStatus.WorkerReportsProgress = false;
            this.workerPhyStatus.WorkerSupportsCancellation = true;
            this.workerPhyStatus.RunWorkerAsync();
        }

        private void InitializedWorkerRefreshRegisters()
        {
            this.workerRefreshRegisters = new BackgroundWorker();
            this.workerRefreshRegisters.DoWork += new DoWorkEventHandler(this.WorkerRefreshRegistersDoWork);

            this.workerRefreshRegisters.WorkerReportsProgress = false;
            this.workerRefreshRegisters.WorkerSupportsCancellation = true;
            this.workerRefreshRegisters.RunWorkerAsync();
        }

        /// <summary>
        /// Write a value
        /// </summary>
        /// <param name="mMap">Memory map of element to write to</param>
        /// <param name="name">Name of element to write to</param>
        /// <param name="value">Value of element</param>
        public void WriteRegister(string mMap, string name, uint value)
        {
            lock (this)
            {
                if (this.selectedDevice != null)
                {
                    try
                    {
                        this.SelectedDevice.FwAPI.WriteValue(mMap, name, value);
                    }
                    catch (FTDIException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (ApplicationException exc)
                    {
                        this.Error(exc.Message);
                    }
                    catch (Exception exc)
                    {
                        this.Error(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// retrieves the user scripts
        /// </summary>
        public void UpdateFromScriptJSON()
        {
            this.Scripts = new ObservableCollection<ScriptJSONStructure>();
            string[] dirs = Directory.GetFiles(@".\scripts");
            foreach (string requiredjsonfile in dirs)
            {
                this.Info(string.Format("Loading scripts from {0}", Path.GetFileName(requiredjsonfile)));
                try
                {
                    this.Scripts.Add(this.jsonParser.ParseScriptData(requiredjsonfile));
                }
                catch (Exception ex)
                {
                    this.Error($"Error Script [{Path.GetFileName(requiredjsonfile)}]\n {ex.Message}");
                }
            }
        }
    }
}
