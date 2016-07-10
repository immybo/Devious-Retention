using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GameInfo.ReadDefinitions();
            Unit.ResetNextID();
            Building.ResetNextID();
            Resource.ResetNextID();
            
            Map map = Map.GenerateMap(Map.GetMapType("Rocky Plains"), 20, 20, 2);

            // CREATE CONNECTIONS
            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"));
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"));

            LocalPlayer player1 = new LocalPlayer(new Player.Relation[]{ Player.Relation.ALLIED, Player.Relation.ENEMY }, 0, Color.Blue, null, new GameInfo());
            Player player2 = new Player(new Player.Relation[] { Player.Relation.ENEMY, Player.Relation.ALLIED }, 1, Color.Red, null, new GameInfo());

            Player[] players = { player1, player2 };

            GameClient client = new GameClient(player1, players, new World(map), cts, null);
            GameServer server = new GameServer(new List<STCConnection> { stc }, new int[] { 1 }, map);

            stc.SetServer(server);
            cts.SetClient(client, player1);
            stc.Connect();
            cts.Connect();

            // TESTING STUFF
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 0, 0, 0);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 3, 3);

            Application.Run(client.GetWindow());
        }
    }
}
