using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceCommunication;
using Utilities;
using Utilities.Feedback;
using Utilities.JSONParser;

namespace DeviceCommunicationTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestOpenConnectionNotPresent()
        {
            bool ExceptionThrown = false;
            var deviceConnection = new DeviceConnection("NOTAVALIDSERIAL");

            try
            {
                deviceConnection.Open();
            }

            catch (System.ArgumentException e)
            {
                ExceptionThrown = true;
                Assert.IsTrue(e.Message.Contains("Invalid device requested"), e.Message);
            }
            finally
            {
                deviceConnection.Close();
            }
            Assert.IsTrue(ExceptionThrown,"Exception expected : " + ExceptionThrown);
        }
        [TestMethod]
        public void TestReadMDIORegister()
        {
            DeviceConnection deviceConnection = new DeviceConnection();
            uint differences = 0x0;
            JSONParserEngine json = new JSONParserEngine();
            json.ParseJSONData("registers_adin1300.json");

            try
            {
                deviceConnection.Open();
            
                foreach (Utilities.JSONParser.JSONClasses.RegisterDetails regdetail in json.RegisterFieldMapping.Registers)
                {
                    if (deviceConnection.ReadMDIORegister(regdetail.Address) != regdetail.ResetValue)
                    {
                        differences += 1;
                    }
                }
            }
            finally
            {
                deviceConnection.Close();
            }
            Assert.IsTrue(differences == 0x40, differences.ToString() + " " + json.RegisterFieldMapping.Registers.GetLength(0));
        }
    }
}
