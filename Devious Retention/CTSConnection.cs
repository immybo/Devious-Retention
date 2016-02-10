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
    /// A Client-to-Server Connection sends information from the client over 
    /// the socket connection, and receives information from the server, passing
    /// that on to its client.
    /// </summary>
    public class CTSConnection
    {
        private IPAddress ip;
        private Socket outgoingSocket;
        private NetworkStream outgoingStream;
        private StreamWriter outgoingWriter;

        private Socket incomingSocket;
        private GameClient client;

        public CTSConnection(IPAddress ip)
        {
            this.ip = ip;
        }

        public void SetClient(GameClient client)
        {
            this.client = client;
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
            TcpListener listener = new TcpListener(ip, GameInfo.STC_PORT);
            listener.Start();
            incomingSocket = listener.AcceptSocket();
            Console.WriteLine("CTS incoming socket opened.");
            
            try
            {
                NetworkStream s = new NetworkStream(incomingSocket);
                StreamReader reader = new StreamReader(s);

                while (true)
                {
                    // If anything was read,
                    string line = reader.ReadLine();
                    if(line != null)
                    {
                        string[] splitLine = line.Split(new Char[] { ' ' });
                        // Check what type of message it was
                        int messageType = int.Parse(splitLine[0]);

                        // And process that message appropriately
                        switch (messageType)
                        {
                            case 0:
                                AddEntity(splitLine);
                                break;
                            case 1:
                                DeleteEntity(splitLine);
                                break;
                            case 2:
                                ChangeEntity(splitLine);
                                break;
                            case 3:
                                ResearchTechnology(splitLine);
                                break;
                            case 4:
                                GameOver(splitLine);
                                break;
                            case 5:
                                client.Tick();
                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Attempts to create a socket connection to the given IP.
        /// Returns whether or not it succeeded in doing so.
        /// </summary>
        public bool Connect()
        {
            if (outgoingSocket != null)
                outgoingSocket.Close();

            try
            {
                outgoingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                outgoingSocket.Connect(ip, GameInfo.CTS_PORT);
                outgoingStream = new NetworkStream(outgoingSocket);
                outgoingWriter = new StreamWriter(outgoingStream);
                outgoingWriter.AutoFlush = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't open CTSConnection socket. " + e);
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
        /// Interprets and passes on a message from the server to
        /// the client to add a certain type of entity.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void AddEntity(string[] splitLine)
        {
            bool isFree = bool.Parse(splitLine[1]);
            int entityType = int.Parse(splitLine[2]);
            string typeName = splitLine[3];
            int id = int.Parse(splitLine[4]);
            double xPos = double.Parse(splitLine[5]);
            double yPos = double.Parse(splitLine[6]);
            int playerNumber = int.Parse(splitLine[2]) == 2 ? 0 : int.Parse(splitLine[7]); // use a player number of 0 if it's a resource
            int resource = int.Parse(splitLine[8]);
            client.AddEntity(isFree, entityType, id, typeName, xPos, yPos, playerNumber, resource);
        }

        /// <summary>
        /// Interprets and passes on a message from the server to
        /// the client to delete a certain entity.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void DeleteEntity(string[] splitLine)
        {
            int entityType = int.Parse(splitLine[1]);
            int entityID = int.Parse(splitLine[2]);
            client.DeleteEntity(entityType, entityID);
        }

        /// <summary>
        /// Interprets and passes on a message from the server to
        /// the client to change an attribute of a certain entity.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void ChangeEntity(string[] splitLine)
        {
            int entityType = int.Parse(splitLine[1]);
            int entityID = int.Parse(splitLine[2]);
            int attribute = int.Parse(splitLine[3]);
            double attributeChange = double.Parse(splitLine[4]);
            double attributeChange2 = double.Parse(splitLine[5]);
            client.ChangeEntityProperty(entityType, entityID, attribute, attributeChange, attributeChange2);
        }

        /// <summary>
        /// Interprets and passes on a message from the server to
        /// the client to research a technology.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void ResearchTechnology(string[] splitLine)
        {
            int playerNumber = int.Parse(splitLine[1]);
            string techName = splitLine[2];
            client.SetTechnologyResearched(playerNumber, techName);
        }

        /// <summary>
        /// Interprets and passes on a message from the server to
        /// the client that the game has finished, with an included
        /// result.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void GameOver(string[] splitLine)
        {
            bool won = bool.Parse(splitLine[1]);
            client.EndGame(won);
        }

        /// <summary>
        /// Sends a request for a building to be created at the specified location.
        /// 
        /// Message format:
        /// [message type = 0] [building type name] [x] [y] [player]
        /// </summary>
        public void RequestBuilding(BuildingType building, double x, double y)
        {
            outgoingWriter.WriteLine("0 " + building.name + " " + x + " " + y + " " + client.playerNumber);
        }

        /// <summary>
        /// Sends a request for a unit to be created at the specified building.
        /// 
        /// Message:
        /// [message type = 1] [building id] [unit type name]
        /// </summary>
        public void RequestUnit(Building sourceBuilding, UnitType unit)
        {
            outgoingWriter.WriteLine("1 " + sourceBuilding.id + " " + unit.name);
        }

        /// <summary>
        /// Sends a request for the given technology to be researched.
        /// 
        /// Message format:
        /// [message type = 2] [technology name] [player]
        /// </summary>
        public void RequestTechnology(Technology technology)
        {
            outgoingWriter.WriteLine("2 " + technology.name + " " + client.playerNumber);
        }

        /// <summary>
        /// Sends a request for the given unit to be moved to the given position.
        /// 
        /// Message format:
        /// [message type = 3] [unit id] [x] [y]
        /// </summary>
        public void RequestMove(Unit unit, double x, double y)
        {
            outgoingWriter.WriteLine("3 " + unit.id + " " + x + " " + y);
        }

        /// <summary>
        /// Sends a request for the given entity to be deleted.
        /// </summary>
        public void RequestDelete(Entity entity)
        {

        }

        /// <summary>
        /// Tells the server to remove the given amount of resource
        /// from the given resource.
        /// 
        /// Message format:
        /// [message type = 5] [amount] [resource id]
        /// </summary>
        public void InformResourceGather(double amount, Resource resource)
        {
            outgoingWriter.WriteLine("5 " + amount + " " + resource.id);
        }
    }
}
