using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace NetworkingHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = null;
            try
            {
                host = new ServiceHost(typeof(Networking.ServerToClient));
                host.Open();
                Console.WriteLine("\n\nService is Running  at following address");
                Console.WriteLine("\nhttp://localhost:9001/MyMathService");
                Console.WriteLine("\nnet.tcp://localhost:9002/MyMathService");
            }
            catch (Exception eX)
            {
                host = null;
                Console.WriteLine("Service can not be started \n\nError Message [" + eX.Message + "]");
            }
            if (host != null)
            {
                Console.WriteLine("\nPress any key to close the Service");
                Console.ReadKey();
                host.Close();
                host = null;
            }
        }
    }
}
