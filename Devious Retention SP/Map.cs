using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// A map defines a 2D array of tiles, and how to
    /// generate itself.
    /// </summary>
    public class Map : Drawable
    {
        private enum Tile
        {
            GRASS
        }

        private Tile[,] tiles;
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Generic constructor for a Map.
        /// </summary>
        public Map()
        {
            tiles = new Tile[10,10];
            this.Width = 10;
            this.Height = 10;
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    tiles[i, j] = Tile.GRASS;
                }
            }
        }

        public void Draw(Graphics g, PositionTransformation p)
        {
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    Point topCorner = p.Transform(new PointF(i, j));
                    if (tiles[i,j] == Tile.GRASS)
                    {
                        SizeF rectSize = new SizeF(new PointF((float)Math.Ceiling(p.Scale().X), (float)Math.Ceiling(p.Scale().Y)));
                        RectangleF tileRect = new RectangleF(topCorner, rectSize);
                        g.FillRectangle(new SolidBrush(Color.Green), tileRect);
                    }
                }
            }
        }
    }
}
