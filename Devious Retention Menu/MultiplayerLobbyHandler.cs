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
    class MultiplayerLobbyHandler : MenuItemHandler
    {
        private Socket outgoingSocket;
        private StreamWriter outgoingWriter;
        private Socket incomingSocket;
        private StreamReader incomingReader;
        private Thread connectionListenThread;

        private Dictionary<int, LobbyHost.ClientData> clients; // identifier is player numbers

        /// <summary>
        /// Creates the multiplayer lobby handler assuming a host
        /// at the given IP address.
        /// Throws an exception if no host is found there.
        /// </summary>
        public MultiplayerLobbyHandler(IPAddress hostIP)
        {
            clients = new Dictionary<int, LobbyHost.ClientData>();

            Connect(hostIP);
            connectionListenThread = new Thread(new ThreadStart(ListenService));
            connectionListenThread.Start();
        }

        /// <summary>
        /// Attempts to connect to a lobby host at the given
        /// IP address; may through an exception if there is
        /// no valid host there.
        /// </summary>
        private void Connect(IPAddress hostIP)
        {
            outgoingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            outgoingSocket.Connect(hostIP, LobbyHost.HOST_CONNECTION_PORT);
            NetworkStream outgoingStream = new NetworkStream(outgoingSocket);
            outgoingWriter = new StreamWriter(outgoingStream);
            outgoingWriter.AutoFlush = true;
        }

        /// <summary>
        /// Writes the specified line to the host (assumes the host exists and is open).
        /// </summary>
        public void WriteLine(string line)
        {
            outgoingWriter.WriteLine(line);
        }

        /// <summary>
        /// Listens to the incoming reader (assumes it exists and
        /// will be open for the lifetime of this method) and interprets
        /// its information.
        /// </summary>
        private void ListenService()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), LobbyHost.HOST_TALK_PORT);
            listener.Start();
            incomingSocket = listener.AcceptSocket();
            incomingReader = new StreamReader(new NetworkStream(incomingSocket));

            while (true)
            {
                string line = incomingReader.ReadLine();
                if (line != null)
                {
                    string[] splitLine = line.Split(new char[] { ' ' });
                    int playerNumber = int.Parse(splitLine[0]);
                    string username = splitLine[1];
                    Color color = Color.FromName(splitLine[2]);
                    string factionName = splitLine[3];

                    clients[playerNumber] = new LobbyHost.ClientData(username, playerNumber, color, factionName);
                }
            }
        }

        public void Close()
        {
            if(outgoingSocket != null)
                outgoingSocket.Close();
            if (incomingSocket != null)
                incomingSocket.Close();
            if (connectionListenThread != null)
                connectionListenThread.Abort();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Player Number | Username | Color | Faction Name\n");
            foreach (LobbyHost.ClientData client in clients.Values)
            {
                b.Append(client.ToString() + "\n");
            }
            return b.ToString();
        }
    }
}
