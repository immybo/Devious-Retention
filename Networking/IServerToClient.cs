using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    [ServiceContract]
    public interface IServerToClient
    {
        string ReturnString(string input);
    }
}
