using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Devious_Retention_SP.HumanPlayerView
{
    class GameArea : Panel
    {
        private World world;

        public GameArea(World world)
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
            this.world = world;
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = g.ClipBounds;

            PositionTransformation worldTransform = new PositionTransformation(
                (int)bounds.X, (int)bounds.Y,
                (float)bounds.Width / world.Map.Width, (float)bounds.Height / world.Map.Height
                );
            world.Draw(g, worldTransform);
        }
    }
}
