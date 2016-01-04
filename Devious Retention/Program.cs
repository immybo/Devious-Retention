using System;
using System.Collections.Generic;
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
            GameWindow gameWindow;
            int[][] tiles = new int[][] { new int[0] };
            List<Tile> tileTypes = new List<Tile>();
            Map map = new Map(tileTypes, tiles, tiles[0].Length, tiles.Length);

            Console.WriteLine(GameInfo.BASE_DIRECTORY);

            GameInfo info = new GameInfo(GameInfo.UNIT_FNAME, GameInfo.BUILDING_FNAME, GameInfo.RESOURCE_FNAME, GameInfo.TECHNOLOGY_FNAME, GameInfo.FACTION_FNAME);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(gameWindow = new GameWindow());
            gameWindow.client = new GameClient(1, map, gameWindow, info, null, null);
        }
    }
}
