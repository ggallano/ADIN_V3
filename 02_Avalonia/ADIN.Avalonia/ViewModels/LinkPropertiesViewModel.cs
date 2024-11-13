// <copyright file="LinkPropertiesViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Services;
using ADIN.Avalonia.Stores;
using ADIN.Device.Models;
using ADIN.Device.Services;
using Avalonia.Threading;
using FTDIChip.Driver.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace ADIN.Avalonia.ViewModels
{
    public class LinkPropertiesViewModel : ViewModelBase
    {
        private SelectedDeviceStore _selectedDeviceStore;
        private IFTDIServices _ftdiServices;
        private readonly ApplicationConfigService _applicationConfigService;
        private BackgroundWorker _backgroundWorker;
        private bool _isLoading = false;
        private List<uint> _downSpeedRetries = new List<uint>(){ 0, 1, 2, 3, 4, 5, 6, 7 };

        public LinkPropertiesViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiServices, ApplicationConfigService appConfigService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiServices = ftdiServices;
            _applicationConfigService = appConfigService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FtdiComOpened += LoadChanges;
            _selectedDeviceStore.PhyModeChanged += LoadChanges;
        }

        public bool AllowInput => bool.Parse(_applicationConfigService.GetConfigValue("AllowGuiControl"));

        public List<string> AdvertisedSpeeds => _linkProperties.AdvertisedSpeeds;

        public List<string> EnergyDetectPowerDownModes => _linkProperties?.EnergyDetectPowerDownModes;

        public List<string> ForcedSpeeds => _linkProperties?.ForcedSpeeds;

        public bool IsAdvertise_1000BASE_T_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_1000BASE_T_FD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_1000BASE_T_FD)
                {
                    _linkProperties.IsAdvertise_1000BASE_T_FD = value;
                    IAdvertisedGbSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedGbSpeedAPI;
                    fwAPI.Speed1000FdAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
                    OnPropertyChanged(nameof(IsEEE1000Enabled));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                    OnPropertyChanged(nameof(IsLeaderFollowerVisible));
                }
            }
        }

        public bool IsAdvertise_1000BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_1000BASE_T_HD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_1000BASE_T_HD)
                {
                    _linkProperties.IsAdvertise_1000BASE_T_HD = value;
                    IAdvertisedGbSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedGbSpeedAPI;
                    fwAPI.Speed1000HdAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                    OnPropertyChanged(nameof(IsLeaderFollowerVisible));
                }
            }
        }

        public bool IsAdvertise_100BASE_TX_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_100BASE_TX_FD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_100BASE_TX_FD)
                {
                    _linkProperties.IsAdvertise_100BASE_TX_FD = value;
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.Speed100FdAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
                    OnPropertyChanged(nameof(IsEEE100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }

            }
        }

        public bool IsAdvertise_100BASE_TX_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_100BASE_TX_HD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_100BASE_TX_HD)
                {
                    _linkProperties.IsAdvertise_100BASE_TX_HD = value;
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.Speed100HdAdvertisement(value);
                
                    OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }
            }
        }

        public bool IsAdvertise_10BASE_T_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_10BASE_T_FD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_10BASE_T_FD)
                {
                    _linkProperties.IsAdvertise_10BASE_T_FD = value;
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.Speed10FdAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }
            }
        }

        public bool IsAdvertise_10BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_10BASE_T_HD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_10BASE_T_HD)
                {
                    _linkProperties.IsAdvertise_10BASE_T_HD = value;
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.Speed10HdAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }
            }
        }

        public bool IsAdvertise_EEE_1000BASE_T
        {
            get
            {
                return _linkProperties?.IsAdvertise_EEE_1000BASE_T == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_EEE_1000BASE_T)
                {
                    _linkProperties.IsAdvertise_EEE_1000BASE_T = value;
                    IAdvertisedGbSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedGbSpeedAPI;
                    fwAPI.Speed1000EEEAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }
            }
        }

        public bool IsAdvertise_EEE_100BASE_TX
        {
            get
            {
                return _linkProperties?.IsAdvertise_EEE_100BASE_TX == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsAdvertise_EEE_100BASE_TX)
                {
                    _linkProperties.IsAdvertise_EEE_100BASE_TX = value;
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.Speed100EEEAdvertisement(value);

                    OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                }
            }
        }

        public bool IsSpeedCapable1G
        {
            get { return (_linkProperties?.SpeedMode != "Forced") && (_linkProperties?.IsSpeedCapable1G != false); }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;
        public bool IsComOpen => _ftdiServices.IsComOpen == true;

        public bool IsDownSpeed_100BASE_TX_HD
        {
            get
            {
                return _linkProperties?.IsDownSpeed_100BASE_TX_HD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsDownSpeed_100BASE_TX_HD)
                {
                    _linkProperties.IsDownSpeed_100BASE_TX_HD = value;
                    IAdvertisedGbSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedGbSpeedAPI;
                    fwAPI.DownSpeed100Hd(value);

                    OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
                }
            }
        }

        public bool IsDownSpeed_10BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsDownSpeed_10BASE_T_HD == true;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.IsDownSpeed_10BASE_T_HD)
                {
                    _linkProperties.IsDownSpeed_10BASE_T_HD = value;
                    IDownSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IDownSpeedAPI;
                    fwAPI.DownSpeed10Hd(value);

                    OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
                }
            }
        }

        public bool IsDownspeed100Enabled
        {
            get
            {
                if ((IsSpeedCapable1G == true)
                    && (IsAdvertise_100BASE_TX_FD == true
                        || IsAdvertise_100BASE_TX_HD == true
                        || IsAdvertise_EEE_100BASE_TX == true))
                {
                    if (IsAdvertise_1000BASE_T_FD == true
                        || IsAdvertise_1000BASE_T_HD == true
                        || IsAdvertise_EEE_1000BASE_T == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsDownspeed10Enabled
        {
            get
            {
                if (IsAdvertise_10BASE_T_FD == true
                    || IsAdvertise_10BASE_T_HD == true)
                {
                    if (IsAdvertise_1000BASE_T_FD == true
                        || IsAdvertise_1000BASE_T_HD == true
                        || IsAdvertise_EEE_1000BASE_T == true
                        || IsAdvertise_100BASE_TX_FD == true
                        || IsAdvertise_100BASE_TX_HD == true
                        || IsAdvertise_EEE_100BASE_TX == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsEEE1000Enabled
        {
            get
            {
                return (IsAdvertise_1000BASE_T_FD == true)
                    && (IsSpeedCapable1G == true);
            }
        }

        public bool IsEEE100Enabled
        {
            get { return IsAdvertise_100BASE_TX_FD == true; }
        }

        public bool IsLeaderFollowerVisible
        {
            get
            {
                return _linkProperties?.SpeedMode == "Advertised" && (IsAdvertise_1000BASE_T_FD == true || IsAdvertise_1000BASE_T_HD == true);
            }
        }

#if !DISABLE_TSN && !DISABLE_T1L
        public bool IsT1LBoard
        {
            get
            {
                return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)) == true;
            }
        }

        public bool IsGigabitBoard => !IsT1LBoard;

#elif !DISABLE_TSN
        public bool IsGigabitBoard { get; } = true;

        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public bool IsEDPD_Disabled
        {
            get 
            {
                if (_linkProperties?.EnergyDetectPowerDownMode != "Disabled")
                    return false;
                OnPropertyChanged(nameof(IsEDPD_Enabled));
                OnPropertyChanged(nameof(IsEDPD_EnabledWithPeriodicPulseTx));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.EnergyDetectPowerDownMode != "Disabled")
                {
                    _linkProperties.EnergyDetectPowerDownMode = "Disabled";
                    IEnergyDetectAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IEnergyDetectAPI;
                    fwAPI.EnableEnergyDetectPowerDown("Disabled");
                    OnPropertyChanged(nameof(IsEDPD_Disabled));
                }
            }
        }

        public bool IsEDPD_Enabled
        {
            get
            {
                if (_linkProperties?.EnergyDetectPowerDownMode != "Enabled")
                    return false;
                OnPropertyChanged(nameof(IsEDPD_Disabled));
                OnPropertyChanged(nameof(IsEDPD_EnabledWithPeriodicPulseTx));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.EnergyDetectPowerDownMode != "Enabled")
                {
                    _linkProperties.EnergyDetectPowerDownMode = "Enabled";
                    IEnergyDetectAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IEnergyDetectAPI;
                    fwAPI.EnableEnergyDetectPowerDown("Enabled");
                    OnPropertyChanged(nameof(IsEDPD_Disabled));
                    OnPropertyChanged(nameof(IsEDPD_EnabledWithPeriodicPulseTx));
                    OnPropertyChanged(nameof(IsEDPD_Enabled));
                }
            }
        }

        public bool IsEDPD_EnabledWithPeriodicPulseTx
        {
            get
            {
                if (_linkProperties?.EnergyDetectPowerDownMode != "Enabled with Periodic Pulse TX")
                    return false;
                OnPropertyChanged(nameof(IsEDPD_Disabled));
                OnPropertyChanged(nameof(IsEDPD_Enabled));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.EnergyDetectPowerDownMode != "Enabled with Periodic Pulse TX")
                {
                    _linkProperties.EnergyDetectPowerDownMode = "Enabled with Periodic Pulse TX";
                    IEnergyDetectAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IEnergyDetectAPI;
                    fwAPI.EnableEnergyDetectPowerDown("Enabled with Periodic Pulse TX");
                    OnPropertyChanged(nameof(IsEDPD_Disabled));
                    OnPropertyChanged(nameof(IsEDPD_Enabled));
                    OnPropertyChanged(nameof(IsEDPD_EnabledWithPeriodicPulseTx));
                }
            }
        }

        public bool IsForcedSpeed_100BASE_TX_FD
        {
            get 
            {
                if (_linkProperties?.ForcedSpeed != "SPEED_100BASE_TX_FD")
                    return false;
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_HD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_FD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_HD));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.ForcedSpeed != "SPEED_100BASE_TX_FD")
                {
                    _linkProperties.ForcedSpeed = "SPEED_100BASE_TX_FD";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.SetForcedSpeed("SPEED_100BASE_TX_FD");
                    OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_FD));
                }
            }
        }

        public bool IsForcedSpeed_100BASE_TX_HD
        {
            get
            {
                if (_linkProperties?.ForcedSpeed != "SPEED_100BASE_TX_HD")
                    return false;
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_FD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_FD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_HD));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.ForcedSpeed != "SPEED_100BASE_TX_HD")
                {
                    _linkProperties.ForcedSpeed = "SPEED_100BASE_TX_HD";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.SetForcedSpeed("SPEED_100BASE_TX_HD");
                    OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_HD));
                }
            }
        }

        public bool IsForcedSpeed_10BASE_T_FD
        {
            get
            {
                if (_linkProperties?.ForcedSpeed != "SPEED_10BASE_T_FD")
                    return false;
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_FD));
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_HD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_HD));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.ForcedSpeed != "SPEED_10BASE_T_FD")
                {
                    _linkProperties.ForcedSpeed = "SPEED_10BASE_T_FD";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.SetForcedSpeed("SPEED_10BASE_T_FD");
                    OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_FD));
                }
            }
        }

        public bool IsForcedSpeed_10BASE_T_HD
        {
            get
            {
                if (_linkProperties?.ForcedSpeed != "SPEED_10BASE_T_HD")
                    return false;
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_FD));
                OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_HD));
                OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_FD));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.ForcedSpeed != "SPEED_10BASE_T_HD")
                {
                    _linkProperties.ForcedSpeed = "SPEED_10BASE_T_HD";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.SetForcedSpeed("SPEED_10BASE_T_HD");
                    OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_HD));
                }
            }
        }

        public bool IsLeaderFollower_Leader
        {
            get 
            {
                if (_linkProperties?.MasterSlaveAdvertise != "Leader")
                    return false;
                OnPropertyChanged(nameof(IsLeaderFollower_Follower));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.MasterSlaveAdvertise != "Leader")
                {
                    _linkProperties.MasterSlaveAdvertise = "Leader";
                    IMasterSlaveSettingsAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IMasterSlaveSettingsAPI;
                    if (_linkProperties.SpeedMode == "Advertised")
                        fwAPI.SetMasterSlave("Prefer_Master");
                    else
                        fwAPI.SetMasterSlave("Forced_Master");
                    OnPropertyChanged(nameof(IsLeaderFollower_Leader));
                }
            }
        }

        public bool IsLeaderFollower_Follower
        {
            get
            {
                if (_linkProperties?.MasterSlaveAdvertise != "Follower")
                    return false;
                OnPropertyChanged(nameof(IsLeaderFollower_Leader));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.MasterSlaveAdvertise != "Follower")
                {
                    _linkProperties.MasterSlaveAdvertise = "Follower";
                    IMasterSlaveSettingsAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IMasterSlaveSettingsAPI;
                    if (_linkProperties.SpeedMode == "Advertised")
                        fwAPI.SetMasterSlave("Prefer_Slave");
                    else
                        fwAPI.SetMasterSlave("Forced_Slave");
                    OnPropertyChanged(nameof(IsLeaderFollower_Follower));
                }
            }
        }

        public bool IsMDIX_AutoMDIX
        {
            get 
            {
                if (_linkProperties?.MDIX != "Auto MDIX")
                    return false;
                OnPropertyChanged(nameof(IsMDIX_FixedMDI));
                OnPropertyChanged(nameof(IsMDIX_FixedMDIX));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.MDIX != "Auto MDIX")
                {
                    _linkProperties.MDIX = "Auto MDIX";
                    IAutoMDIXAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAutoMDIXAPI;
                    fwAPI.AutoMDIXMode("Auto MDIX");
                    OnPropertyChanged(nameof(IsMDIX_AutoMDIX));
                }
            }
        }

        public bool IsMDIX_FixedMDI
        {
            get
            {
                if (_linkProperties?.MDIX != "Fixed MDI")
                    return false;
                OnPropertyChanged(nameof(IsMDIX_AutoMDIX));
                OnPropertyChanged(nameof(IsMDIX_FixedMDIX));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.MDIX != "Fixed MDI")
                {
                    _linkProperties.MDIX = "Fixed MDI";
                    IAutoMDIXAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAutoMDIXAPI;
                    fwAPI.AutoMDIXMode("Fixed MDI");
                    OnPropertyChanged(nameof(IsMDIX_FixedMDI));
                }
            }
        }

        public bool IsMDIX_FixedMDIX
        {
            get
            {
                if (_linkProperties?.MDIX != "Fixed MDIX")
                    return false;
                OnPropertyChanged(nameof(IsMDIX_AutoMDIX));
                OnPropertyChanged(nameof(IsMDIX_FixedMDI));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties.MDIX != "Fixed MDIX")
                {
                    _linkProperties.MDIX = "Fixed MDIX";
                    IAutoMDIXAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAutoMDIXAPI;
                    fwAPI.AutoMDIXMode("Fixed MDIX");
                    OnPropertyChanged(nameof(IsMDIX_FixedMDIX));
                }
            }
        }

        public bool IsSpeedMode_Advertised
        {
            get
            {
                if (_linkProperties?.SpeedMode == "Forced")
                    return false;
                OnPropertyChanged(nameof(IsSpeedMode_Forced));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.SpeedMode != "Advertised")
                {
                    _linkProperties.SpeedMode = "Advertised";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.AdvertisedForcedSpeed("Advertised");
                    OnPropertyChanged(nameof(IsSpeedMode_Advertised));
                    OnPropertyChanged(nameof(IsSpeedCapable1G));
                    OnPropertyChanged(nameof(IsLeaderFollowerVisible));

                }
            }
        }

        public bool IsSpeedMode_Forced
        {
            get 
            {
                if (_linkProperties?.SpeedMode != "Forced")
                    return false;
                OnPropertyChanged(nameof(IsSpeedMode_Advertised));
                return true;
            }
            set
            {
                if (IsDeviceSelected && IsComOpen && _linkProperties?.SpeedMode != "Forced")
                {
                    _linkProperties.SpeedMode = "Forced";
                    IAdvertisedSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IAdvertisedSpeedAPI;
                    fwAPI.AdvertisedForcedSpeed("Forced");
                    OnPropertyChanged(nameof(IsSpeedMode_Forced));
                    OnPropertyChanged(nameof(IsSpeedCapable1G));
                    OnPropertyChanged(nameof(IsLeaderFollowerVisible));

                }
            }
        }

        public List<uint> DownSpeedRetries
        {
            get { return _downSpeedRetries; }
        }

        public uint SelectedDownSpeedRetries
        {
            get
            {
                return _linkProperties?.DownSpeedRetries ?? 0;
            }

            set
            {
                if (IsDeviceSelected && IsComOpen && value != _linkProperties?.DownSpeedRetries)
                {
                    _linkProperties.DownSpeedRetries = value;
                    IDownSpeedAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IDownSpeedAPI;
                    fwAPI.DownSpeedRetriesSetVal(value);

                    OnPropertyChanged(nameof(SelectedDownSpeedRetries));
                }
            }
        }

        public bool IsMacInt_RGMII
        {
            get { return _phyMode?.MacInterface == "RGMII"; }
            set
            {
                if (IsDeviceSelected && IsComOpen && _phyMode?.MacInterface != "RGMII")
                {
                    _phyMode.MacInterface = "RGMII";
                    OnPropertyChanged(nameof(IsMacInt_RMII));
                    OnPropertyChanged(nameof(IsMacInt_MII));
                    OnPropertyChanged(nameof(IsMacInt_SGMII));
                    OnPropertyChanged(nameof(IsMacInt_RGMII));
                }
            }
        }

        public bool IsMacInt_RMII
        {
            get { return _phyMode?.MacInterface == "RMII"; }
            set
            {
                if (IsDeviceSelected && IsComOpen && _phyMode?.MacInterface != "RMII")
                {
                    _phyMode.MacInterface = "RMII";
                    OnPropertyChanged(nameof(IsMacInt_RGMII));
                    OnPropertyChanged(nameof(IsMacInt_MII));
                    OnPropertyChanged(nameof(IsMacInt_SGMII));
                    OnPropertyChanged(nameof(IsMacInt_RMII));
                }
            }
        }

        public bool IsMacInt_MII
        {
            get { return _phyMode?.MacInterface == "MII"; }
            set
            {
                if (IsDeviceSelected && IsComOpen && _phyMode?.MacInterface != "MII")
                {
                    _phyMode.MacInterface = "MII";
                    OnPropertyChanged(nameof(IsMacInt_RGMII));
                    OnPropertyChanged(nameof(IsMacInt_RMII));
                    OnPropertyChanged(nameof(IsMacInt_SGMII));
                    OnPropertyChanged(nameof(IsMacInt_MII));
                }
            }
        }

        public bool IsMacInt_SGMII
        {
            get { return _phyMode?.MacInterface == "SGMII"; }
            set
            {
                if (IsDeviceSelected && IsComOpen && _phyMode?.MacInterface != "SGMII")
                {
                    _phyMode.MacInterface = "SGMII";
                    OnPropertyChanged(nameof(IsMacInt_RGMII));
                    OnPropertyChanged(nameof(IsMacInt_RMII));
                    OnPropertyChanged(nameof(IsMacInt_MII));
                    OnPropertyChanged(nameof(IsMacInt_SGMII));
                }
            }
        }

        public bool HasActivePhyMode => _phyMode?.ActivePhyMode != null;

        public string ActivePhyMode
        {
            get
            {
                if (_phyMode?.ActivePhyMode == "Auto Media Detect_Cu"
                    || _phyMode?.ActivePhyMode == "Auto Media Detect_Fi")
                    return "Auto Media Detect";
                else if (HasActivePhyMode)
                    return _phyMode?.ActivePhyMode;
                else
                    return string.Empty;
            }
        }

        public bool HasLoadedValues
        {
            get => !_isLoading && IsDeviceSelected;
            set
            {
                _isLoading = !value;
                OnPropertyChanged(nameof(HasLoadedValues));
            }
        }

        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice?.LinkProperties;
        private IPhyMode _phyMode => _selectedDeviceStore.SelectedDevice?.PhyMode;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));
            OnPropertyChanged(nameof(HasLoadedValues));

            if (!IsDeviceSelected)
                return;

            OnPropertyChanged(nameof(AllowInput));
            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));
        }

        private async void LoadChanges()
        {
            await Task.Run(() => _selectedDeviceStore.OnLoadingStatusChanged(this, true, "Loading values..."));
            await Task.Run(() => UpdateValues());
        }

        private void UpdateValues()
        {
            HasLoadedValues = false;

            IValueUpdate fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as IValueUpdate;

            if (IsGigabitBoard)
            {
                _linkProperties.SpeedMode = fwAPI.GetLinkProp_SpeedMode();
                switch (_linkProperties.SpeedMode)
                {
                    case "Advertised":
                        OnPropertyChanged(nameof(IsSpeedMode_Advertised));
                        break;
                    case "Forced":
                        OnPropertyChanged(nameof(IsSpeedMode_Forced));
                        break;
                    default:
                        // Do nothing
                        break;
                }

                OnPropertyChanged(nameof(IsSpeedCapable1G));
                _linkProperties.IsAdvertise_1000BASE_T_FD = fwAPI.GetLinkProp_IsAdv1000BaseTFd();
                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
                _linkProperties.IsAdvertise_1000BASE_T_HD = fwAPI.GetLinkProp_IsAdv1000BaseTHd();
                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
                _linkProperties.IsAdvertise_100BASE_TX_FD = fwAPI.GetLinkProp_IsAdv100BaseTxFd();
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
                _linkProperties.IsAdvertise_100BASE_TX_HD = fwAPI.GetLinkProp_IsAdv100BaseTxHd();
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
                _linkProperties.IsAdvertise_10BASE_T_FD = fwAPI.GetLinkProp_IsAdv10BaseTFd();
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
                _linkProperties.IsAdvertise_10BASE_T_HD = fwAPI.GetLinkProp_IsAdv10BaseTHd();
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));

                _linkProperties.IsAdvertise_EEE_1000BASE_T = fwAPI.GetLinkProp_IsAdvEee1000();
                OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
                _linkProperties.IsAdvertise_EEE_100BASE_TX = fwAPI.GetLinkProp_IsAdvEee100();
                OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));

                _linkProperties.IsDownSpeed_100BASE_TX_HD = fwAPI.GetLinkProp_IsDwnspd100TxHd();
                OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
                _linkProperties.IsDownSpeed_10BASE_T_HD = fwAPI.GetLinkProp_IsDwnspd10THd();
                OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
                _linkProperties.DownSpeedRetries = fwAPI.GetLinkProp_DownspeedRetries();
                OnPropertyChanged(nameof(DownSpeedRetries));

                _linkProperties.ForcedSpeed = fwAPI.GetLinkProp_ForcedSpeed();
                switch (_linkProperties.ForcedSpeed)
                {
                    case "SPEED_100BASE_TX_FD":
                        OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_FD));
                        break;
                    case "SPEED_100BASE_TX_HD":
                        OnPropertyChanged(nameof(IsForcedSpeed_100BASE_TX_HD));
                        break;
                    case "SPEED_10BASE_T_FD":
                        OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_FD));
                        break;
                    case "SPEED_10BASE_T_HD":
                        OnPropertyChanged(nameof(IsForcedSpeed_10BASE_T_HD));
                        break;
                    default:
                        // Do nothing
                        break;
                }

                _linkProperties.MDIX = fwAPI.GetLinkProp_MDIX();
                switch (_linkProperties.MDIX)
                {
                    case "Auto MDIX":
                        OnPropertyChanged(nameof(IsMDIX_AutoMDIX));
                        break;
                    case "Fixed MDI":
                        OnPropertyChanged(nameof(IsMDIX_FixedMDI));
                        break;
                    case "Fixed MDIX":
                        OnPropertyChanged(nameof(IsMDIX_FixedMDIX));
                        break;
                    default:
                        // Do nothing
                        break;
                }

                _linkProperties.EnergyDetectPowerDownMode = fwAPI.GetLinkProp_EDPD();
                switch (_linkProperties.EnergyDetectPowerDownMode)
                {
                    case "Disabled":
                        OnPropertyChanged(nameof(IsEDPD_Disabled));
                        break;
                    case "Enabled":
                        OnPropertyChanged(nameof(IsEDPD_Enabled));
                        break;
                    case "Enabled with Periodic Pulse TX":
                        OnPropertyChanged(nameof(IsEDPD_EnabledWithPeriodicPulseTx));
                        break;
                    default:
                        // Do nothing
                        break;
                }

                _linkProperties.MasterSlaveAdvertise = fwAPI.GetLinkProp_LeadFollow();
                switch (_linkProperties.MasterSlaveAdvertise)
                {
                    case "Leader":
                        OnPropertyChanged(nameof(IsLeaderFollower_Leader));
                        break;
                    case "Follower":
                        OnPropertyChanged(nameof(IsLeaderFollower_Follower));
                        break;
                    default:
                        // Do nothing
                        break;
                }

                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
                OnPropertyChanged(nameof(IsEEE1000Enabled));
                OnPropertyChanged(nameof(IsEEE100Enabled));
                OnPropertyChanged(nameof(IsLeaderFollowerVisible));

                OnPropertyChanged(nameof(HasActivePhyMode));
                OnPropertyChanged(nameof(ActivePhyMode));

                _selectedDeviceStore.OnLoadingStatusChanged(this, false, "Values loaded");
                HasLoadedValues = true;
            }
        }
    }
}