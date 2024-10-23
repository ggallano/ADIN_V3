// <copyright file="FTDIServices.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FTDIChip.Driver.Services
{
    public delegate List<byte> ResponseDelegate(int timeoutms);

    public class FTDIServices : IFTDIServices
    {
        #region Private Fields
        private FTDI _ftdi;
        private FTDI.FT_STATUS _ftStatus = FTDI.FT_STATUS.FT_OK;
        private uint baudRate = 115200;
        #endregion

        #region Public Property
        public event EventHandler<string> ProcessCompleted;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the communication is open.
        /// </summary>
        public bool IsComOpen => _ftdi.IsOpen;
        #endregion

        #region Constructor
        /// <summary>
        /// creates new instances
        /// </summary>
        public FTDIServices()
        {
            _ftdi = new FTDI();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// intializes the serial communication
        /// </summary>
        private void InitializedSerialSettings()
        {
            _ftStatus = _ftdi.SetBaudRate(baudRate);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException(baudRate.ToString(), ftStatus, "SetBaudRate");
            }

            _ftStatus = _ftdi.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException("Unable to set FTDI device data characteristics", ftStatus, "SetDataCharacteristics");
            }

            _ftStatus = _ftdi.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException("Unable to set FTDI device flow control", ftStatus, "SetFlowControl");
            }

            _ftStatus = _ftdi.SetTimeouts(100, 0);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException("Unable to set FTDI device timeout", ftStatus, "SetTimeouts");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Closes the communication
        /// </summary>
        public void Close()
        {
            if (IsComOpen)
            {

                string serialNum = string.Empty;
                _ftdi.GetSerialNumber(out serialNum);
                Debug.WriteLine($"{serialNum} Closed");

                _ftdi.Close();
            }
        }

        /// <summary>
        /// retrieves the available connected devices
        /// </summary>
        /// <returns>returns the list of FTDI devices</returns>
        public FTDI.FT_DEVICE_INFO_NODE[] GetDeviceList()
        {
            uint ftdiDeviceCount = 0;
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;
            FTDI.FT_STATUS ftdiStatus = FTDI.FT_STATUS.FT_DEVICE_NOT_FOUND;
            uint retry = 0;

            do
            {
                _ftdi.GetNumberOfDevices(ref ftdiDeviceCount);
                ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
                ftdiStatus = _ftdi.GetDeviceList(ftdiDeviceList);

                if (ftdiDeviceList.Count() == 0)
                    retry++;

                foreach (var device in ftdiDeviceList)
                {
                    if (device.Description != "")
                        retry++;
                }
                Thread.Sleep(1000);
            } while (retry < 5);


            return ftdiDeviceList;
        }

        /// <summary>
        /// Opens the communication
        /// </summary>
        public void Open(string serialNumber)
        {
            _ftStatus = this._ftdi.OpenBySerialNumber(serialNumber);
            if (_ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException(this.Device.ID.ToString(), ftStatus, "OpenBySerialNumber");
            }

            //always initialized the serial settings whenever you switch/open new serial com port
            InitializedSerialSettings();

            string serialNum = string.Empty;
            _ftdi.GetSerialNumber(out serialNum);
            Debug.WriteLine($"{serialNum} Open");
        }

        /// <summary>
        /// Sends the string data
        /// </summary>
        /// <param name="dataToWrite"></param>
        public void SendData(string dataToWrite)
        {
            uint numBytesWritten = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            if (!_ftdi.IsOpen)
                throw new InvalidOperationException("FTDI Device is not open.");

            ftStatus = _ftdi.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                //throw new FTDIException("Unable to write to FTDI device", ftStatus, "Write: " + ftStatus.ToString());
            }
        }

        /// <summary>
        /// Purge the data buffer
        /// </summary>
        public void Purge()
        {
            this._ftdi.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX);
        }

#if false
        public string ReadCommandResponse()
        {
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
            string readData;
            uint numBytesAvailable = 0;
            uint numBytesRead = 0;

            if (!this._ftdi.IsOpen)
                throw new InvalidOperationException("FTDI Device is not open.");

            ftStatus = _ftdi.GetRxBytesAvailable(ref numBytesAvailable);
            ftStatus = _ftdi.Read(out readData, numBytesAvailable, ref numBytesRead);
            return readData;
        }
#else
        /// <summary>
        /// reads the command respond.
        /// </summary>
        /// <returns>returns the string respond.</returns>
        public string ReadCommandResponse()
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

                if (serialchunk[0] == '\n' || serialchunk[0] == '\r')
                {
                    complete = true;
                }

                if (commandresponse.Count > 200)
                {
                    throw new ApplicationException("Lost communication with Evaluation board.");
                }
            }

            return encoding.GetString(commandresponse.ToArray());
        }

        private List<byte> WaitOnSerialResponse(int timeout)
        {
            ResponseDelegate d = new ResponseDelegate(this.GetSerialBytes);
            List<byte> serialresponse;
            //IAsyncResult res = d.BeginInvoke(timeout, null, null);
            //serialresponse = d.EndInvoke((AsyncResult)res);

            Task<List<byte>> responseTask = Task.Run(() => d.Invoke(timeout));

            responseTask.ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    serialresponse = task.Result;
                    return serialresponse;
                }
                else //if (task.Status == TaskStatus.Faulted)
                {
                    throw new Exception("Failed getting response.");
                }
            });

            //if (serialresponse == null)
            //{
            throw new ApplicationException("Timeout waiting for communication response byte.");
            //}

            //return serialresponse;
        }

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

                this._ftdi.GetRxBytesAvailable(ref bytesAvail);
                while (bytesAvail < num_bytes)
                {
                    timenow = Environment.TickCount & int.MaxValue;
                    if (timenow > (starttime + timeoutms))
                    {
                        break;
                    }

                    Thread.Sleep(timeoutms / 100);
                    this._ftdi.GetRxBytesAvailable(ref bytesAvail);
                }

                this._ftdi.Read(serialbytes, num_bytes, ref bytesRead);

                return new List<byte>(serialbytes);
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif
        public string GetSerialNumber()
        {
            string serialNumber = string.Empty;
            _ftdi.GetSerialNumber(out serialNumber);
            return serialNumber;
        }
        #endregion

        protected virtual void OnProcessCompleted(string message)
        {
            ProcessCompleted?.Invoke(this, message);
        }
    }
}
