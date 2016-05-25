using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Devious_Retention_Menu;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Devious_Retention_Tests
{

    /// <summary>
    /// Summary description for MenuConnectionTests
    /// </summary>
    [TestClass]
    public class MenuConnectionTests : IReceiverFunction, IConnectionDataListener
    {
        public Connection sender;
        public Connection receiverConnection;
        public ConnectionListener receiver;

        private BlockingCollection<string> readLinesBuffer = new BlockingCollection<string>();

        [TestCleanup]
        public void CloseConnections()
        {
            readLinesBuffer = new BlockingCollection<string>();
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
        public async Task TestLocalConnection()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);
            receiver = new ConnectionListener(2942);
            receiver.AddReceiverFunction(this);
            receiver.BeginListening();
            sender.Connect(2000);

            Thread.Sleep(500); // wait for the connection to be established

            sender.AddConnectionDataListener(this);
            receiverConnection.AddConnectionDataListener(this);
            sender.BeginListening();
            receiverConnection.BeginListening();
            
            sender.WriteLine("sender to receiver");

            string line = await GetRecentLine();

            Assert.AreEqual("sender to receiver", line);

            receiverConnection.WriteLine("receiver to sender");

            line = await GetRecentLine();

            Assert.AreEqual("receiver to sender", line);
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

            sender.Connect(2000);
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
        /// Makes sure that a connection remains closed when it is not connected.
        /// </summary>
        [TestMethod]
        public void TestConnectionClosed()
        {
            sender = new Connection(IPAddress.Parse("127.0.0.1"), 2942);
            sender.AddConnectionDataListener(this);
            Assert.IsFalse(sender.IsOpen());
        }

        public void OnConnection(Connection newClient)
        {
            receiverConnection = newClient;
        }

        public void OnLineRead(Connection connection, string line)
        {
            readLinesBuffer.TryAdd(line);
        }

        /// <summary>
        /// Asynchronously waits for a line to be available
        /// and then returns it.
        /// </summary>
        private async Task<string> GetRecentLine()
        {
            return await Task.Run(() => readLinesBuffer.Take());
        }
    }
}
