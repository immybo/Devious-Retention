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
            
            Map map = Map.GenerateMap(Map.GetMapType("Rocky Plains"), 20, 20, 2);
            World world = new World(map);

            // CREATE CONNECTIONS
            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"));
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"));

            LocalPlayer player1 = new LocalPlayer(new Player.Relation[]{ Player.Relation.ALLIED, Player.Relation.ENEMY }, 0, Color.Blue, null, new GameInfo(), world);
            Player player2 = new Player(new Player.Relation[] { Player.Relation.ENEMY, Player.Relation.ALLIED }, 1, Color.Red, null, new GameInfo());

            Player[] players = { player1, player2 };
            
            GameServer server = new GameServer(new List<STCConnection> { stc }, new int[] { 1 }, world);
            GameClient client = new GameClient(player1, players, world, cts, null);

            stc.SetServer(server);
            cts.SetClient(client, player1);
            stc.Connect();
            cts.Connect();

            server.SyncMap();

            // TESTING STUFF
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 0, 2, 2);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 3, 3);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 3, 4);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 3, 5);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 4, 3);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 4, 4);
            server.SpawnEntity(client.GetLocalDefinitions().unitTypes["TestUnit"], 1, 4, 5);

            Application.Run(client.GetWindow());
        }
    }
}
