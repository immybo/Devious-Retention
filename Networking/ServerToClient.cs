using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class ServerToClient : IServerToClient
    {
        public string ReturnString(string input)
        {
            return "You sent: " + input;
        }
    }
}
