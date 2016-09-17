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
            World world = new World();
            Player player = new HumanPlayer(world);
            world.AddEntity(new Entities.TestUnit(player, 3, 3, 1));
            world.AddEntity(new Entities.TestUnit(player, 6, 6, 1));
            Game game = new Game(new Player[] { player }, world, new GameConfiguration(30));
            game.RunGame();

            ((HumanPlayer)player).Run();
        }
    }
}
