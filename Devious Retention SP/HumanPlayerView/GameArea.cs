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
        // Relative to 1:1 scrolling
        private const float SCROLL_SPEED = 1f;

        private World world;
        private HumanPlayerListener listener;

        private Point mouseDownLocation;
        private bool isMouseDown;

        // The top left point of the player's view; this is used to calculate what to show the player
        public PointF playerView;

        public GameArea(World world, HumanPlayerListener listener, PointF initialPlayerView)
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
            this.isMouseDown = false;
            this.world = world;
            this.listener = listener;
            this.playerView = initialPlayerView;

            this.MouseClick += new MouseEventHandler(DoMouse);

            this.MouseDown += delegate (object o, MouseEventArgs e) {
                mouseDownLocation = e.Location;
                isMouseDown = true;
            };
            this.MouseUp += delegate (object o, MouseEventArgs e) { isMouseDown = false; };
            this.MouseEnter += delegate (object o, EventArgs e) { isMouseDown = false; };
            this.MouseMove += DoMouseDrag;
        }

        private void DoMouse(object o, MouseEventArgs e)
        {
            PointF worldCoordinate = new PointF(
                e.X / ((float)this.Width / world.Map.Width),
                e.Y / ((float)this.Height / world.Map.Height)
                );
            listener.DoGameAreaClick(worldCoordinate, e.Button);
        }

        /// <summary>
        /// Performs the action corresponding to a mouse drag.
        /// Only performs any action if the mouse is considered to be down;
        /// does nothing otherwise.
        /// </summary>
        private void DoMouseDrag(object o, MouseEventArgs e)
        {
            if (!isMouseDown) return;

            float tileWidth = this.Bounds.Width / world.Map.Width;
            float tileHeight = this.Bounds.Height / world.Map.Height;

            int deltaX = e.X - mouseDownLocation.X;
            int deltaY = e.Y - mouseDownLocation.Y;

            playerView.X += deltaX * SCROLL_SPEED / tileWidth;
            playerView.Y += deltaY * SCROLL_SPEED / tileHeight;

            mouseDownLocation = e.Location;

            Refresh();
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = g.ClipBounds;

            float tileWidth = bounds.Width / world.Map.Width;
            float tileHeight = bounds.Height / world.Map.Height;

            PositionTransformation worldTransform = new PositionTransformation(
                (int)(bounds.X + playerView.X*tileWidth), (int)(bounds.Y + playerView.Y* tileHeight),
                tileWidth, tileHeight);

            world.Draw(g, worldTransform);
        }
    }
}
