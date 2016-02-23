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
                { 0, 0, 0, 1, 1 },
                { 1, 1, 0, 1, 1 },
                { 0, 0, 0, 0, 0 },
                { 1, 1, 1, 1, 0 },
            };
            List<Tile> tileTypes = new List<Tile>();
            foreach (Tile t in GameInfo.tiles.Values)
                tileTypes.Add(t);
            Map map = new Map(tileTypes, tiles, 5, 4);

            // CREATE CONNECTIONS
            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"));
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"));

            GameWindow gameWindow = new GameWindow();
            GameClient client = new GameClient(1, 8, map, gameWindow, cts, null);
            GameServer server = new GameServer(new List<STCConnection> { stc }, map);

            stc.SetServer(server);
            cts.SetClient(client);
            stc.Connect();
            cts.Connect();

            // TESTING STUFF
            server.SpawnEntity(client.info.unitTypes["TestUnit"], 1, 0, 0);
            server.SpawnEntity(client.info.unitTypes["TestUnit"], 2, 3, 3);

            Application.Run(gameWindow);
        }
    }
}
