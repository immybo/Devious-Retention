using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Devious_Retention_Tests
{

    /// <summary>
    /// Summary description for MenuConnectionTests
    /// </summary>
    [TestClass]
    public class MenuConnectionTests
    {
        public Devious_Retention_Menu.Connection sender;
        public Devious_Retention_Menu.Connection receiver;

        [TestCleanup]
        public void CloseConnections()
        {
            if (sender != null) sender.Close();
            if (receiver != null) receiver.Close();
        }

        /// <summary>
        /// Makes sure that a connection can connect and be connected to
        /// on the local machine.
        /// </summary>
        [TestMethod]
        public void TestLocalConnection()
        {
            sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            receiver = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);

            Thread listenThread = new Thread(new ThreadStart(receiver.ListenForConnection));
            listenThread.Start();
            sender.Connect();
            
            sender.WriteLine("sender to receiver");

            Assert.AreEqual("sender to receiver", receiver.ReadLine());

            receiver.WriteLine("receiver to sender");

            Assert.AreEqual("receiver to sender", sender.ReadLine());

            listenThread.Abort();
        }

        /// <summary>
        /// Makes sure that a connection can't connect to an address which
        /// has no listening connection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLocalConnectionUnableToConnect()
        {
            sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            receiver = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);

            sender.Connect();
        }

        /// <summary>
        /// Makes sure that no lines can be written when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidWrite()
        {
            sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            sender.WriteLine("test");
        }

        /// <summary>
        /// Makes sure that no lines can be read when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidRead()
        {
            sender = new Devious_Retention_Menu.Connection(IPAddress.Parse("127.0.0.1"), 2942);
            sender.ReadLine();
        }
    }
}
