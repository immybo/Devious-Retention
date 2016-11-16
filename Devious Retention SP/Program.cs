using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Devious_Retention_SP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            World world = new World();
            Player player = new HumanPlayer(world, Color.Blue);
            Player player2 = new HumanPlayer(world, Color.Red);
            world.AddEntity(new Entities.TestUnit(player, 3, 3, 1));
            world.AddEntity(new Entities.TestBuilding(player2, 1, 1, 1.5));
            world.AddEntity(new Entities.TestUnit(player2, 5, 5, 1));
            Game game = new Game(new Player[] { player, player2 }, world, new GameConfiguration(30));
            game.RunGame();

            ((HumanPlayer)player).Run();
        }
    }
}
