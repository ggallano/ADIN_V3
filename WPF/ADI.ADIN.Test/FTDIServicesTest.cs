using FTD2XX_NET;
using FTDIChip.Driver.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace ADI.ADIN.Test
{
    [TestClass]
    public class FTDIServicesTest
    {
        private FTDI myFtdiDevice;
        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

        public FTDIServicesTest()
        {
            //_ftdiService = new FTDIServices();
            myFtdiDevice = new FTDI();

            //uint baudRate = 115200;
            //ftStatus = myFtdiDevice.SetBaudRate(baudRate);
            //ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            //ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            //ftStatus = myFtdiDevice.SetTimeouts(100, 0);
        }

        [TestMethod]
        public void OpenTest()
        {
            myFtdiDevice.OpenBySerialNumber("AU6X0HS7");
            uint baudRate = 115200;
            ftStatus = myFtdiDevice.SetBaudRate(baudRate);
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            ftStatus = myFtdiDevice.SetTimeouts(100, 0);

            ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_TX | FTDI.FT_PURGE.FT_PURGE_RX);

            string dataToWrite = $"mdiord_cl45 0,{1966083.ToString("X")}\n";
            Trace.WriteLine($"Command: {dataToWrite}");
            //string dataToWrite = $"?\n";
            UInt32 numBytesWritten = 0;
            ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
            Thread.Sleep(50);

            UInt32 numBytesAvailable = 0;
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);

            string readData;
            UInt32 numBytesRead = 0;
            // Note that the Read method is overloaded, so can read string or byte array data
            ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
            Trace.WriteLine($"Response: {readData}");

        }
    }
}
