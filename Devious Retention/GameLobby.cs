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

namespace Devious_Retention
{
    class GameLobby
    {
        public IPAddress ipAddress { get; private set; } // The IP address of this game client
        public IPAddress hostIPAddress { get; private set; } // The IP address of the host that this client is connecting to
        public bool connected { get; private set; }

        private Socket socketConnection;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        private Thread listenThread;

        public bool terminated { get; private set; } // Whether or not this client is waiting to be disposed of

        // Known information about the game lobby
        protected List<LobbyPlayer> players;

        public GameLobby(IPAddress ipAddress, IPAddress hostIPAddress)
        {
            this.ipAddress = ipAddress;
            this.hostIPAddress = hostIPAddress;
            terminated = false;

            players = new List<LobbyPlayer>();
        }

        /// <summary>
        /// Returns the number of players currently in this game lobby.
        /// </summary>
        public int NumPlayers()
        {
            return players.Count;
        }

        /// <summary>
        /// Returns all players currently in the game lobby.
        /// </summary>
        public List<LobbyPlayer> GetPlayers()
        {
            return players.ToList();
        }

        /// <summary>
        /// Renders this game lobby within the specified bounds.
        /// Specifically, displays information for all players 
        /// </summary>
        /// <param name="g">The graphics object on which to render</param>
        /// <param name="bounds">The bounds within which to render; it will occupy almost all of the area</param>
        public void Render(Graphics g, Rectangle bounds)
        {
            int i = 1;
            int playerHeight = bounds.Height / NumPlayers();
            foreach (LobbyPlayer player in GetPlayers())
            {
                g.DrawString(i + "", GameMenu.MENU_FONT, GameMenu.PLAYER_BRUSH, 20, 20 + i * playerHeight);
                i++;
            }

            if (this is HostGameLobby)
                g.DrawString("Hosting a game.", GameMenu.MENU_FONT, Brushes.Black, 250, 250);
            else
                g.DrawString("Client in a lobby.", GameMenu.MENU_FONT, Brushes.Black, 250, 250);

            StringBuilder builder = new StringBuilder();
            foreach (LobbyPlayer player in players)
            {
                builder.Append(player.name + "\n");
            }
        }

        /// <summary>
        /// Attempts to establish a socket connection to the lobby host.
        /// </summary>
        public bool Connect()
        {
            if (socketConnection != null) socketConnection.Close();
            connected = false;

            try
            {
                socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketConnection.Connect(hostIPAddress, GameInfo.LOBBY_PORT);
                stream = new NetworkStream(socketConnection);
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                reader = new StreamReader(stream);

                listenThread = new Thread(new ThreadStart(ListenService)); // Start listening to the host
                listenThread.Start();

                connected = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect: " + e);
                return false;
            }
        }

        /// <summary>
        /// Listens to the reader from the host.
        /// </summary>
        private void ListenService()
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    string[] splitLine = line.Split(new char[] { ' ' });
                    int messageType = int.Parse(splitLine[0]);

