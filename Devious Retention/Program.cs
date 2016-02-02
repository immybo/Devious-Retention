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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            GameInfo.ReadDefinitions();
            Unit.ResetNextID();
            Building.ResetNextID();
            Resource.ResetNextID();

            // CREATE MAP, WINDOW AND CLIENT
            int[,] tiles = new int[,] {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };
            List<Tile> tileTypes = new List<Tile>();
            foreach (Tile t in GameInfo.tiles.Values)
                tileTypes.Add(t);
            Map map = new Map(tileTypes, tiles, 15,15);

            GameWindow gameWindow = new GameWindow();
            GameClient client = new GameClient(1, 8, map, gameWindow, null, null);
            GameServer server = new GameServer(null, map);

            // CREATE CONNECTIONS
            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"), server);
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"), client);
            stc.Connect();
            cts.Connect();

            client.connection = cts;
            server.connections.Add(stc);

            // TESTING STUFF
            server.SpawnEntity(client.info.unitTypes["TestUnit"], 1, 2, 2);
            Thread.Sleep(10);

            stc.InformTechnologyResearch(1, client.info.technologies["TestTechnology"]);

            Application.Run(gameWindow);
        }
    }
}
