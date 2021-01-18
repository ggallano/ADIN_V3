using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TargetInterface;
using DeviceCommunication;

namespace TargetInterfaceTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestReadYodaRgIndirectAccess()
        {
            DeviceConnection deviceConnection = new DeviceConnection();

            try
            {
                deviceConnection.Open();
                FirmwareAPI fwAPI = new FirmwareAPI();
                fwAPI.AttachDevice(deviceConnection);
                Assert.AreEqual(48144U, fwAPI.ReadYodaRg("GESubsys", "GePhyId2"));
                Assert.AreEqual(1U, fwAPI.ReadYodaRg("GESubsys", "GeModelNum"));
                Assert.AreEqual(0U, fwAPI.ReadYodaRg("GESubsys", "GeRevNum"));
                Assert.AreEqual(47U, fwAPI.ReadYodaRg("GESubsys", "GePhyId2Oui"));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                deviceConnection.Close();
            }
        }

        [TestMethod]
        public void TestReadYodaRgDirectAccess()
        {
            DeviceConnection deviceConnection = new DeviceConnection();

            try
            {
                deviceConnection.Open();
                FirmwareAPI fwAPI = new FirmwareAPI();
                fwAPI.AttachDevice(deviceConnection);

                Assert.AreEqual(643U, fwAPI.ReadYodaRg("GEPhy", "PhyId1"));

                Assert.AreEqual(48144U, fwAPI.ReadYodaRg("GEPhy", "PhyId2"));
                Assert.AreEqual(0U, fwAPI.ReadYodaRg("GEPhy", "RevNum"));
                Assert.AreEqual(1U, fwAPI.ReadYodaRg("GEPhy", "ModelNum"));
                Assert.AreEqual(47U, fwAPI.ReadYodaRg("GEPhy", "PhyId2Oui"));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                deviceConnection.Close();
            }
        }

        [TestMethod]
        public void TestReadYodaRgMMapDoesntExist()
        {
            DeviceConnection deviceConnection = new DeviceConnection();

            try
            {
                bool ExceptionThrown = false;
                deviceConnection.Open();
                FirmwareAPI fwAPI = new FirmwareAPI();
                fwAPI.AttachDevice(deviceConnection);

                try
                {
                    Assert.AreEqual(643U, fwAPI.ReadYodaRg("GEPhy2", "PhyId1"));

                }
                catch (System.ArgumentException e)
                {
                    ExceptionThrown = true;
                    Assert.IsTrue(e.Message.Contains("Information on register or field \"PhyId1\" is not available"), e.Message);
                }
                Assert.IsTrue(ExceptionThrown, "Exception expected : " + ExceptionThrown);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                deviceConnection.Close();
            }
        }

        [TestMethod]
        public void TestReadYodaRgRegDoesntExist()
        {
            DeviceConnection deviceConnection = new DeviceConnection();

            try
            {
                bool ExceptionThrown = false;
                deviceConnection.Open();
                FirmwareAPI fwAPI = new FirmwareAPI();
                fwAPI.AttachDevice(deviceConnection);

                try
                {
                    Assert.AreEqual(643U, fwAPI.ReadYodaRg("GEPhy", "PhyId122"));
                }

                catch (System.ArgumentException e)
                {
                    ExceptionThrown = true;
                    Assert.IsTrue(e.Message.Contains("Information on register or field \"PhyId122\" is not available"), e.Message);
                }
                Assert.IsTrue(ExceptionThrown, "Exception expected : " + ExceptionThrown);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                deviceConnection.Close();
            }
        }


    }
}
