using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
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

            //client.AddEntity(0, "TestUnit", 0, 0, 1);
            //client.AddEntity(1, "TestBuilding", 1.1, 1.1, 1);
            //client.AddEntity(0, "TestUnit", 13, 13, 2);

            // CREATE CONNECTIONS
            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"), null);
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"), client);
            stc.Connect();
            cts.Connect();

            //stc.InformEntityAdd(new Resource(client.info.resourceTypes["TestResource"], 3,0));

            Unit deletionTestEntity = new Unit(client.info.unitTypes["TestUnit"], Unit.nextID, 5, 5, 1);
            Unit.IncrementNextID();
            stc.InformEntityAdd(deletionTestEntity);
            stc.InformEntityDeletion(deletionTestEntity);

            Application.Run(gameWindow);
            
        }
    }
}
