using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using Devious_Retention;
using static Devious_Retention.GameBuilder;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A lobby host communicates with all lobby clients and allows for
    /// new clients to connect through it.
    /// </summary>
    public class LobbyHost : MenuItemHandler, IReceiverFunction, IConnectionDataListener
    {
        public const int HOST_CONNECTION_PORT = 4985; // The port which a lobby host will listen for new connections on

        private int currentUniqueID;
        private Dictionary<Connection, ClientData> clients;

        private ConnectionListener connectionListener;

        private int maxPlayers;

        private bool closed;

        public LobbyHost(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            closed = false;
            currentUniqueID = 0;

            clients = new Dictionary<Connection, ClientData>();

            // Build the connection listener
            connectionListener = new ConnectionListener(HOST_CONNECTION_PORT);
            connectionListener.AddReceiverFunction(this);
            connectionListener.BeginListening();
        }

        /// <summary>
        /// Refreshes each client's knowledge of the specified client data.
        /// </summary>
        private void UpdateClients(ClientData client)
        {
            foreach(Connection c in clients.Keys)
            {
                c.WriteLine("update " + client.ToString());
            }
        }

        /// <summary>
        /// Refreshes each client's knowledge of all client data.
        /// </summary>
        private void UpdateClientsAll()
        {
            foreach(ClientData data in clients.Values)
            {
                foreach(Connection c in clients.Keys)
                {
                    c.WriteLine(data.ToString());
                }
            }
        }

        /// <summary>
        /// Tells each client that a client has left the lobby.
        /// </summary>
        private void UpdateClientsClose(ClientData data)
        {
            foreach(Connection connection in clients.Keys)
            {
                connection.WriteLine("terminate " + data.uniqueID);
            }
        }

        /// <summary>
        /// Ends this lobby host, terminating all connections nicely.
        /// </summary>
        public void Close()
        {
            closed = true;
            if (connectionListener != null)
                connectionListener.StopListening();
            foreach (Connection c in clients.Keys)
            {
                c.WriteLine("terminate");
                c.Close();
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (ClientData client in clients.Values)
            {
                b.Append(client.ToString() + "\n");
            }
            return b.ToString();
        }

        /// <summary>
        /// Called when a new client connects.
        /// Adds the client to the list of clients and begins listening to the connection.
        /// </summary>
        public void OnConnection(Connection newClient)
        {
            if(clients.Count == maxPlayers || closed)
            {
                newClient.WriteLine("full");
            }

            clients.Add(newClient, new ClientData(currentUniqueID, "Default", GetFreePlayerNumber(), Color.Black, "Default Faction"));
            currentUniqueID++;

            newClient.AddConnectionDataListener(this);
            newClient.BeginListening();
        }

        /// <summary>
        /// On receiving data from a client, updates all clients' information
        /// and this server's knowledge; terminates the client if the command
        /// is given.
        /// </summary>
        public void OnLineRead(Connection connection, string line)
        {
            if (closed) return;

            ClientData client;
            try
            {
                client = clients[connection];
            }
            catch(KeyNotFoundException)
            {
                throw new ObjectDisposedException("Attempting to read from a connection that doesn't exist in a lobby host.");
            }
            
            string[] splitLine = line.Split(new char[] { ' ' });
            string identifier = splitLine[0];

            // End the connection
            if (identifier.Equals("terminate"))
            {
                clients.Remove(connection);
                UpdateClientsClose(client);
            }
            // Start the game
            else if (identifier.Equals("start"))
                StartGame();

            // Edit the client data
            else
            {
                StringBuilder resultBuilder = new StringBuilder();
                for (int i = 1; i < splitLine.Length; i++)
                    resultBuilder.Append(splitLine[i]+" ");
                string result = resultBuilder.ToString(); // in case it's more than one word

                if (identifier.Equals("username"))
                    client.username = result;
                else if (identifier.Equals("number"))
                    client.playerNumber = int.Parse(result);
                else if (identifier.Equals("color"))
                    client.color = ColorTranslator.FromHtml(result);
                else if (identifier.Equals("faction"))
                    client.factionName = result;
                UpdateClients(client);
            }
        }

        /// <summary>
        /// Returns an appropriate player number for a new player connecting
        /// to use.
        /// </summary>
        private int GetFreePlayerNumber()
        {
            int num = 0;
            while (num < 100000)
            {
                num++;
                foreach (ClientData c in clients.Values)
                    if (c.playerNumber == num)
                        continue;
                return num;
            }
            throw new InvalidOperationException("Can't get a free player number from the server!");
        }

        /// <summary>
        /// Shuffles player numbers of connected clients down given that the 
        /// client with the given player number just quit.
        /// </summary>
        private void ShufflePlayerNumbersOnQuit(int quitNumber)
        {
            foreach(ClientData c in clients.Values)
                if (c.playerNumber > quitNumber)
                    c.playerNumber--;
        }

        private void StartGame()
        {
            // First, build the STC connections
            Dictionary<STCConnection, ClientData> serverClients = new Dictionary<STCConnection, ClientData>();
            foreach(KeyValuePair<Connection, ClientData> client in clients)
            {
                client.Key.WriteLine("start");
                STCConnection newConnection = new STCConnection(client.Key.GetLocalIP());
            }
            GameBuilder.BuildServer(serverClients);
        }
    }
}
