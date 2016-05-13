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
    public class MultiplayerLobbyHandler : MenuItemHandler, IConnectionDataListener
    { 
        private Connection connection;
        private Dictionary<int, LobbyHost.ClientData> clients; // identifier is unique player ID
        
        public bool Connected
        {
            get; private set;
        }

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
            Connected = false;
            clients = new Dictionary<int, LobbyHost.ClientData>();

            // Attempt to connect to the host
            connection = new Connection(hostIP, LobbyHost.HOST_CONNECTION_PORT);
            connection.Connect();

            // Listen to this connection
            connection.AddConnectionDataListener(this);
            connection.BeginListening();
            Connected = true;
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
            }
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

        /// <summary>
        /// Upon a line being sent from the connection, interprets it
        /// and updates this client's knowledge.
        /// </summary>
        public void OnLineRead(Connection connection, string line)
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
            // A client has left or the server has terminated the connection
            else if (splitLine[0].Equals("terminate"))
            {
                if (splitLine.Length == 1)
                {
                    Close();
                }
                else
                {
                    clients.Remove(int.Parse(splitLine[1]));
                }
            }
            else if (splitLine[0].Equals("full"))
            {
                Connected = false;
            }
        }
    }
}
