using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Devious_Retention_Menu;

namespace Devious_Retention_Tests
{

    /// <summary>
    /// Summary description for MenuConnectionTests
    /// </summary>
    [TestClass]
    public class MenuConnectionTests : IReceiverFunction
    {
        public Connection sender;
        public Connection receiverConnection;
        public ConnectionListener receiver;

    

        [TestCleanup]
        public void CloseConnections()
        {
            if (sender != null) sender.Close();
            if (receiver != null)
            {
                try { receiver.StopListening(); } catch (Exception) { }; // exception = isn't connected
            }
        }

        /// <summary>
        /// Makes sure that a connection can connect and be connected to
        /// on the local machine.
        /// </summary>
        [TestMethod]
        public void TestLocalConnection()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);
            receiver = new ConnectionListener(2942);
            receiver.AddReceiverFunction(this);
            receiver.BeginListening();
            sender.Connect();

            Thread.Sleep(500);
            
            sender.WriteLine("sender to receiver");

            Assert.AreEqual("sender to receiver", receiverConnection.ReadLine());

            receiverConnection.WriteLine("receiver to sender");

            Assert.AreEqual("receiver to sender", sender.ReadLine());
        }

        /// <summary>
        /// Makes sure that a connection can't connect to an address which
        /// has no listening connection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLocalConnectionUnableToConnect()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);

            sender.Connect();
        }

        /// <summary>
        /// Makes sure that no lines can be written when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidWrite()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);
            sender.WriteLine("test");
        }

        /// <summary>
        /// Makes sure that no lines can be read when a connection isn't connected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidRead()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);
            sender.ReadLine();
        }

        public void OnConnection(Connection newClient)
        {
            receiverConnection = newClient;
        }
    }
}
