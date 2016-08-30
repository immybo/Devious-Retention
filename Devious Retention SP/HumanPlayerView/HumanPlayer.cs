using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Represents a human player on the local machine.
    /// </summary>
    public class HumanPlayer : Player, HumanPlayerListener
    {
        private HumanPlayerWindow window;

        public HumanPlayer(World world)
            : base(world)
        {
            window = new HumanPlayerWindow(this, world);
            Application.Run(window);
        }

        public void DoGameAreaClick(PointF worldCoordinate, MouseButtons buttons)
        {
            Console.WriteLine("Mouse pressed at " + worldCoordinate.X + ", " + worldCoordinate.Y + ".");
        }

        public void DoKeyPress(char keyChar)
        {
            Console.WriteLine("Key " + keyChar + " pressed.");
        }
    }
}
