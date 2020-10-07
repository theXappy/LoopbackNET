using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using LoopbackNET.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoopbackNET.Tests
{
    [TestClass]
    public class LoopbackTests
    {
        [TestInitialize]
        public void RemovePreviousLoopbacks()
        {
            // We need this to run before every test because user might have defined 
            // loopbacks outside our testing session
            Loopback.RemoveAll();
        }

        [TestCleanup]
        public void RemoveCreatedLoopbacks()
        {
            // We need this to run after every test especially to not leave behind
            // the last test's interfaces still in the system
            Loopback.RemoveAll();
        }


        [TestMethod]
        public void Create_InterfaceCreated()
        {
            // Arrange

            //Act
            LoopbackInterface res = LoopbackNET.Loopback.Create();
            string createdIFaceId = res.NetworkInterface.Id;

            // Assert
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.NetworkInterface);
            int amountOfMatchingIfaces =
                NetworkInterface.GetAllNetworkInterfaces().Count(iface => iface.Id == createdIFaceId);
            Assert.IsTrue(amountOfMatchingIfaces == 1,
                            $"Expected exactly ONE interface with the created interface id, but found {amountOfMatchingIfaces}");
        }

        [TestMethod]
        public void Create_WithName_InterfaceCreated()
        {
            // Arrange
            Random r = new Random();
            string ifaceName = "TestInterface" + r.Next();

            //Act
            LoopbackInterface res = LoopbackNET.Loopback.Create(ifaceName);
            string createdIFaceId = res.NetworkInterface.Id;

            // Assert
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.NetworkInterface);
            Assert.AreEqual(res.NetworkInterface.Name, ifaceName);
            int amountOfMatchingIfaces =
                NetworkInterface.GetAllNetworkInterfaces().Count(iface => iface.Id == createdIFaceId);
            Assert.IsTrue(amountOfMatchingIfaces == 1,
                            $"Expected exactly ONE interface with the created interface id, but found {amountOfMatchingIfaces}");
        }


        [TestMethod]
        public void Remove_InterfaceRemoved()
        {
            // Arrange

            //Act
            LoopbackInterface res = LoopbackNET.Loopback.Create();
            string createdIFaceId = res.NetworkInterface.Id;
            res.Remove();

            // Assert
            Assert.IsFalse(NetworkInterface.GetAllNetworkInterfaces().Any(iface => iface.Id == createdIFaceId),
                            "Expected NO interfaces with the created interface id, but found at least 1");
        }

        [TestMethod]
        public void Remove_WithName_InterfaceRemoved()
        {
            // Arrange
            Random r = new Random();
            string ifaceName = "TestInterface" + r.Next();

            //Act
            LoopbackInterface res = LoopbackNET.Loopback.Create(ifaceName);
            string createdIFaceId = res.NetworkInterface.Id;
            res.Remove();

            // Assert
            Assert.IsFalse(NetworkInterface.GetAllNetworkInterfaces().Any(iface => iface.Id == createdIFaceId),
                            "Expected NO interfaces with the created interface id, but found at least 1");
        }


        [TestMethod]
        public void RemoveAll_AllLoopbackNetIfaces_AllRemoved()
        {
            // Arrange
            //  creating some interfaces
            LoopbackNET.Loopback.Create();
            LoopbackNET.Loopback.Create();
            LoopbackNET.Loopback.Create();
            Assert.IsTrue(NetworkInterface.GetAllNetworkInterfaces().Any(iface => iface.Description.Contains("Microsoft KM-TEST")),
                            "Expected At least 1 loopback interface after arrange part of test. None were found.");

            //Act
            LoopbackNET.Loopback.RemoveAll(onlyLoopbackNetIfaces: false);

            // Assert
            int amountOfMatchingIfaces =
                NetworkInterface.GetAllNetworkInterfaces().Count(iface => iface.Description.Contains("Microsoft KM-TEST"));
            Assert.IsTrue(amountOfMatchingIfaces == 0,
                            $"Expected NO loopback interfaces after arrange part of test. But {amountOfMatchingIfaces} found.");
        }



        [TestMethod]
        public void CreateNamed_CreateTwice_SameLoopbackReturned()
        {
            // Arrange
            Random r = new Random();
            string ifaceName = "TestInterface" + r.Next();


            //Act
            LoopbackInterface loop1 = LoopbackNET.Loopback.Create(ifaceName);
            LoopbackInterface loop2 = LoopbackNET.Loopback.Create(ifaceName);

            // Assert
            Assert.AreEqual(loop1.NetworkInterface.Id, loop2.NetworkInterface.Id,
                            $"Expected Network Interfaces IDs to be the same.");
        }



        [TestMethod]
        public void RemoveNamed_TwoLoopbacksExist_OtherLoopbackNotRemoved()
        {
            // Arrange
            Random r = new Random();
            string ifaceName1 = "TestInterface" + r.Next();
            string ifaceName2 = "TestInterface" + r.Next();
            //  Creating the interfaces
            LoopbackInterface loop1 = LoopbackNET.Loopback.Create(ifaceName1);
            LoopbackInterface loop2 = LoopbackNET.Loopback.Create(ifaceName2);


            //Act
            loop1.Remove();
            LoopbackInterface loop3 = LoopbackNET.Loopback.Create(ifaceName2); // should return same as loop2



            // Assert
            Assert.IsNotNull(loop3);
            Assert.IsNotNull(loop3.NetworkInterface);
            Assert.AreEqual(loop3.NetworkInterface.Id, loop2.NetworkInterface.Id,
                            $"Expected Network Interfaces ID of new interface '2' to be the same as one retrieved before removal of '1'");
        }


        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Create_BadDevconDirectory_ExceptionThrown()
        {
            // Arrange

            // Create temporary directory (which obviously doesn't contain devcon.exe)
            string tempPath = Path.GetTempPath();
            string tempDir = Path.Combine(tempPath, Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempDir);
            }
            catch (Exception e)
            {
                throw new Exception("Could not prepare test. Failed to create temporary directory.", e);
            }
            // Tell DevconHelper to search in the temp dir
            string originalDevConDir = DevconHelper.DevconDirectory;
            DevconHelper.DevconDirectory = tempDir;


            // Act
            try
            {
                Loopback.Create();
            }
            finally
            {
                DevconHelper.DevconDirectory = originalDevConDir;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void DevconHelper_BadCommand_ExceptionThrown()
        {
            // Arrange

            // Act
            DevconHelper.RunCommand("NOT_A_REAL_COMMAND");
        }
    }
}