                    switch (messageType)
                    {
                        case 0:
                            Terminate();
                            break;
                        case 1:
                            SetClients(splitLine[1], splitLine[2], splitLine[3]);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets this client's knowledge of the client information
        /// to the given information.
        /// </summary>
        protected void SetClients(string listOfIPs, string listOfFactions, string listOfNames)
        {
            // Convert the IP addresses
            string[] ipStrings = listOfIPs.Split(new char[] { ',' });
            IPAddress[] ips = new IPAddress[ipStrings.Length];
            for (int i = 0; i < ipStrings.Length; i++)
            {
                ips[i] = IPAddress.Parse(ipStrings[i]);
            }

            string[] factions = listOfFactions.Split(new char[] { ',' });
            string[] names = listOfNames.Split(new char[] { ',' });

            // Create a whole new set of players; refresh the data
            players = new List<LobbyPlayer>();
            for(int i = 0; i < ipStrings.Length; i++)
            {
                players.Add(new LobbyPlayer(ips[i], names[i], factions[i]));
            }
        }

        /// <summary>
        /// Terminates this ClientGameLobby.
        /// </summary>
        public void Terminate()
        {
            if (listenThread != null)
                listenThread.Abort();
            if (writer != null)
            {
                writer.WriteLine("0");
                writer.Close();
            }
            if (reader != null)
                reader.Close();
            if (stream != null)
                stream.Close();

            terminated = true;
        }

        /// <summary>
        /// Tells the lobby host to change this client's name.
        /// </summary>
        /// <param name="newName"></param>
        public void ChangeName(string newName)
        {
            if (writer != null)
            {
                writer.WriteLine("2 " + newName);
            }
            else
            {
                throw new Exception("Couldn't change name because writer is null.");
            }
        }

        /// <summary>
        /// Tells the lobby host to change this client's faction.
        /// </summary>
        public void ChangeFaction(string newFaction)
        {
            if (writer != null)
            {
                writer.WriteLine("1 " + newFaction);
            }
            else
            {
                throw new Exception("Couldn't change faction because writer is null.");
            }
        }

        /// <summary>
        /// Represents all the information needed to draw a game player in the lobby
        /// </summary>
        public class LobbyPlayer
        {
            public IPAddress ip;
            public string name;
            public string faction;

            public LobbyPlayer(IPAddress ip, string name, string faction)
            {
                this.ip = ip;
                this.name = name;
                this.faction = faction;
            }
        }
    }

    /// <summary>
    /// A host of the game lobby; handles communication to all clients.
    /// </summary>
    class HostGameLobby : GameLobby
    {
        private List<Connection> playerConnections; // Connection information about players in the lobby

        private Thread listenThread;

        private Socket incomingSocket;

        public int numComputers { get; set; } = 0; // How many computers will be added as the game starts
        public int computerDifficulty { get; set; } = 50; // The difficulty level of the computers

        public HostGameLobby(IPAddress ipAddress, IPAddress hostAddress) : base(ipAddress, hostAddress)
        {
            playerConnections = new List<Connection>();

            ListenForNewConnections();
        }

        /// <summary>
        /// Begins listening for new client connections, if it wasn't already
        /// </summary>
        private void ListenForNewConnections()
        {
            listenThread = new Thread(new ThreadStart(ListenForNewConnectionsService));
            listenThread.Start();
        }

