using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            GameWindow gameWindow;
            DebugWindow debugWindow;

            Console.WriteLine(GameInfo.BASE_DIRECTORY);

            debugWindow = new DebugWindow();
            debugWindow.Show();
            GameInfo info = new GameInfo(true, debugWindow);

            debugWindow.SetDesktopLocation(2000, 100);

            gameWindow = new GameWindow();

            int[][] tiles = new int[][] { new int[5] { 1, 1, 1, 1, 1 }, new int[5] { 0, 1, 0, 1, 1 }, new int[5] { 1, 1, 1, 1, 1 }, new int[5] { 1, 1, 1, 1, 1 }, new int[5] { 1, 1, 1, 1, 1 } };
            List<Tile> tileTypes = new List<Tile>();
            foreach (Tile t in info.tiles.Values)
                tileTypes.Add(t);
            Map map = new Map(tileTypes, tiles, tiles[0].Length, tiles.Length);
            GameClient client = new GameClient(1, map, gameWindow, info, null, null);
            gameWindow.client = client;
            info.WriteDebug("Game window opened.", Color.Blue);

            info.WriteDefinitionsToDebug();
            
            client.units.Add(new Unit(info.unitTypes["TestUnit"], 4.1, 4.1));
            client.buildings.Add(new Building(info.buildingTypes["TestBuilding"], 1.1, 1.1));
            client.resources.Add(new Resource(info.resourceTypes["TestResource"], 2, 2));

            /* SELECTED BUILDING
            client.selected.Add(client.buildings.ElementAt(0));
            client.buildings.ElementAt(0).QueueUnit(info.unitTypes["TestUnit"]);
            client.buildings.ElementAt(0).QueueUnit(info.unitTypes["TestUnit"]);
            client.buildings.ElementAt(0).QueueUnit(info.unitTypes["TestUnit2"]);
            client.buildings.ElementAt(0).QueueUnit(info.unitTypes["TestUnit"]);
            */
            /*
            client.selected.Add(client.units.ElementAt(0));
            */
            client.selected.Add(client.resources.ElementAt(0));

            Application.Run(gameWindow);

        }
    }
}
