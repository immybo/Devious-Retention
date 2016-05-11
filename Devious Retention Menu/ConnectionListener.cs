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
            listener.BeginAcceptTcpClient(Callback, null);
        }

        private void Callback(IAsyncResult result)
        {
            if (listener == null) return; // after stopped, possible due to async

            TcpClient newClient = listener.EndAcceptTcpClient(result);

            foreach(IReceiverFunction receiver in receivers)
            { 
                receiver.OnConnection(new Connection(newClient));
            }

            listener.BeginAcceptTcpClient(Callback, null);
        }

        public void StopListening()
        {
            if (listener == null) throw new InvalidOperationException("Attempting to end listening operation when not listening.");
            
            listener.Stop();
            listener = null;
        }

        public void AddReceiverFunction(IReceiverFunction receiver)
        {
            receivers.Add(receiver);
        }
    }

    public interface IReceiverFunction
    {
        void OnConnection(Connection newClient);
    }
}
