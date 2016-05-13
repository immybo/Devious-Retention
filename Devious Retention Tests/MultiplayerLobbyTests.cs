using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Devious_Retention_Menu;
using System.Net;
using System.Threading;

namespace Devious_Retention_Tests
{
    /// <summary>
    /// Summary description for MultiplayerLobbyTests
    /// </summary>
    [TestClass]
    public class MultiplayerLobbyTests
    {
        private MultiplayerLobbyHandler[] clients;
        private LobbyHost host;

        [TestCleanup]
        public void CloseConnections()
        {
            if(host!= null)
                host.Close();
        }

        /// <summary>
        /// Makes sure that a server can listen for a connection to 
        /// a client, after which the client can connect to the server,
        /// and they can exchange information.
        /// </summary>
        [TestMethod]
        public void TestBasicClientServerConnection()
        {
            host = new LobbyHost(8);

            clients = new MultiplayerLobbyHandler[1];
            clients[0] = new MultiplayerLobbyHandler(IPAddress.Parse("127.0.0.1"));

            clients[0].UpdatePlayerNumber(1);
            clients[0].UpdateUsername("test");
            clients[0].UpdateColor("Red");
            clients[0].UpdateFactionName("testfaction");

            Thread.Sleep(50);

            try {
                Assert.AreEqual("0 1 test Red testfaction\n", clients[0].ToString());
            }
            catch(Exception e)
            {
                Assert.Fail("Output was " + clients[0].ToString() + ". Expected 0 1 text Red testfaction\n.");
            }
        }

        /// <summary>
        /// Makes sure that a server can't accept more connections 
        /// than lobby slots it has.
        /// </summary>
        [TestMethod]
        public void TestInvalidConnection()
        {
            host = new LobbyHost(3);
            clients = new MultiplayerLobbyHandler[4];
            for (int i = 0; i < 4; i++)
                clients[i] = new MultiplayerLobbyHandler(IPAddress.Parse("127.0.0.1"));

            Thread.Sleep(500); // Give it some time... the connected thing is handled async, so we can't wait for it in another way

            Assert.IsTrue(clients[0].Connected);
            Assert.IsTrue(clients[1].Connected);
            Assert.IsTrue(clients[2].Connected);
            Assert.IsFalse(clients[3].Connected);
        }

        [TestMethod]
        public void TestClientServerMultipleConnections()
        {
            LobbyHost host = new LobbyHost(8);
            clients = new MultiplayerLobbyHandler[8];
            for (int i = 0; i < 8; i++)
                clients[i] = new MultiplayerLobbyHandler(IPAddress.Parse("127.0.0.1"));
        }
    }
}
