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
                            case 0:
                                InformServerBuildingCreation(splitLine);
                                break;
                            case 1:
                                InformServerUnitTraining(splitLine);
                                break;
                            case 2:
                                InformServerTechnologyResearch(splitLine);
                                break;
                            case 3:
                                InformServerUnitMove(splitLine);
                                break;
                            case 4:
                                InformServerEntityDeletion(splitLine);
                                break;
                            case 5:
                                InformServerResourceGather(splitLine);
                                break;
                            case 6:
                                InformServerAttack(splitLine);
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
        /// Tells the server that the player has request that some entities attack an entity.
        /// </summary>
        private void InformServerAttack(string[] splitLine)
        {
            int defenderType = int.Parse(splitLine[1]);
            int defenderId = int.Parse(splitLine[2]);
            List<int> attackerTypes = new List<int>();
            List<int> attackerIds = new List<int>();
            for(int i = 3; i < splitLine.Length; i += 2)
            {
                attackerTypes.Add(int.Parse(splitLine[i]));
                attackerIds.Add(int.Parse(splitLine[i + 1]));
            }

            server.AttackEntity(defenderType, defenderId, attackerTypes, attackerIds);
        }

        /// <summary>
        /// Tells the server that a player has requested that an entity be deleted.
        /// </summary>
        private void InformServerEntityDeletion(string[] splitLine)
        {
            int type = int.Parse(splitLine[1]);
            int id = int.Parse(splitLine[2]);

            server.DeleteEntity(type, id);
        }

        /// <summary>
        /// Tells the server that a player has requested that a unit be trained.
        /// </summary>
        /// <param name="splitLine"></param>
        private void InformServerUnitTraining(string[] splitLine)
        {
            int id = int.Parse(splitLine[1]);
            string unitTypeName = splitLine[2];
            server.CreateUnit(id, unitTypeName);
        }

        /// <summary>
        /// Tells the server that a player has requested that a building be constructed.
        /// </summary>
        private void InformServerBuildingCreation(string[] splitLine)
        {
            string buildingTypeName = splitLine[1];
            double x = double.Parse(splitLine[2]);
            double y = double.Parse(splitLine[3]);
            int playerNumber = int.Parse(splitLine[4]);
            server.CreateBuilding(playerNumber, buildingTypeName, x, y);
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
        /// Tells the server to remove a certain amount from a resource.
        /// </summary>
        private void InformServerResourceGather(string[] splitLine)
        {
            double amount = double.Parse(splitLine[1]);
            int id = int.Parse(splitLine[2]);
            server.GatherResource(amount, id);
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
        /// [message type=0] [isFree] [0=unit,1=building,2=resource] [type name] [id] [xpos] [ypos] [player (if not a resource)] [resource id (if a building, -1 if none or not building)]
        /// </summary>
        public void InformEntityAdd(Entity entity, bool isFree)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;

            int entityType = -1;
            int resourceID = -1;
            string typeName = "";
            if (entity is Unit) { entityType = 0; typeName = ((Unit)entity).unitType.name; }
            else if (entity is Building) { entityType = 1; typeName = ((Building)entity).buildingType.name; resourceID = ((Building)entity).resource == null ? -1 : ((Building)entity).resource.ID; }
            else if (entity is Resource) { entityType = 2; typeName = ((Resource)entity).resourceType.name; }
            outgoingWriter.WriteLine("0 " + isFree + " " + entityType + " " + typeName + " " + entity.ID + " " + entity.X + " " + entity.Y + " " + entity.Player.Number + " " + resourceID);
        }

        /// <summary>
        /// Informs the client that a given entity
        /// has been deleted.
        /// 
        /// Message format:
        /// [message type=1] [0=unit,1=building,2=resource] [id]
        /// </summary>
        public void InformEntityDeletion(int entityType, int entityID)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("1 " + entityType + " " + entityID);
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
        /// 2 = tick battle animation (change: 0 = tick, 1 = reset)
        /// 3 = tick movement animation "
        /// Building:
        /// 0 = hitpoints
        /// 1 = tick battle animation "
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
            outgoingWriter.WriteLine("2 " + entityType + " " + entity.ID + " " + attributeID + " " + attributeChange + " " + attributeChange2);
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

        /// <summary>
        /// Informs the client that an entity has either started (start=true)
        /// or stopped (start=false) attacking another entity.
        /// 
        /// Message format:
        /// [message type=6] [started] [attacker type] [attacker id] [defender type] [defender id]
        /// </summary>
        public void InformEntityAttack(Entity attacker, Entity defender, bool started)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("6 " + started + " " + (attacker is Unit ? 0 : 1) + " " + attacker.ID + " " + (defender is Unit ? 0 : 1) + " " + defender.ID);
        }

        /// <summary>
        /// Informs the client to update to the given map.
        /// </summary>
        public void InformMap(Map map)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;
            outgoingWriter.WriteLine("7 " + map.ToString());
        }
    }
}
