using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_Menu
{
    static class Program
    {
        /// <summary>
        /// Creates the initial menu.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());
            */

            LobbyHost host = new LobbyHost();
            MultiplayerLobbyHandler client = new MultiplayerLobbyHandler(IPAddress.Parse("127.0.0.1"));

            Console.WriteLine(host.ToString());
            Console.WriteLine(client.ToString());

            client.WriteLine("username testing");

            Console.WriteLine(host.ToString());
            Console.WriteLine(client.ToString());
        }
    }
}
