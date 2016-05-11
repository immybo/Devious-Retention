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

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A lobby host communicates with all lobby clients and allows for
    /// new clients to connect through it.
    /// </summary>
    public class LobbyHost : MenuItemHandler, IReceiverFunction
    {
        public const int HOST_CONNECTION_PORT = 4985; // The port which a lobby host will listen for new connections on

        private int currentUniqueID;
        private ConcurrentDictionary<Connection, ClientData> clients;
        private Dictionary<Connection, ClientData> toAddClients; // since we add clients on another thread, we have to make sure that the listening thread adds them at an okay time
        private bool toAddClientsBeingModified;

        private bool listenThreadToAbort;

        private Thread listenThread;

        private ConnectionListener connectionListener;

        private int maxPlayers;

        public LobbyHost(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            currentUniqueID = 0;

            clients = new Dictionary<Connection, ClientData>();
            toAddClients = new Dictionary<Connection, ClientData>();
            toAddClientsBeingModified = false;

            listenThreadToAbort = false;
            
            listenThread = new Thread(new ThreadStart(ListenService));
            listenThread.Start();

            // Build the connection listener
            connectionListener = new ConnectionListener(HOST_CONNECTION_PORT);
            connectionListener.AddReceiverFunction(this);
            connectionListener.BeginListening();
        }

        /// <summary>
        /// Continuously listens to all clients and takes appropriate actions.
        /// </summary>
        private void ListenService()
        {
            while (true)
            {
                List<Connection> toRemoveClients = new List<Connection>();

                foreach (KeyValuePair<Connection, ClientData> entry in clients)
                {
                    Connection connection = entry.Key;

                    if (!connection.IsOpen())
                    {
                        continue;
                    }

                    ClientData client = entry.Value;

                    // When we read anything, figure out what it is and adjust the appropriate piece of data
                    string line = connection.ReadLine();
                    if (line != null)
                    {
                        string[] splitLine = line.Split(new char[] { ' ' });
                        string identifier = splitLine[0];

                        // End the connection
                        if (identifier.Equals("terminate"))
                        {
                            toRemoveClients.Add(connection);
                            UpdateClientsClose(client);
                        }

                        else {
                            string result = splitLine[1];

                            if (identifier.Equals("username"))
                                client.username = result;
                            else if (identifier.Equals("number"))
                                client.playerNumber = int.Parse(result);
                            else if (identifier.Equals("color"))
                                client.color = Color.FromName(result);
                            else if (identifier.Equals("faction"))
                                client.factionName = result;
                            UpdateClients(client);
                        }
                    }
                }

                foreach (Connection c in toRemoveClients)
                {
                    clients.Remove(c);
                    c.Close();
                }

                while (toAddClientsBeingModified) Thread.Sleep(10);
                toAddClientsBeingModified = true;
                foreach (KeyValuePair<Connection, ClientData> c in toAddClients)
                    clients.Add(c.Key, c.Value);
                toAddClients.Clear();
                toAddClientsBeingModified = false;

                if (listenThreadToAbort) Thread.CurrentThread.Abort();
            }
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
        /// Ends this lobby host, terminating all connections and threads.
        /// </summary>
        public void Close()
        {
            if (connectionListener != null)
                connectionListener.StopListening();
            if (listenThread != null) { 

                listenThreadToAbort = true;
            while (listenThreadToAbort) Thread.Sleep(10); }
            foreach (Connection c in clients.Keys)
                c.Close();
        }

        /// <summary>
        /// Provides information about one client.
        /// </summary>
        public class ClientData
        {
            public int uniqueID;
            public string username;
            public int playerNumber;
            public Color color;
            
            public string factionName; // Kept as a primitive type and synced up on launch

            public ClientData(int uniqueID, string username, int playerNumber, Color color, string factionName)
            {
                this.username = username;
                this.playerNumber = playerNumber;
                this.color = color;
                this.factionName = factionName;
            }

            public override string ToString()
            {
                return uniqueID + " " + playerNumber + " " + username + " " + color.Name + " " + factionName;
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
        /// </summary>
        public void OnConnection(Connection newClient)
        {
            while (toAddClientsBeingModified) Thread.Sleep(10);
            toAddClientsBeingModified = true;
            toAddClients.Add(newClient, new ClientData(currentUniqueID, "", 0, Color.Black, ""));
            currentUniqueID++;
            toAddClientsBeingModified = false;
        }
    }
}
