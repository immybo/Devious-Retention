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
using Devious_Retention;
using static Devious_Retention.GameBuilder;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A multiplayer lobby allows a player to connect with other players
    /// before starting a game.
    /// </summary>
    public class MultiplayerLobbyHandler : MenuItemHandler, IConnectionDataListener, IPlayerChangeListener
    { 
        private Connection connection;
        private Dictionary<int, ClientData> clients; // identifier is unique player ID
        private MultiplayerLobby gui;
        
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
            GameInfo.ReadDefinitions();

            Connected = false;
            clients = new Dictionary<int, ClientData>();

            // Attempt to connect to the host
            connection = new Connection(hostIP, LobbyHost.HOST_CONNECTION_PORT);
            connection.Connect(1000);

            // Listen to this connection
            connection.AddConnectionDataListener(this);
            connection.BeginListening();
            Connected = true;

            // Give the connection initial values for this client
            UpdateClientUsername("Default");
            UpdateClientColor("#000000");
            UpdateClientFactionName("Default");
        }

        public void BeginGUI(bool isHost, LobbyHost host)
        {
            if (isHost)
                gui = new MultiplayerLobby(host, this);
            else
                gui = new MultiplayerLobby(this);
            gui.SetPlayers(clients.Values, PlayerID);
            gui.Visible = true;
            gui.Refresh();
        }

        public void UpdateClientUsername(string newUsername)
        {
            connection.WriteLine("username " + newUsername);
        }
        public void UpdateClientPlayerNumber(int newPlayerNumber)
        {
            connection.WriteLine("number " + newPlayerNumber);
        }
        public void UpdateClientColor(string newColorHex)
        {
            connection.WriteLine("color " + newColorHex);
        }
        public void UpdateClientFactionName(string newFactionName)
        {
            connection.WriteLine("faction " + newFactionName);
        }
        public void StartGame()
        {
            connection.WriteLine("start");
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.WriteLine("terminate");
            }
            if(gui != null)
            {
                gui.Close();
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
        /// Upon a line being sent from the connection, interprets it
        /// and updates this client's knowledge.
        /// </summary>
        public void OnLineRead(Connection connection, string line)
        {
            string[] splitLine = line.Split(new char[] { ' ' });

            // Update information about one client
            if (splitLine[0].Equals("update"))
            {
                StringBuilder b = new StringBuilder();
                for (int i = 1; i < splitLine.Length; i++)
                    b.Append(splitLine[i]);
                ClientData newData = ClientData.FromString(b.ToString());
                if (clients.ContainsKey(newData.uniqueID))
                    clients.Remove(newData.uniqueID);
                clients[newData.uniqueID] = newData;
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
            else if (splitLine[0].Equals("start"))
            {
                OpenGameClient();
            }

            gui.SetPlayers(clients.Values, PlayerID);
            gui.Refresh();
        }

        private void OpenGameClient()
        {
            CTSConnection c = new CTSConnection(connection.GetRemoteIP());
            connection.Close();
            GameBuilder.BuildClient(c, clients.Values.ToList(), clients[PlayerID].playerNumber);
            Close();
        }
    }
}