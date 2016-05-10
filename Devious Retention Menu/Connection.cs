using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// Defines a socket connection between this device and one at
    /// the specified IP address. Can be used to read from or write
    /// to this connection.
    /// </summary>
    public class Connection
    {
        private IPAddress ip;
        private int port;

        private Socket socket;
        private NetworkStream stream;

        private StreamWriter outgoingWriter;
        private StreamReader incomingReader;
        
        public Connection(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        /// <summary>
        /// Attempts to connect to the IP address associated with this connection.
        /// Throws an exception if it is unable to do so.
        /// Begins listening to the connection if it is able to do so.
        /// </summary>
        public void Connect()
        {
            try {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ip, port);
                SetUpFromSocket();
            }
            catch(SocketException)
            {
                throw new InvalidOperationException("Couldn't connect to IP " + ip + ".");
            }
        }

        /// <summary>
        /// Listens for a connection to this machine.
        /// Throws an exception if it is unable to do so within the specified timeout.
        /// Begins listening to the connection if it is able to do so.
        /// </summary>
        public void ListenForConnection(int msTimeout)
        {
            DateTime endTime = DateTime.Now.AddMilliseconds(msTimeout);
            
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();

            while (true)
            {
                if (DateTime.Now.CompareTo(endTime) >= 0)
                {
                    listener.Stop();
                    throw new TimeoutException();
                }

                if (listener.Pending())
                {
                    socket = listener.AcceptSocket();
                    SetUpFromSocket();
                    return;
                }
            }
        }

        private void SetUpFromSocket()
        {
            stream = new NetworkStream(socket);

            incomingReader = new StreamReader(stream);
            outgoingWriter = new StreamWriter(stream);
            outgoingWriter.AutoFlush = true; // makes sure it always sends a line when written to
        }

        /// <summary>
        /// Attempts to read a line from the connection.
        /// </summary>
        /// <returns>The line that was read, or null if no line could be read.</returns>
        public string ReadLine()
        {
            if (incomingReader == null) throw new InvalidOperationException();

            return incomingReader.ReadLine();
        }

        /// <summary>
        /// Attempts to write a line to the connection, and sends it.
        /// </summary>
        /// <param name="line">The line that is to be written.</param>
        public void WriteLine(string line)
        {
            if (outgoingWriter == null) throw new InvalidOperationException();

            outgoingWriter.WriteLine(line);
        }

        /// <summary>
        /// Closes the underlying socket connection and stream, releasing
        /// all resources occupied by this connection.
        /// </summary>
        public void Close()
        {
            if (socket != null) socket.Close();
        }
    }
}
