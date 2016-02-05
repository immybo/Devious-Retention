using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A Server-to-Client Connection sends information from the server over 
    /// the socket connection, and receives information from the client, passing
    /// that on to its server.
    /// </summary>
    class STCConnection
    {
        private IPAddress ip;
        private Socket outgoingSocket;
        private NetworkStream outgoingStream;
        private StreamWriter outgoingWriter;

        private Socket incomingSocket;
        private GameServer server;

        public STCConnection(IPAddress ip)
        {
            this.ip = ip;
        }
        public void SetServer(GameServer server)
        {
            this.server = server;
            Listen();
        }

        /// <summary>
        /// Starts listening on the appropriate port for an incoming
        /// socket connection, and sets incomingSocket to the connection
        /// when made.
        /// </summary>
        public void Listen()
        {
            Thread t = new Thread(new ThreadStart(ListenService));
            t.Start();
        }
        /// <summary>
        /// After recieving the socket connection, listens to messages
        /// from it and responds appropriately.
        /// </summary>
        private void ListenService()
        {
            TcpListener listener = new TcpListener(ip, GameInfo.CTS_PORT);
            listener.Start();
            incomingSocket = listener.AcceptSocket();
            Console.WriteLine("STC incoming socket opened.");

            try
            {
                NetworkStream s = new NetworkStream(incomingSocket);
                StreamReader reader = new StreamReader(s);

                while (true)
                {
                    // If anything was read,
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] splitLine = line.Split(new Char[] { ' ' });
                        // Check what type of message it was
                        int messageType = int.Parse(splitLine[0]);

                        // And process that message appropriately
                        switch (messageType)
                        {
                            case 2:
                                InformServerTechnologyResearch(splitLine);
                                break;
                            case 3:
                                InformServerUnitMove(splitLine);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Tells the server that a player has researched a technology.
        /// </summary>
        private void InformServerTechnologyResearch(string[] splitLine)
        {
            string techName = splitLine[1];
            int player = int.Parse(splitLine[2]);
            server.ResearchTechnology(player, techName);
        }

        /// <summary>
        /// Tells the server to move a unit.
        /// </summary>
        private void InformServerUnitMove(string[] splitLine)
        {
            int id = int.Parse(splitLine[1]);
            double x = double.Parse(splitLine[2]);
            double y = double.Parse(splitLine[3]);
            server.CommandUnitToMove(id, x, y);
        }

        /// <summary>
        /// Attempts to create a socket connection to the given IP.
        /// Returns whether or not it succeeded in doing so.
        /// Closes any existing connection.
        /// </summary>
        public bool Connect()
        {
            if (outgoingSocket != null)
                outgoingSocket.Close();

            try
            {
                outgoingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                outgoingSocket.Connect(ip, GameInfo.STC_PORT);
                outgoingStream = new NetworkStream(outgoingSocket);
                outgoingWriter = new StreamWriter(outgoingStream);
                outgoingWriter.AutoFlush = true;
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Couldn't open STCConnection socket. " + e);
                return false;
            }
        }

        /// <summary>
        /// Closes the socket connection.
        /// </summary>
        public void Disconnect()
        {
            if (outgoingSocket != null)
                outgoingSocket.Close();
        }

        /// <summary>
        /// Informs the client that a new entity has been added.
        /// 
        /// Message format:
        /// [message type=0] [isFree] [0=unit,1=building,2=resource] [type name] [id] [xpos] [ypos] [player (if not a resource)]
        /// </summary>
        public void InformEntityAdd(Entity entity, bool isFree)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;

            int entityType = -1;
            string typeName = "";
            if (entity is Unit) { entityType = 0; typeName = ((Unit)entity).unitType.name; }
            else if (entity is Building) { entityType = 1; typeName = ((Building)entity).buildingType.name; }
            else if (entity is Resource) { entityType = 2; typeName = ((Resource)entity).resourceType.name; }
            outgoingWriter.WriteLine("0 " + isFree + " " + entityType + " " + typeName + " " + entity.id + " " + entity.x + " " + entity.y + " " + entity.playerNumber);
        }

        /// <summary>
        /// Informs the client that a given entity
        /// has been deleted.
        /// 
        /// Message format:
        /// [message type=1] [0=unit,1=building,2=resource] [id]
        /// </summary>
        public void InformEntityDeletion(Entity entity)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;

            int entityType = -1;
            if (entity is Unit) entityType = 0;
            else if (entity is Building) entityType = 1;
            else if (entity is Resource) entityType = 2;
            outgoingWriter.WriteLine("1 " + entityType + " " + entity.id);
        }

        /// <summary>
        /// Informs the client that one of an entity's attributes has changed.
        /// 
        /// Message format:
        /// [message type=2] [0=unit,1=building,2=resource] [id] [attribute] [attribute change]
        /// 
        /// Attribute IDs:
        /// Unit:
        /// 0 = hitpoints
        /// 1 = x and y position (2 attribute changes)
        /// 2 = play battle animation (1 = start/restart, 0 = stop)
        /// 3 = play movement animation (1 = start/restart, 0 = stop)
        /// Building:
        /// 0 = hitpoints
        /// 1 = built (only 1 is accepted)
        /// Resource:
        /// 0 = amount remaining
        /// </summary>
        public void InformEntityChange(Entity entity, int attributeID, double attributeChange, double attributeChange2)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            int entityType = -1;
            if (entity is Unit) entityType = 0;
            else if (entity is Building) entityType = 1;
            else if (entity is Resource) entityType = 2;
            outgoingWriter.WriteLine("2 " + entityType + " " + entity.id + " " + attributeID + " " + attributeChange + " " + attributeChange2);
        }

        /// <summary>
        /// Informs the client that the given player has researched
        /// the given technology.
        /// 
        /// Message format:
        /// [message type=3] [player number] [technology name]
        /// </summary>
        public void InformTechnologyResearch(int player, Technology technology)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("3 " + player + " " + technology.name);
        }

        /// <summary>
        /// Informs the client that the game has ended.
        /// 
        /// Message format:
        /// [message type=4] [won]
        /// </summary>
        /// <param name="won">Whether or not this player won the game.</param>
        public void InformGameOver(Boolean won)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("4 " + won);
        }

        /// <summary>
        /// Informs the client to update the game.
        /// 
        /// Message format:
        /// [message type=5]
        /// </summary>
        public void Tick()
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("5");
        }
    }
}
