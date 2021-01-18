//-----------------------------------------------------------------------
// <copyright file="DeviceConnection.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------
namespace DeviceCommunication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Threading;
    using FTD2XX_NET;
    using Utilities.Feedback;
    using System.Collections.Specialized;

    /// <summary>
    /// Serial Port Delegate
    /// </summary>
    /// <param name="timeoutms">Timeout in milliseconds waiting for that response byte</param>
    /// <returns>The response from the firmware</returns>
    public delegate List<byte> ResponseDelegate(int timeoutms);

    /// <summary>
    /// This is a device communication class for UART communication
    /// </summary>
    public class DeviceConnection : FeedbackPropertyChange
    {
        /// <summary>
        /// The device description that identifies the 10SPE board
        /// </summary>
        public const string DeviceDescriptionEVALADIN11xx = "EVAL-ADIN1100FMCZ";//"EVAL-10SPE";

        /// <summary>
        /// The device description that identifies our MDIO dongle
        /// </summary>
        private const string DeviceDescription1 = "ADIN1300 MDIO DONGLE";

        /// <summary>
        /// The device description that identifies our MDIO dongle
        /// </summary>
        private const string DeviceDescription2 = "ADIN1200 MDIO DONGLE";

        /// <summary>
        /// Stores the list of serial numbers of connected MDIO dongles
        /// </summary>
        private static List<string> deviceSerialNumbers = new List<string>();

        /// <summary>
        /// Flag to indicate if this is an initial scan
        /// </summary>
        private static bool initialScan = true;

        /// <summary>
        /// The serial number of the connected device
        /// </summary>
        private string deviceID;

        /// <summary>
        /// The mdio address to be used
        /// </summary>
        private uint mdioAddress = 0;

        /// <summary>
        /// Device has been already discovered and recognised
        /// </summary>
        private bool deviceRecognised = false;

        /// <summary>
        /// The device description
        /// </summary>
        private string deviceDescription = "ADIN1300 MDIO DONGLE";

        /// <summary>
        /// FTDI Dev
        /// </summary>
        private FTDI ftdiDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceConnection"/> class.
        /// </summary>
        /// <param name="deviceID">Device ID of device to connect to </param>
        public DeviceConnection(string deviceID)
        {
            this.mdioAddress = 0x0;

            /* See if we have an override value for the the MDIO address to be used. */
            for (int i = 0; i < Properties.Settings.Default.DongleMDIOAddrOverRide.Count; i += 2)
            {
                if (Properties.Settings.Default.DongleMDIOAddrOverRide[i] == deviceID)
                {
                    if (i + 1 != Properties.Settings.Default.DongleMDIOAddrOverRide.Count)
                    {
                        if (uint.TryParse(Properties.Settings.Default.DongleMDIOAddrOverRide[i + 1], out this.mdioAddress))
                        {
                            break;
                        }
                    }
                }
            }
            if (this.Claus45Addressing())
            {
                uint dev_ID = 0;
                for (uint i = 0; i < 7; i++)
                {
                    this.mdioAddress = i;
                    dev_ID = this.ReadMDIORegister(0x1e0003);
                    if (dev_ID == 0xbc80)
                    {
                        break;
                    }
                }
            }
            this.ftdiDevice = new FTDI();
            this.deviceID = deviceID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceConnection"/> class.
        /// </summary>
        public DeviceConnection()
        {
            this.ftdiDevice = new FTDI();
            this.mdioAddress = 0x0;
            if (deviceSerialNumbers.Count > 0)
            {
                this.deviceID = deviceSerialNumbers[0];
            }
            else
            {
                this.deviceID = null;
            }
        }

        /// <summary>
        /// A static event representing changes to static property DeviceSerialNumbers
        /// </summary>
        public static event EventHandler DeviceSerialNumbersChanged;

        /// <summary>
        /// Gets the list of Serial ports available
        /// </summary>
        public static List<string> DeviceSerialNumbers
        {
            get
            {
                return DeviceConnection.deviceSerialNumbers;
            }
        }

        /// <summary>
        /// Gets a value indicating the device description
        /// </summary>
        public string DeviceDescription
        {
            get
            {
                return this.deviceDescription;
            }
        }

        /// <summary>
        /// Returns a value indicating if claus 45 addressing is to be used
        /// </summary>
        /// <returns>bool</returns>
        public bool Claus45Addressing()
        {
            return this.DeviceDescription == DeviceConnection.DeviceDescriptionEVALADIN11xx;
        }

        /// <summary>
        /// Sets the Phy HW address
        ///
        /// </summary>
        /// <param name="hwAddress"> Phy HW address</param>
        public void ModifyMDIOAddress(uint hwAddress)
        {
            this.mdioAddress = hwAddress;
        }

        /// <summary>
        /// Gets selected Phy HW address
        /// </summary>
        /// <returns>Phy HW address</returns>
        public uint GetMDIOAddress()
        {
            return this.mdioAddress;
        }

        /// <summary>
        /// is the device already recognised
        /// </summary>
        /// <returns> true - dev is recognised</returns>
        public bool IsDevRecognised()
        {
            return this.deviceRecognised;

        }

        /// <summary>
        /// is the device already recognised
        /// </summary>
        /// <param name="dev"> true -dev is recognised</param>
        public void IsDevRecognised(bool dev)
        {
            this.deviceRecognised = dev;

        }

        /// <summary>
        /// This method refreshes the list of FTDI devices
        /// </summary>
        public static void RefreshConnectedDevices()
        {
            uint ftdiDeviceCount = 0;
            uint ftdiRetryCount = 10;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
            FTDI ftdiDevice = new FTDI();
            List<string> deviceSerialNumbers = new List<string>();
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;

            // Determine the number of FTDI devices connected to the machine
            ftStatus = ftdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException(string.Empty, ftStatus, "GetNumberOfDevices");
            }

            if (ftdiDeviceCount != 0)
            {
                bool retryGetDeviceList = true;
                while (retryGetDeviceList)
                {
                    retryGetDeviceList = false;

                    ftStatus = ftdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        throw new FTDIException(string.Empty, ftStatus, "GetNumberOfDevices");
                    }

                    ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
                    try
                    {
                        ftStatus = ftdiDevice.GetDeviceList(ftdiDeviceList);
                        if (ftStatus != FTDI.FT_STATUS.FT_OK)
                        {
                            throw new FTDIException(string.Empty, ftStatus, "GetDeviceList");
                        }

                        /* If we see unknown devices in the list after a hardware change then it might be
                           that the FTDI is still querying the device. Retry after a short delay.
                         */
                        for (uint i = 0; i < ftdiDeviceCount; i++)
                        {
                            if (ftdiDeviceList[i] != null)
                            {
                                if ((ftdiDeviceList[i].Type == FTDI.FT_DEVICE.FT_DEVICE_UNKNOWN) && (ftdiRetryCount > 0))
                                {
                                    ftdiRetryCount--;
                                    retryGetDeviceList = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (FTDI.FT_EXCEPTION e)
                    {
                        if ((e.Message == "The supplied buffer is not big enough.") && (ftdiRetryCount > 0))
                        {
                            /* If a device was added in between calling GetNumberOfDevices  and
                               GetDeviceList then GetDeviceList will throw an exception as
                               we have not allocated enough space. Just retry in this situation.

                             */
                            ftdiRetryCount--;
                            retryGetDeviceList = true;
                        }
                        else
                        {
                            /* Some other unhandled error.*/
                            throw new FTDIException(string.Empty, ftStatus, "GetDeviceList");
                        }
                    }

                    if (!retryGetDeviceList)
                    {
                        for (uint i = 0; i < ftdiDeviceCount; i++)
                        {
                            /* If a device was removed in between calling GetNumberOfDevices  and
                               GetDeviceList then we will have some empty locations.
                             */
                            if ((ftdiDeviceList[i] != null) && (ftdiDeviceList[i].Description != null))
                            {
                                if ((ftdiDeviceList[i].Description == DeviceDescription1) || (ftdiDeviceList[i].Description == DeviceDescription2) || (ftdiDeviceList[i].Description == DeviceDescriptionEVALADIN11xx))
                                {
                                    deviceSerialNumbers.Add(ftdiDeviceList[i].SerialNumber.ToString());
                                }
                            }
                        }

                        deviceSerialNumbers.Sort();
                    }
                    else
                    {
                        /* We are going to retry but wait a little before doing so */
                        Thread.Sleep(100);
                    }
                }
            }

            bool differences = false;

            if (deviceSerialNumbers.Count != DeviceConnection.deviceSerialNumbers.Count)
            {
                differences = true;
            }
            else
            {
                for (int i = 0; i < deviceSerialNumbers.Count; i++)
                {
                    if (deviceSerialNumbers[i] != DeviceConnection.deviceSerialNumbers[i])
                    {
                        differences = true;
                        break;
                    }
                }
            }

            if (differences || DeviceConnection.initialScan)
            {
                DeviceConnection.deviceSerialNumbers = deviceSerialNumbers;
                OnDeviceSerialNumbersChanged(EventArgs.Empty);
            }

            DeviceConnection.initialScan = false;
        }

        /// <summary>
        /// Verifies if the serial port is open currently
        /// </summary>
        /// <returns>Returns if the serial port is open</returns>
        public bool IsOpen()
        {
            return this.ftdiDevice.IsOpen;
        }

        /// <summary>
        /// This method sets the passed parameters of the port and opens the connection to the port
        /// </summary>
        /// <exception cref = "FTDIException">Thrown if some error when opening the FTDI device </exception>
        /// <returns>Returns a value indicating if the connection is successful</returns>
        public bool Open()
        {
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
            uint baudRate = 115200;

            if (this.deviceID == null)
            {
                throw new ArgumentException("Invalid device requested", "deviceIndex");
            }

            ftStatus = this.ftdiDevice.OpenBySerialNumber(this.deviceID);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException(this.deviceID, ftStatus, "OpenBySerialNumber");
            }

            ftStatus = this.ftdiDevice.GetDescription(out this.deviceDescription);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException(this.deviceID, ftStatus, "GetDescription");
            }

            ftStatus = this.ftdiDevice.SetBaudRate(baudRate);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException(baudRate.ToString(), ftStatus, "SetBaudRate");
            }

            ftStatus = this.ftdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException("Unable to set FTDI device data characteristics", ftStatus, "SetDataCharacteristics");
            }

            ftStatus = this.ftdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException("Unable to set FTDI device flow control", ftStatus, "SetFlowControl");
            }

            ftStatus = this.ftdiDevice.SetTimeouts(100, 0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException("Unable to set FTDI device timeout", ftStatus, "SetTimeouts");
            }

            return true;
        }

        /// <summary>
        /// This method closes the connection with the serial port
        /// </summary>
        public void Close()
        {
            if (this.IsOpen())
            {
                this.ftdiDevice.Close();
            }
        }
 
        /// <summary>
        /// Read an MDIO Register
        /// </summary>
        /// <param name="regAddr">Register to read</param>
        /// <returns>Returns register content</returns>
        public uint ReadMDIORegister(uint regAddr)
        {
            string readData;
            uint regContent = 0;

            uint pageNumber = regAddr >> 16;
            uint pageAddr = regAddr & 0xFFFF;

            this.Purge();
            if (this.Claus45Addressing())
            {
                // mdiord_cl45<phyAddress>,< register address in Hex ><\n >
                this.SendData(string.Format("mdiord_cl45 {0:X},{1:X}\n", this.mdioAddress, regAddr));
                /* ...and get the response */
                readData = this.ReadCommandResponse();
            }
            else
            {
                if (pageNumber == 0)
                {
                    Debug.Assert(pageAddr <= 0x1F, "Invalid page address in page 0");

                    this.SendData(string.Format("mdioread {1:X},{0:X}\n", pageAddr, this.mdioAddress));

                    /* ...and get the response */
                    readData = this.ReadCommandResponse();
                }
                else
                {
                    Debug.Assert(pageNumber <= 0x1E, "Invalid page number, Must be 0 or 30");

                    // Indirect access
                    this.WriteMDIORegister(0x10, pageAddr);

                    this.SendData(string.Format("mdioread {0:X},11\n", this.mdioAddress));
                    /* ...and get the response */
                    readData = this.ReadCommandResponse();
                }
            }

            if (!uint.TryParse(readData, System.Globalization.NumberStyles.HexNumber, null, out regContent))
            {
                throw new ApplicationException("invalid response");
            }

            return regContent;
        }

        /// <summary>
        /// Read an MDIO Register
        /// </summary>
        /// <param name="regAddr">Register to read</param>
        /// <param name="value">Value to write</param>
        public void WriteMDIORegister(uint regAddr, uint value)
        {
            string readData;

            uint pageNumber = regAddr >> 16;
            uint pageAddr = regAddr & 0xFFFF;

            this.Purge();
            if (this.Claus45Addressing())
            {
                // mdiowr_cl45<phyAddress>,< register address in Hex >,< data in hex ><\n >
                this.SendData(string.Format("mdiowr_cl45 {2:X},{0:X},{1:X}\n", regAddr, value, this.mdioAddress));
                /* ...and get the response */
                readData = this.ReadCommandResponse();
            }
            else
            {
                if (pageNumber == 0)
                {
                    Debug.Assert(pageAddr <= 0x1F, "Invalid page address in page 0");
                    this.SendData(string.Format("mdiowrite {2:X},{0:X},{1:X}\n", pageAddr, value, this.mdioAddress));
                    /* ...and get the response */
                    readData = this.ReadCommandResponse();
                }
                else
                {
                    Debug.Assert(pageNumber <= 0x1E, "Invalid page number, Must be 0 or 30");

                    // Indirect access
                    this.SendData(string.Format("mdiowrite {1:X},10,{0:X}\n", pageAddr, this.mdioAddress));
                    /* ...and get the response */
                    readData = this.ReadCommandResponse();
                    this.SendData(string.Format("mdiowrite {1:X},11,{0:X}\n", value, this.mdioAddress));
                    /* ...and get the response */
                    readData = this.ReadCommandResponse();
                }
            }
        }

        /// <summary>
        /// Raise the change event through this static method
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected static void OnDeviceSerialNumbersChanged(EventArgs e)
        {
            EventHandler handler = DeviceSerialNumbersChanged;

            if (handler != null)
            {
                handler(null, e);
            }
        }

        /// <summary>
        /// A command has been issued , now read a command response
        /// </summary>
        /// <returns>String Read</returns>
        private string ReadCommandResponse()
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            bool complete = false;
            List<byte> serialchunk = null;
            List<byte> commandresponse = new List<byte>();

            while (!complete)
            {
                /* The timeout for the initial byte back is command dependent */
                serialchunk = this.WaitOnSerialResponse(10);
                if (serialchunk == null)
                {
                    throw new ApplicationException("Lost communication with Evaluation board.");
                }

                /* Packet transmission is underway? Capture the byte */
                commandresponse.Add(serialchunk[0]);

                if (serialchunk[0] == '\n')
                {
                    complete = true;
                }

                if (commandresponse.Count > 30)
                {
                    throw new ApplicationException("Lost communication with Evaluation board.");
                }
            }

            return encoding.GetString(commandresponse.ToArray());
        }

        /// <summary>
        /// This method reads the available data from the serial port
        /// </summary>
        /// <returns>Returns the read value as string</returns>
        private string ReadData()
        {
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
            string readData;
            uint numBytesAvailable = 0;
            uint numBytesRead = 0;
            ftStatus = this.ftdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
            return readData;
        }

        /// <summary>
        /// This method sends the bytes to the serial port
        /// </summary>
        /// <param name="data">The data in the form of byte array</param>
        /// <param name="count">The count of the number of bytes</param>
        private void SendData(byte[] data, int count)
        {
            uint numBytesWritten = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            if (!this.ftdiDevice.IsOpen)
            {
                throw new InvalidOperationException("FTDI Device is not open.");
            }

            ftStatus = this.ftdiDevice.Write(data, count, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException("Unable to write to FTDI device", ftStatus, "Write: " + ftStatus.ToString());
            }
        }

        /// <summary>
        /// This method sends the bytes to the serial port
        /// </summary>
        /// <param name="dataToWrite">The data in the form of string</param>
        private void SendData(string dataToWrite)
        {
            uint numBytesWritten = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            if (!this.ftdiDevice.IsOpen)
            {
                throw new InvalidOperationException("FTDI Device is not open.");
            }

            ftStatus = this.ftdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                throw new FTDIException("Unable to write to FTDI device", ftStatus, "Write: " + ftStatus.ToString());
            }
        }

        /// <summary>
        /// Read specified number of bytes from the serial port
        /// </summary>
        /// <param name="timeoutms">Timeout in milliseconds waiting for that response byte</param>
        /// <returns>Bytes Read</returns>
        private List<byte> GetSerialBytes(int timeoutms)
        {
            uint num_bytes = 1;
            byte[] serialbytes = new byte[num_bytes];
            uint bytesRead = 0;

            /* This function has a timeout implemented in WaitOnSerialResponse
                Wait until the number of bytes are available
             */
            try
            {
                int starttime = Environment.TickCount & int.MaxValue;
                int timenow;
                uint bytesAvail = 0;

                this.ftdiDevice.GetRxBytesAvailable(ref bytesAvail);
                while (bytesAvail < num_bytes)
                {
                    timenow = Environment.TickCount & int.MaxValue;
                    if (timenow > (starttime + timeoutms))
                    {
                        break;
                    }

                    Thread.Sleep(timeoutms / 100);
                    this.ftdiDevice.GetRxBytesAvailable(ref bytesAvail);
                }

                this.ftdiDevice.Read(serialbytes, num_bytes, ref bytesRead);

                return new List<byte>(serialbytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Wait for the specified timeout for a response byte to be received.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds waiting for that response byte</param>
        /// <returns>Bytes Read</returns>
        private List<byte> WaitOnSerialResponse(int timeout)
        {
            ResponseDelegate d = new ResponseDelegate(this.GetSerialBytes);
            List<byte> serialresponse;
            IAsyncResult res = d.BeginInvoke(timeout, null, null);
            serialresponse = d.EndInvoke((AsyncResult)res);
            if (serialresponse == null)
            {
                throw new ApplicationException("Timeout waiting for communication response byte.");
            }

            return serialresponse;
        }

        /// <summary>
        /// Purge the communication buffers
        /// </summary>
        private void Purge()
        {
            this.ftdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX);
        }
    }
}
