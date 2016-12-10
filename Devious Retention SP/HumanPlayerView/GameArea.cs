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
        public const float SCROLL_SPEED = 1f;
        // Compared to width
        public const float MINIMAP_SIZE = 0.2f;
        public const int MINIMAP_BORDER_SIZE = 5;

        // Static
        public const int TILE_SIZE = 100;

        private World world;
        private HumanPlayerListener listener;

        private Point mouseDownLocation;
        private bool isMouseDown;
        private Rectangle selectedRect;

        // The top left point of the player's view; this is used to calculate what to show the player
        public PointF PlayerView;

        public GameArea(World world, HumanPlayerListener listener, PointF initialPlayerView)
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
            this.isMouseDown = false;
            this.world = world;
            this.listener = listener;
            this.PlayerView = initialPlayerView;

            this.MouseClick += new MouseEventHandler(DoMouse);

            this.MouseDown += delegate (object o, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left)
                {
                    mouseDownLocation = e.Location;
                    isMouseDown = true;
                }
            };
            this.MouseUp += delegate (object o, MouseEventArgs e)
            {
                if (isMouseDown)
                {
                    PositionTransformation transformation = GetTransformation();
                    PointF startPoint = transformation.TransformReverse(new Point(selectedRect.Left, selectedRect.Top));
                    SizeF size = new SizeF((float)(selectedRect.Right - selectedRect.Left) / transformation.Scale().X,
                                           (float)(selectedRect.Bottom - selectedRect.Top) / transformation.Scale().Y);
                    listener.DoGameAreaDrag(new RectangleF(startPoint, size));
                }
                selectedRect = new Rectangle(0, 0,0,0);
                isMouseDown = false;
            };
            this.MouseEnter += delegate (object o, EventArgs e) { isMouseDown = false; };
            this.MouseMove += DoMouseDrag;
        }

        private void DoMouse(object o, MouseEventArgs e)
        {
            listener.DoGameAreaClick(GetTransformation().TransformReverse(e.Location), e.Button);
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

            int leftMost = Math.Min(mouseDownLocation.X, e.X);
            int width = Math.Max(mouseDownLocation.X, e.X) - leftMost;
            int topMost = Math.Min(mouseDownLocation.Y, e.Y);
            int height = Math.Max(mouseDownLocation.Y, e.Y) - topMost;

            // Make sure we're not selecting too small an area; this is for QoL
            Rectangle proposedSelection = new Rectangle(leftMost, topMost, width, height);
            if (proposedSelection.Width * proposedSelection.Height < 0.1) return;

            selectedRect = new Rectangle(leftMost, topMost, width, height);

            Refresh();
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = g.ClipBounds;

            world.Draw(g, GetTransformation());

            if(isMouseDown && selectedRect != null)
            {
                g.DrawRectangle(Pens.Black, selectedRect);
            }

            float minimapSize = bounds.Width * MINIMAP_SIZE;
            float minimapX = bounds.Width - minimapSize;
            float minimapY = bounds.Height - minimapSize;
            RectangleF minimapBounds = new RectangleF(minimapX, minimapY, minimapSize, minimapSize);
            RenderMinimap(g, minimapBounds);
        }

        public void RenderMinimap(Graphics g, RectangleF bounds){
            g.FillRectangle(Brushes.Black, bounds);

            bounds.X += MINIMAP_BORDER_SIZE;
            bounds.Y += MINIMAP_BORDER_SIZE;
            bounds.Width -= MINIMAP_BORDER_SIZE * 2;
            bounds.Height -= MINIMAP_BORDER_SIZE * 2;
            
            Map map = world.Map;
            float tileWidth = bounds.Width / map.Width;

            // Account for non-square maps
            int dimensionDifference = Math.Abs(map.Width - map.Height);
            bool widthGreater = map.Width > map.Height;

            float greatestMapDimension = widthGreater ? map.Width : map.Height;
            float tileSize = bounds.Width / greatestMapDimension;

            // Center the smallest side 
            float xOffset = bounds.X + (widthGreater ? 0 : tileSize * dimensionDifference / 2);
            float yOffset = bounds.Y + (widthGreater ? tileSize * dimensionDifference / 2 : 0);

            for (int x = 0; x < map.Width; x++){
                for (int y = 0; y < map.Height; y++){
                    RectangleF tileRect = new RectangleF(xOffset + x*tileSize,
                                                         yOffset + y*tileSize,
                                                         tileSize, tileSize);
                    MapDraw.DrawTile(map.GetTile(x, y), g, tileRect);
                }
            }
            
            foreach (Entity e in world.GetEntities())
            {
                double x = xOffset + tileSize * e.X;
                double y = yOffset + tileSize * e.Y;

                int size = (int)(e.Size * tileSize);
                if (size < 1) size = 1;

                g.FillRectangle(new SolidBrush(e.Player.Color), new Rectangle((int)x, (int)y, size, size));
            }
        }

        private PositionTransformation GetTransformation()
        {
            return new PositionTransformation(
                    (int)(PlayerView.X * TILE_SIZE),
                    (int)(PlayerView.Y * TILE_SIZE),
                    TILE_SIZE,
                    TILE_SIZE);
        }
    }
}
