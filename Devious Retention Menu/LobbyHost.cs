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

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A lobby host communicates with all lobby clients and allows for
    /// new clients to connect through it.
    /// </summary>
    public class LobbyHost : MenuItemHandler
    {
        public const int HOST_CONNECTION_PORT = 4985; // The port which a lobby host will listen for new connections on
        public const int MIN_CLIENT_PORT = 4986; // The port for the first player to connect; ports between this and this+maxPlayers will be used

        private int currentUniqueID; // Could well introduce a race condition... I'm not really sure what to do about it, though
        private Dictionary<Connection, ClientData> clients;

        private Thread listenThread;
        private Thread connectionListenThread;

        private int maxPlayers;

        public LobbyHost(int maxPlayers)
        {
            this.maxPlayers = maxPlayers;
            currentUniqueID = 0;

            clients = new Dictionary<Connection, ClientData>();

            connectionListenThread = new Thread(new ThreadStart(ConnectionListenService));
            connectionListenThread.Start();

            listenThread = new Thread(new ThreadStart(ListenService));
            listenThread.Start();
        }

        /// <summary>
        /// Returns how many player slots are currently free in the lobby.
        /// </summary>
        public int GetRemainingSlots()
        {
            return maxPlayers - clients.Count;
        }

        /// <summary>
        /// Returns the first available port for this host within the range of available
        /// ports. Throws an exception if there are no ports available.
        /// </summary>
        private int GetAvailablePort()
        {
            HashSet<int> usedPorts = new HashSet<int>();
            foreach (Connection c in clients.Keys)
                usedPorts.Add(c.GetPort());

            for (int i = 0; i < maxPlayers; i++)
                if (!usedPorts.Contains(MIN_CLIENT_PORT + i))
                    return MIN_CLIENT_PORT + i;

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Begins listening for incoming connections
        /// </summary>
        private void ConnectionListenService()
        {
            while (true)
            {
                // If we've got the maximum amount of players now, pause until we don't
                while (clients.Count == maxPlayers)
                {
                    // Another way to do this would be a flag and resuming the thread, but this would
                    // add a bit of complexity and the underlying system is probably smart enough to
                    // allocate these resources somewhere else.
                    Thread.Sleep(50);
                }

                // Acknowledge them and send a port to connect to
                Connection listener = new Connection(IPAddress.Parse("127.0.0.1"), HOST_CONNECTION_PORT);
                listener.ListenForConnection();
                
                try {
                    int port = GetAvailablePort();

                    listener.WriteLine(port + "");
                    new Thread(new ThreadStart(() => ListenForPlayer(port))).Start();
                }
                catch (InvalidOperationException) // There were no ports available
                {
                    listener.WriteLine("full");
                }
                finally
                {
                    listener.Close();
                }
            }
        }

        /// <summary>
        /// Waits for a connection from the given port. When it is created,
        /// adds it as a player.
        /// </summary>
        private void ListenForPlayer(int port)
        {
            // Establish the connection
            Connection connection = new Connection(IPAddress.Parse("127.0.0.1"), port);
            connection.ListenForConnection();

            // Update this after connecting, and tell the client its unique ID
            clients.Add(connection, new ClientData(currentUniqueID, "", 0, Color.Black, ""));
            connection.WriteLine("inform " + currentUniqueID);

            currentUniqueID++;

            UpdateClientsAll();
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

                            connection.Close();

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
                    clients.Remove(c);
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
            if (connectionListenThread != null)
                connectionListenThread.Abort();
            if (listenThread != null)
                listenThread.Abort();
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
    }
}
