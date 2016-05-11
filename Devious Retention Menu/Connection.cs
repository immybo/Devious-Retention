﻿using System;
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
        private IPEndPoint endPoint;

        private TcpClient client;
        private NetworkStream stream;

        private StreamWriter outgoingWriter;
        private StreamReader incomingReader;
        
        public Connection(IPAddress ip, int port)
        {
            endPoint = new IPEndPoint(ip, port);
        }

        public Connection(TcpClient client)
        {
            this.client = client;
            SetUpFromClient();
        }

        /// <summary>
        /// Attempts to connect to the IP address associated with this connection.
        /// Throws an exception if it is unable to do so.
        /// Begins listening to the connection if it is able to do so.
        /// </summary>
        public void Connect()
        {
            if (client != null) throw new InvalidOperationException("Attempting to connect a connection when already connected.");

            try {
                client = new TcpClient();
                client.Connect(endPoint);
                SetUpFromClient();
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("Couldn't connect to IP " + endPoint.Address + ".\n" + e);
            }
        }

        private void SetUpFromClient()
        {
            stream = client.GetStream();

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
            if (client != null)
            {
                client.Close();

                client = null; stream = null; outgoingWriter = null; incomingReader = null;
            }
        }

        /// <summary>
        /// Returns the end point of this connection's port.
        /// </summary>
        public int GetPort()
        {
            return endPoint.Port;
        }

        /// <summary>
        /// Returns whether or not this connection is currently open.
        /// </summary>
        public bool IsOpen()
        {
            return client != null;
        }
    }
}
