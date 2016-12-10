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
        private Tile[,] tiles;
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// Generic constructor for a Map.
        /// </summary>
        public Map()
        {
            tiles = new Tile[20,20];
            this.Width = 20;
            this.Height = 20;

            Random random = new Random();
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    if (random.NextDouble() > 0.2)
                        tiles[i, j] = new Tiles.Grass();
                    else
                        tiles[i, j] = new Tiles.Mountain();

                    tiles[i, j].SetPosition(new PointF(i, j));
                }
            }
        }

        public void Draw(Graphics g, PositionTransformation p)
        {
            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    tiles[i, j].Draw(g, p);
                }
            }
        }

        public Tile GetTile(int x, int y){
            return tiles[x, y];
        }
    }
}
