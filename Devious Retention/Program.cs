﻿using System;
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
            int[][] tiles = new int[][] { new int[0] };
            List<Tile> tileTypes = new List<Tile>();
            Map map = new Map(tileTypes, tiles, tiles[0].Length, tiles.Length);

            Console.WriteLine(GameInfo.BASE_DIRECTORY);

            debugWindow = new DebugWindow();
            debugWindow.Show();
            GameInfo info = new GameInfo(true, debugWindow);

            debugWindow.SetDesktopLocation(2000, 100);

            gameWindow = new GameWindow();
            gameWindow.client = new GameClient(1, map, gameWindow, info, null, null);
            info.WriteDebug("Game window opened.", Color.Blue);

            info.WriteDefinitionsToDebug();

            Application.Run(gameWindow);

        }
    }
}
