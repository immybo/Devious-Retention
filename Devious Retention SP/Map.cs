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
        private int width;
        private int height;

        /// <summary>
        /// Generic constructor for a Map.
        /// </summary>
        public Map()
        {
            tiles = new Tile[10,10];
            this.width = 10;
            this.height = 10;
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    tiles[i, j] = Tile.GRASS;
                }
            }
        }

        public void Draw(Graphics g, PositionTransformation p)
        {
            for(int i = 0; i < width; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    if(tiles[i,j] == Tile.GRASS)
                    {
                        Point topCorner = p.Transform(new PointF(i, j));
                        g.FillRectangle(new SolidBrush(Color.Green), new Rectangle(topCorner, new Size(new Point((int)p.Scale().X, (int)p.Scale().Y)))); // ..
                    }
                }
            }
        }
    }
}
