﻿using System;
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

            GameWindow gameWindow;
            DebugWindow debugWindow;

            Console.WriteLine(GameInfo.BASE_DIRECTORY);

            debugWindow = new DebugWindow();
            debugWindow.Show();
            GameInfo info = new GameInfo(true, debugWindow);

            debugWindow.SetDesktopLocation(2000, 100);

            gameWindow = new GameWindow();

            int[][] tiles = new int[][] {
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };
            List<Tile> tileTypes = new List<Tile>();
            foreach (Tile t in info.tiles.Values)
                tileTypes.Add(t);
            Map map = new Map(tileTypes, tiles, tiles[0].Length, tiles.Length);
            GameClient client = new GameClient(1, map, gameWindow, info, null, null);
            info.WriteDebug("Game window opened.", Color.Blue);

            info.WriteDefinitionsToDebug();
            
            client.units.Add(new Unit(info.unitTypes["TestUnit"], 0,0, 1));
            client.buildings.Add(new Building(info.buildingTypes["TestBuilding"], 1.1, 1.1, 1));
            // other player's unit
            client.units.Add(new Unit(info.unitTypes["TestUnit"], 13, 13, 2));
            
            foreach(Unit u in client.units)
            {
                gameWindow.UpdateLOSAdd(u);
            }
            foreach(Building b in client.buildings)
            {
                gameWindow.UpdateLOSAdd(b);
            }

            STCConnection stc = new STCConnection(IPAddress.Parse("127.0.0.1"), null);
            CTSConnection cts = new CTSConnection(IPAddress.Parse("127.0.0.1"), client);
            stc.Connect();
            cts.Connect();
            stc.InformEntityAdd(new Resource(info.resourceTypes["TestResource"], 3,0));

            Application.Run(gameWindow);
            
        }
    }
}
