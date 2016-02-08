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
        public int[,] tiles { get; private set; }
        private List<Tile> possibleTiles;

        /// <summary>
        /// When a map is constructed, a list of all possible tiles must be
        /// provided in order. Then, the actual tiles on the map are given
        /// as integers; indices in the list of possible tiles.
        /// </summary>
        public Map(List<Tile> possibleTiles, int[,] tiles, int width, int height)
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
            return possibleTiles[tiles[x,y]];
        }

        /// <summary>
        /// Returns the list of coordinates which the given entity
        /// is at least partially on.
        /// Uses the given map to make sure none of them are out of bounds.
        /// </summary>
        public static List<Coordinate> GetIncludedTiles(Map map, Entity entity)
        {
            return GetIncludedTiles(map, entity.x, entity.y, entity.type.size);
        }

        /// <summary>
        /// Returns the list of coordinates which an entity with the given
        /// x, y and size would be at least partially on.
        /// Uses the given map to make sure none of them are out of bounds.
        /// </summary>
        public static List<Coordinate> GetIncludedTiles(Map map, double x, double y, double size)
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            // Assuming that no entity will be *exactly* on a square, it will occupy (size+1)*(size+1) partial squares
            // We have to check that it's not off the map just in case the entity IS exactly on the square, because this would cause a crash.
            // However, checking one extra row/column of tiles isn't a big deal in the rare case that this happens.
            for (int i = 0; i <= size; i++)
                for (int j = 0; j < size; j++)
                    if ((int)x + i < map.width && (int)y + j < map.height)
                        coordinates.Add(new Coordinate((int)x + i, (int)y + j));

            return coordinates;
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
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