        /// <summary>
        /// Constantly listens on the port for new client connections
        /// </summary>
        private void ListenForNewConnectionsService()
        {
            while (true)
            {
                if (incomingSocket != null) incomingSocket.Disconnect(true);

                TcpListener listener = new TcpListener(hostIPAddress, GameInfo.LOBBY_PORT);
                listener.Start();
                incomingSocket = listener.AcceptSocket();
                listener.Stop();
                IPAddress incomingSocketAddress = (incomingSocket.RemoteEndPoint as IPEndPoint).Address;

                try
                {
                    NetworkStream stream = new NetworkStream(incomingSocket);
                    StreamWriter writer = new StreamWriter(stream);
                    StreamReader reader = new StreamReader(stream);

                    players.Add(new LobbyPlayer(incomingSocketAddress, "", ""));
                    playerConnections.Add(new Connection(incomingSocketAddress, stream, writer, reader, incomingSocket));
                    InformClients();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Stops listening for client connections, if it was before
        /// </summary>
        private void StopListening()
        {
            if(listenThread != null && listenThread.IsAlive)
                listenThread.Abort();
        }

        /// <summary>
        /// Listens to all current connections.s
        /// </summary>
        private void ListenService()
        {
            while (true)
            {
                // Cycle through all connections and attempt to read from them
                foreach (Connection player in playerConnections)
                {
                    string line = player.reader.ReadLine();
                    if (line != null)
                    {
                        // If they've sent something, check what the message is and process it
                        string[] splitLine = line.Split(new char[] { ' ' });
                        int messageType = int.Parse(splitLine[0]);

                        switch (messageType)
                        {
                            // The client has left
                            case 0:
                                ProcessClientLeave(player.ip);
                                break;
                            // The client has set their faction
                            case 1:
                                ProcessClientSetFaction(player.ip, splitLine[1]);
                                break;
                            // The client has set their name
                            case 2:
                                ProcessClientSetName(player.ip, splitLine[1]);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes a client with the given IP address leaving the game lobby.
        /// </summary>
        private void ProcessClientLeave(IPAddress clientIP)
        {
            foreach(LobbyPlayer player in players)
            {
                if (player.ip.Equals(clientIP))
                {
                    players.Remove(player);
                    InformClients();
                    break;
                }
            }
            foreach(Connection c in playerConnections)
            {
                if (c.ip.Equals(clientIP))
                {
                    playerConnections.Remove(c);
                    break;
                }
            }
        }

        /// <summary>
        /// Processes a client with the given IP address switching to the given faction.
        /// </summary>
        private void ProcessClientSetFaction(IPAddress clientIP, string factionName)
        {
            LobbyPlayer playerToChange = null;
            foreach(LobbyPlayer player in players)
            {
                if (player.ip.Equals(clientIP))
                {
                    playerToChange = player;
                    break;
                }
            }

            playerToChange.faction = factionName;

            InformClients();
        } 

        /// <summary>
        /// Processes a client with the given IP address switching to have the given new name.
        /// </summary>
        private void ProcessClientSetName(IPAddress clientIP, string newName)
        {
            LobbyPlayer playerToChange = null;
            foreach(LobbyPlayer player in players)
            {
                if (player.ip.Equals(clientIP)){
                    playerToChange = player;
                    break;
                }
            }

            playerToChange.name = newName;
            InformClients();
        }

        /// <summary>
        /// Informs clients of an update to player details
        /// and sends the new values to them.
        /// Also informs this HostGameLobby.
        /// </summary>
        private void InformClients()
        {
            // Build the message to send with all of the relevant attributes
            StringBuilder builder = new StringBuilder();
            foreach(LobbyPlayer player in players)
            {
                builder.Append(player.ToString() + " ");
            }

            // And send it to all players
            foreach(Connection connection in playerConnections)
            {
                connection.writer.WriteLine(builder.ToString());
            }
        }

        /// <summary>
        /// Removes the player with the given IP address from the lobby.
        /// Returns whether or not it succeeded in kicking the player.
        /// </summary>
        public bool KickPlayer(IPAddress clientIP)
        {
            LobbyPlayer playerToRemove = null;
            foreach(LobbyPlayer player in players)
            {
                if (player.ip.Equals(clientIP))
                {
                    playerToRemove = player;
                    players.Remove(player);
                    InformClients();
                }
            }
            foreach(Connection c in playerConnections)
            {
                if (c.ip.Equals(clientIP))
                {
                    c.writer.WriteLine("0");
                    c.socket.Close();
                }
            }

            if (playerToRemove != null) return true;
            return false;
        }

        /// <summary>
        /// Terminates this GameLobbyHost instance, stopping all connections.
        /// </summary>
        public new void Terminate()
        {
            // Terminate the connections with all clients
            foreach(Connection player in playerConnections)
            {
                player.writer.WriteLine("0");
                player.stream.Close();
            }
            if (listenThread != null)
                listenThread.Abort();

            base.Terminate();
        }

        /// <summary>
        /// Represents a connection to a player in the lobby
        /// </summary>
        class Connection
        {
            public IPAddress ip;
            public NetworkStream stream;
            public StreamWriter writer;
            public StreamReader reader;
            public Socket socket;

            public Connection(IPAddress ip, NetworkStream stream, StreamWriter writer, StreamReader reader, Socket socket)
            {
                this.ip = ip;
                this.stream = stream;
                this.writer = writer;
                this.reader = reader;
                this.socket = socket;
            }
        }
    }
}
