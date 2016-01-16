using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A map is merely a list of which tiles are at which co-ordinates.
    /// </summary>
    public class Map
    {
        public int width { get; private set; }
        public int height { get; private set; }
        private int[][] tiles;
        private List<Tile> possibleTiles;

        /// <summary>
        /// When a map is constructed, a list of all possible tiles must be
        /// provided in order. Then, the actual tiles on the map are given
        /// as integers; indices in the list of possible tiles.
        /// </summary>
        public Map(List<Tile> possibleTiles, int[][] tiles, int width, int height)
        {
            this.possibleTiles = possibleTiles;
            this.tiles = tiles;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Returns the tile in this map at the given co-ordinates.
        /// </summary>
        public Tile GetTile(int x, int y)
        {
            return possibleTiles[tiles[x][y]];
        }
    }

    /// <summary>
    /// A tile is immutable, because it defines a type of
    /// tile; i.e. what can move over it, the image to draw of it.
    /// </summary>
    public class Tile
    {
        // The filename of the image should be GameInfo.RESOURCE_IMAGE_BASE+imageName
        private string imageName;
        public Image image { get; private set; }
        // The color of this tile as displayed on the minimap
        private int[] colorRGB;
        public Color color { get; private set; }
        // Whether building foundations can be put on this tile
        public bool buildable { get; private set; }
        // Whether the given unit type can move through this tile
        public bool[] unitTypePassable { get; private set; }

        public String name { get; private set; }

        /// <summary>
        /// Anything attempting to create a Tile from a file must first
        /// parse the string into these attributes.
        /// </summary>
        public Tile(string name, string imageName, bool buildable, bool[] unitTypePassable, int[] colorRGB)
        {
            this.name = name;
            this.imageName = imageName;
            image = Image.FromFile(GameInfo.TILE_IMAGE_BASE + imageName);
            this.buildable = buildable;
            this.unitTypePassable = unitTypePassable;
            this.colorRGB = colorRGB;
            color = Color.FromArgb(colorRGB[0], colorRGB[1], colorRGB[2]);
        }

        /// <summary>
        /// Returns "[name] [image filename] [buildable] [unit type passable 1 .. unit type passable x]"
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            builder.Append(imageName + " ");
            builder.Append(buildable + " ");
            foreach (bool u in unitTypePassable)
                builder.Append(u + " ");
            return builder.ToString();
        }
    }

    public struct Coordinate
    {
        public int x;
        public int y;
        public Coordinate(int x, int y)
        {
            this.x = x; this.y = y;
        }
        public override bool Equals(Object obj)
        {
            return (obj is Coordinate && ((Coordinate)obj).x == x && ((Coordinate)obj).y == y);
        }
    }
}
