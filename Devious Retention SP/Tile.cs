using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Defines the "background" for a square. Tiles can be drawn differently
    /// (e.g. some are green, some grey), and can be walked over by different
    /// types of entity.
    /// </summary>
    public abstract class Tile : Drawable
    {
        private PointF position;

        /// <summary>
        /// Sets this tile's position to be the given one.
        /// This must be called before drawing this tile.
        /// </summary>
        public void SetPosition(PointF newPosition)
        {
            this.position = newPosition;
        }

        public void Draw(Graphics g, PositionTransformation p)
        {
            Point topCorner = p.Transform(position);
            SizeF rectSize = new SizeF(new PointF((float)Math.Ceiling(p.Scale().X), (float)Math.Ceiling(p.Scale().Y)));
            RectangleF tileRect = new RectangleF(topCorner, rectSize);
            this.DrawAtPosition(g, tileRect);
        }

        /// <summary>
        /// Draws this tile at the given world coordinate.
        /// </summary>
        public abstract void DrawAtPosition(Graphics g, RectangleF graphicsRect);
    }
}
