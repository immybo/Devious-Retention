using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A ConnectionListener is an object which listens for connections on a specified
    /// port and invokes a callback when it receives a connection.
    /// </summary>
    public class ConnectionListener
    {
        private int port;
        private TcpListener listener;
        private bool listenerGoing = false;

        private List<IReceiverFunction> receivers;

        public ConnectionListener(int port)
        {
            this.port = port;
            receivers = new List<IReceiverFunction>();
        }

        public void BeginListening()
        {
            if (listener != null) throw new InvalidOperationException("Attempting to begin listening operation when already listening.");

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listenerGoing = true;
            listener.BeginAcceptTcpClient(Callback, null);
        }

        private void Callback(IAsyncResult result)
        {
            if (!listenerGoing) return; // after stopped, possible due to async

            TcpClient newClient = listener.EndAcceptTcpClient(result);

            foreach(IReceiverFunction receiver in receivers)
            { 
                receiver.OnConnection(new Connection(newClient));
            }

            listener.BeginAcceptTcpClient(Callback, null);
        }

        public void StopListening()
        {
            if (!listenerGoing) throw new InvalidOperationException("Attempting to end listening operation when not listening.");

            listenerGoing = false;
            listener.Stop();
        }

        public void AddReceiverFunction(IReceiverFunction receiver)
        {
            receivers.Add(receiver);
        }
    }

    /// <summary>
    /// Defines a class's ability to handle clients being
    /// connected from a ConnectionListener.
    /// </summary>
    public interface IReceiverFunction
    {
        void OnConnection(Connection newClient);
    }
}
