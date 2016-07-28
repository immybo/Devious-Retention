using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    public class GameBuilder
    {
        public static void BuildServer(Dictionary<STCConnection, ClientData> clientInfo)
        {
            Console.WriteLine("Building server [unimplemented]");
        }

        public static void BuildClient(CTSConnection connection)
        {
            Console.WriteLine("Building client [unimplemented]");
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
