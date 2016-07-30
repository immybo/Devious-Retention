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
        private bool isListening = false;

        private IPEndPoint endPoint;

        private TcpClient client;
        private NetworkStream stream;

        private StreamWriter outgoingWriter;
        private StreamReader incomingReader;

        private List<IConnectionDataListener> listeners;
        
        public Connection(IPAddress ip, int port)
        {
            endPoint = new IPEndPoint(ip, port);
            listeners = new List<IConnectionDataListener>();
        }

        public Connection(TcpClient client)
        {
            this.client = client;
            listeners = new List<IConnectionDataListener>();
            SetUpFromClient();
        }

        /// <summary>
        /// Attempts to connect to the IP address associated with this connection.
        /// Throws an exception if it is unable to do so within the specified time,
        /// or if the connection is refused.
        /// Begins listening to the connection if it is able to do so.
        /// </summary>
        public void Connect(int timeoutMs)
        {
            if (client != null) throw new InvalidOperationException("Attempting to connect a connection when already connected.");

            try {
                client = new TcpClient();
                if (!client.ConnectAsync(endPoint.Address, endPoint.Port).Wait(timeoutMs)) // Executes if it fails to do so within this time 
                {
                    throw new Exception();
                }
                SetUpFromClient();
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("Couldn't connect to IP " + endPoint.Address + ".\n" + e);
            }
        }

        public IPAddress GetLocalIP()
        {
            return (client.Client.LocalEndPoint as IPEndPoint).Address;
        }
        public IPAddress GetRemoteIP()
        {
            return (client.Client.RemoteEndPoint as IPEndPoint).Address;
        }

        private void SetUpFromClient()
        {
            stream = client.GetStream();

            incomingReader = new StreamReader(stream);
            outgoingWriter = new StreamWriter(stream);
            outgoingWriter.AutoFlush = true; // makes sure it always sends a line when written to
        }

        /// <summary>
        /// Adds a listener which will be notified whenever a line can read, if this
        /// connection is asynchronously receiving data at the time.
        /// Listeners may not be added while the connection is currently listening.
        /// </summary>
        public void AddConnectionDataListener(IConnectionDataListener listener)
        {
            if (isListening) throw new InvalidOperationException("Can't add a data connection listener to a connection while it is currently listening.");
            listeners.Add(listener);
        }

        /// <summary>
        /// Begins listening asynchronously to all data from the connection.
        /// Notifies all connection data listeners when any data is received.
        /// </summary>
        public async void BeginListening()
        {
            isListening = true;
            try
            {
                await ReadLinesAsync();
            }
            // Could throw either of these exceptions and just mean that the connection has been closed.
            // We can't catch them further up, though, because this is async and returns void.
            // If we didn't return void, we'd have to wait for it to finish in a method, and eventually
            // we'd either have to explicitly create a thread, return void, or call a new throw function
            // outside of this method.
            catch (ObjectDisposedException){}
            catch(IOException){}
            isListening = false;
        }

        /// <summary>
        /// Begins awaiting lines from the reader until the tcpclient is closed.
        /// </summary>
        private async Task ReadLinesAsync()
        {
            string line;
            try
            {
                while ((line = await incomingReader.ReadLineAsync()) != null)
                {
                    ProcessLine(line);
                }
            }
            catch (NullReferenceException) { } // closed
        }

        /// <summary>
        /// Informs all listeners that the given line has been read.
        /// </summary>
        private void ProcessLine(string line)
        {
            foreach(IConnectionDataListener listener in listeners)
            {
                listener.OnLineRead(this, line);
            }
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

    /// <summary>
    /// A class which can await for lines read from a connection,
    /// and respond appropriately when given them.
    /// </summary>
    public interface IConnectionDataListener
    {
        void OnLineRead(Connection connection, string line);
    }
}
