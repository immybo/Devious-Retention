using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            new Game(new Player[] { new HumanPlayer(new World()) }, new World(), new GameConfiguration(30)).RunGame();
        }
    }
}
