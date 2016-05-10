using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Devious_Retention_Tests
{
    /// <summary>
    /// Summary description for MenuConnectionTests
    /// </summary>
    [TestClass]
    public class MenuConnectionTests
    {
        /// <summary>
        /// Makes sure that a connection can connect and be connected to
        /// on the local machine.
        /// </summary>
        [TestMethod]
        public void TestLocalConnection()
        {
            Devious_Retention_Menu.Connection sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            Devious_Retention_Menu.Connection receiver = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);

            receiver.ListenForConnection(10000);
            sender.Connect();

            sender.WriteLine("sender to receiver");

            Assert.AreEqual("sender to reveiver", receiver.ReadLine());

            receiver.WriteLine("receiver to sender");

            Assert.AreEqual("receiver to sender", sender.ReadLine());
        }

        /// <summary>
        /// Makes sure that a connection can't be connected to, and times out, when
        /// attempting to listen for a connection and not receiving one.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void TestLocalConnectionTimeout()
        {
            Devious_Retention_Menu.Connection connection = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            connection.ListenForConnection(500);
        }

        /// <summary>
        /// Makes sure that a connection can't connect to an address which
        /// has no listening connection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLocalConnectionUnableToConnect()
        {
            Devious_Retention_Menu.Connection sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            Devious_Retention_Menu.Connection receiver = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);

            sender.Connect();
        }

        /// <summary>
        /// Makes sure that no lines can be written when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidWrite()
        {
            Devious_Retention_Menu.Connection connection = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            connection.WriteLine("test");
        }

        /// <summary>
        /// Makes sure that no lines can be read when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidRead()
        {
            Devious_Retention_Menu.Connection connection = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            connection.ReadLine();
        }
    }
}
