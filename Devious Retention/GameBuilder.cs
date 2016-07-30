using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    public class GameBuilder
    {
        public static void BuildServer(Dictionary<STCConnection, ClientData> clientInfo)
        {
            List<STCConnection> connections = clientInfo.Keys.ToList();
            Map map = Map.GenerateMap(Map.GetMapType(Map.GetPossibleMapTypes()[0]), 20, 20, clientInfo.Count);
            World world = new World(map);
            GameServer server = new GameServer(connections, new int[] { }, world);
            foreach (STCConnection c in connections)
            {
                c.SetServer(server);
                c.Listen();
                c.RepeatedlyAttemptConnect();
            }
            server.SyncMap();
        }

        public static void BuildClient(CTSConnection connection, List<ClientData> clientInfo, int localPlayerNumber)
        {
            ClientData[] clients = new ClientData[clientInfo.Count+1];
            foreach (ClientData data in clientInfo)
                clients[data.playerNumber] = data;
            Map tempMap = Map.EmptyMap();
            World world = new World(tempMap);
            LocalPlayer localPlayer = new LocalPlayer(Player.DefaultRelations(localPlayerNumber, clientInfo.Count),
                localPlayerNumber,
                clients[localPlayerNumber].color,
                GameInfo.factions[clients[localPlayerNumber].factionName],
                new GameInfo(),
                world);

            Player[] players = new Player[clientInfo.Count];
            players[localPlayerNumber-1] = localPlayer;
            for (int i = 1; i < clients.Count(); i++)
            {
                if (i == localPlayerNumber) continue;
                players[i] = new Player(Player.DefaultRelations(i, clients.Count()),
                    i+1, clients[i].color, GameInfo.factions[clients[i].factionName], new GameInfo());
            }

            connection.Listen();
            connection.Connect();
            GameClient client = new GameClient(localPlayer, players, world, connection);
            connection.SetClient(client, localPlayer);

            Application.Run(client.GetWindow());
        }

        /// <summary>
        /// Provides information about one client.
        /// </summary>
        public class ClientData
        {
            public int uniqueID;
            public string username;
            public int playerNumber;
            public Color color;

            public string factionName; // Kept as a primitive type and synced up on launch

            public ClientData(int uniqueID, string username, int playerNumber, Color color, string factionName)
            {
                this.username = username;
                this.playerNumber = playerNumber;
                this.color = color;
                this.factionName = factionName;
            }

            /// <summary>
            /// The opposite of the toString method for a clientdata.
            /// </summary>
            public static ClientData FromString(string inputString)
            {
                // We need to split it with "!!"
                string[] splitLine = inputString.Split(new string[] { "!!" }, StringSplitOptions.None);

                int uniqueID = int.Parse(splitLine[0]);
                int playerNumber = int.Parse(splitLine[1]);
                string username = splitLine[2];
                Color color = ColorTranslator.FromHtml(splitLine[3]);
                string factionName = splitLine[4];

                return new ClientData(uniqueID, username, playerNumber, color, factionName);
            }

            public override string ToString()
            {
                return uniqueID + "!!" + playerNumber + "!!" + username + "!!" + ColorTranslator.ToHtml(color) + "!!" + factionName;
            }
        }
    }
}
