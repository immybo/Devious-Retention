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
    class LobbyHost : MenuItemHandler
    {
        public const int HOST_CONNECTION_PORT = 4985; // The port which a lobby host will listen for new connections on
        public const int HOST_TALK_PORT = 4986; // The port which a lobby host will send data to all clients on
        public const int HOST_LISTEN_PORT = 4987; // The port which a lobby host will listen for new data from clients on
        
        private Dictionary<Socket, ClientData> clients;
        private Dictionary<Socket, StreamWriter> clientWriters;
        private Thread connectionListenThread;
        private List<Thread> listenThreads;

        public LobbyHost()
        {
            clients = new Dictionary<Socket, ClientData>();
            clientWriters = new Dictionary<Socket, StreamWriter>();
            listenThreads = new List<Thread>();

            connectionListenThread = new Thread(new ThreadStart(ConnectionListenService));
            connectionListenThread.Start();
        }

        /// <summary>
        /// Begins listening for incoming connections
        /// </summary>
        private void ConnectionListenService()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), HOST_CONNECTION_PORT);
            listener.Start();

            while (true)
            {
                Socket incomingSocket = listener.AcceptSocket();
                clientWriters.Add(incomingSocket, new StreamWriter(new NetworkStream(incomingSocket)));
                clientWriters[incomingSocket].AutoFlush = true;

                ClientData data = new ClientData("default", 0, Color.Black, "");
                clients.Add(incomingSocket, data);
                listenThreads.Add(new Thread(() => ListenService(incomingSocket, data)));
            }
        }

        /// <summary>
        /// Begins listening for incoming data and writing it to the specified client
        /// </summary>
        private void ListenService(Socket socket, ClientData client)
        {
            NetworkStream s = new NetworkStream(socket);
            StreamReader reader = new StreamReader(s);

            while (true)
            {
                // When we read anything, figure out what it is and adjust the appropriate piece of data
                string line = reader.ReadLine();
                if(line != null)
                {
                    string[] splitLine = line.Split(new char[] { ' ' });
                    string identifier = splitLine[0];

                    // End the connection and thread
                    if (identifier.Equals("terminate"))
                    {
                        clients.Remove(socket);
                        clientWriters.Remove(socket);
                        socket.Close();
                        Thread.CurrentThread.Abort();
                    }

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

        /// <summary>
        /// Refreshes each client's knowledge of the specified client data.
        /// </summary>
        private void UpdateClients(ClientData client)
        {
            foreach(StreamWriter s in clientWriters.Values)
            {
                s.WriteLine(client.ToString());
            }
        }

        /// <summary>
        /// Ends this lobby host, terminating all connections and threads.
        /// </summary>
        public void Close()
        {
            if (connectionListenThread != null)
                connectionListenThread.Abort();
            foreach (Socket s in clients.Keys)
                s.Close();
            foreach (Thread t in listenThreads)
                t.Abort();
        }

        /// <summary>
        /// Provides information about one client.
        /// </summary>
        public class ClientData
        {
            public string username;
            public int playerNumber;
            public Color color;
            
            public string factionName; // Kept as a primitive type and synced up on launch

            public ClientData(string username, int playerNumber, Color color, string factionName)
            {
                this.username = username;
                this.playerNumber = playerNumber;
                this.color = color;
                this.factionName = factionName;
            }

            public override string ToString()
            {
                return playerNumber + " " + username + " " + color + " " + factionName;
            }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Player Number | Username | Color | Faction Name\n");
            foreach (ClientData client in clients.Values)
            {
                b.Append(client.ToString() + "\n");
            }
            return b.ToString();
        }
    }
}
