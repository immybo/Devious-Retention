using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A multiplayer lobby allows a player to connect with other players
    /// before starting a game.
    /// </summary>
    public class MultiplayerLobbyHandler : MenuItemHandler
    { 
        private Connection connection;
        private Thread connectionListenThread;

        private Dictionary<int, LobbyHost.ClientData> clients; // identifier is unique player ID
        public int PlayerID
        {
            get; private set;
        }

        /// <summary>
        /// Creates the multiplayer lobby handler assuming a host
        /// at the given IP address.
        /// Throws an exception if no host is found there.
        /// </summary>
        public MultiplayerLobbyHandler(IPAddress hostIP)
        {
            clients = new Dictionary<int, LobbyHost.ClientData>();

            // 1. Attempt to connect to the host
            connection = new Connection(hostIP, LobbyHost.HOST_CONNECTION_PORT);
            connection.Connect();

            // 2. Get the port to connect properly on
            string port;
            while((port = connection.ReadLine()) == null)
                Thread.Sleep(10);
            connection.Close();

            // 3. Connect over the new port OR fail if there was no available room
            if (port.Equals("full"))
                throw new InvalidOperationException("Lobby was full.");

            connection = new Connection(hostIP, int.Parse(port));
            connection.Connect();

            // 4. Listen to this connection
            connectionListenThread = new Thread(new ThreadStart(ListenService));
            connectionListenThread.Start();
        }

        /// <summary>
        /// Continuously listens for data from the connection,
        /// and performs the appropriate actions with that data if necessary.
        /// Assumes that the connection exists.
        /// </summary>
        private void ListenService()
        {
            while (true)
            {
                string line = connection.ReadLine();
                if (line != null)
                {
                    string[] splitLine = line.Split(new char[] { ' ' });

                    // Update information about one client
                    if (splitLine[0].Equals("update"))
                    {
                        int uniqueID = int.Parse(splitLine[1]);
                        int playerNumber = int.Parse(splitLine[2]);
                        string username = splitLine[3];
                        Color color = Color.FromName(splitLine[4]);
                        string factionName = splitLine[5];

                        clients[playerNumber] = new LobbyHost.ClientData(uniqueID, username, playerNumber, color, factionName);
                    }
                    // Inform this client's ID
                    else if (splitLine[0].Equals("inform"))
                    {
                        PlayerID = int.Parse(splitLine[1]);
                    }
                    // A client has left
                    else if (splitLine[0].Equals("terminate"))
                    {
                        clients.Remove(int.Parse(splitLine[1]));
                    }
                }
            }
        }

        public void UpdateUsername(string newUsername)
        {
            connection.WriteLine("username " + newUsername);
        }
        public void UpdatePlayerNumber(int newPlayerNumber)
        {
            connection.WriteLine("number " + newPlayerNumber);
        }
        public void UpdateColor(string newColorName)
        {
            connection.WriteLine("color " + newColorName);
        }
        public void UpdateFactionName(string newFactionName)
        {
            connection.WriteLine("faction " + newFactionName);
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.WriteLine("terminate");
                connection.Close();
            }
            if (connectionListenThread != null)
                connectionListenThread.Abort();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (LobbyHost.ClientData client in clients.Values)
            {
                b.Append(client.ToString() + "\n");
            }
            return b.ToString();
        }
    }
}
