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
        private HumanPlayerListener listener;

        public GameArea(World world, HumanPlayerListener listener)
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
            this.world = world;
            this.listener = listener;
            this.MouseClick += new MouseEventHandler(DoMouse);
        }

        private void DoMouse(object o, MouseEventArgs e)
        {
            PointF worldCoordinate = new PointF(
                e.X / ((float)this.Width / world.Map.Width),
                e.Y / ((float)this.Height / world.Map.Height)
                );
            listener.DoGameAreaClick(worldCoordinate, e.Button);
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
