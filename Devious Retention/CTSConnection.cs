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

        public CTSConnection(IPAddress ip, GameClient client)
        {
            this.ip = ip;
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
        /// Processes a request to add an entity from the server to the client.
        /// </summary>
        /// <param name="splitLine">The received line, split by spaces.</param>
        private void AddEntity(string[] splitLine)
        {
            int entityType = int.Parse(splitLine[1]);
            string typeName = splitLine[2];
            double xPos = double.Parse(splitLine[3]);
            double yPos = double.Parse(splitLine[4]);
            int playerNumber = int.Parse(splitLine[1]) == 2 ? 0 : int.Parse(splitLine[5]); // use a player number of 0 if it's a resource
            client.AddEntity(entityType, typeName, xPos, yPos, playerNumber);
        }

        /// <summary>
        /// Sends a request for a building to be created at the specified location.
        /// </summary>
        public void RequestBuilding(BuildingType building, double x, double y)
        {

        }

        /// <summary>
        /// Sends a request for a unit to be created at the specified building.
        /// </summary>
        public void RequestUnit(Building sourceBuilding, UnitType unit)
        {

        }

        /// <summary>
        /// Sends a request for the given technology to be researched.
        /// </summary>
        public void RequestTechnology(Technology technology)
        {

        }

        /// <summary>
        /// Sends a request for the given unit to be moved by the given amount.
        /// </summary>
        public void RequestMove(Unit unit, double dX, double dY)
        {

        }

        /// <summary>
        /// Sends a request for the given entity to be deleted.
        /// </summary>
        public void RequestDelete(Entity entity)
        {

        }
    }
}
