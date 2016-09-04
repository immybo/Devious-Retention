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
        private Timer drawClock;

        public HumanPlayer(World world)
            : base(world)
        {
        }

        public void Run()
        {
            window = new HumanPlayerWindow(this, this.world);

            drawClock = new Timer();
            drawClock.Interval = 16;
            drawClock.Tick += new EventHandler(Tick);
            drawClock.Start();

            Application.Run(window);
        }

        public void Tick(object o, EventArgs e)
        {
            window.Refresh();
        }

        public void DoGameAreaClick(PointF worldCoordinate, MouseButtons buttons)
        {
            Console.WriteLine("Mouse pressed at " + worldCoordinate.X + ", " + worldCoordinate.Y + ".");
        }

        public void DoKeyPress(char keyChar)
        {
            Console.WriteLine("Key " + keyChar + " pressed.");
        }

        public void Tick()
        {
            window.Refresh();
        }
    }
}
