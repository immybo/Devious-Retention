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

        public STCConnection(IPAddress ip, GameServer server)
        {
            this.ip = ip;
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
                    Console.WriteLine(reader.ReadLine());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
        /// [message type=0] [0=unit,1=building,2=resource] [type name] [xpos] [ypos] [player (if not a resource)]
        /// </summary>
        public void InformEntityAdd(Entity entity)
        {
            if (outgoingSocket == null || !outgoingSocket.Connected) return;

            int entityType = -1;
            string typeName = "";
            if (entity is Unit) { entityType = 0; typeName = ((Unit)entity).type.name; }
            else if (entity is Building) { entityType = 1; typeName = ((Building)entity).type.name; }
            else if (entity is Resource) { entityType = 2; typeName = ((Resource)entity).type.name; }
            outgoingWriter.WriteLine("0 " + entityType + " " + typeName + " " + entity.GetX() + " " + entity.GetY() + " " + entity.GetPlayerNumber());
        }

        /// <summary>
        /// Informs the client that a given entity
        /// has been deleted.
        /// </summary>
        public void InformEntityDeletion(Entity entity)
        {

        }

        /// <summary>
        /// Informs the client that the map has been
        /// changed to the given one. Typically only used
        /// at the start of a game.
        /// </summary>
        public void InformMap(Map map)
        {
        }

        /// <summary>
        /// Informs the client that the game has ended.
        /// </summary>
        /// <param name="won">Whether or not this player won the game.</param>
        public void InformGameOver(Boolean won)
        {

        }

        /// <summary>
        /// Informs the client to update the game.
        /// </summary>
        public void Tick()
        {

        }
    }
}
