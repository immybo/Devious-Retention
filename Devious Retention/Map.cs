using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public List<Coordinate> startingPositions { get; private set; }

        /// <summary>
        /// When a map is constructed, a list of all possible tiles must be
        /// provided in order. Then, the actual tiles on the map are given
        /// as integers; indices in the list of possible tiles.
        /// </summary>
        public Map(List<Tile> possibleTiles, int[,] tiles, int width, int height, List<Coordinate> startingPositions)
        {
            this.possibleTiles = possibleTiles;
            this.tiles = tiles;
            this.width = width;
            this.height = height;
            this.startingPositions = startingPositions;
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
        public List<Coordinate> GetIncludedTiles(Entity entity)
        {
            return GetIncludedTiles(entity.x, entity.y, entity.type.size);
        }

        /// <summary>
        /// Returns the list of coordinates which an entity with the given
        /// x, y and size would be at least partially on.
        /// Also makes sure none of them are out of bounds.
        /// </summary>
        public List<Coordinate> GetIncludedTiles(double x, double y, double size)
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            // Assuming that no entity will be *exactly* on a square, it will occupy (size+1)*(size+1) partial squares
            // We have to check that it's not off the map just in case the entity IS exactly on the square, because this would cause a crash.
            // However, checking one extra row/column of tiles isn't a big deal in the rare case that this happens.
            for (int i = 0; i <= size; i++)
                for (int j = 0; j < size; j++)
                    if ((int)x + i < this.width && (int)y + j < this.height)
                        coordinates.Add(new Coordinate((int)x + i, (int)y + j));

            return coordinates;
        }

        /// <summary>
        /// Returns whether or not a new entity
        /// at the given coordinates
        /// would collide with any of the other given entities.
        /// </summary>
        /// <param name="includeResource">Whether or not to care about any resources colliding.</param>
        public Entity Collides(double x, double y, double size, List<Entity>[,] entitiesBySquare, bool includeResource)
        {
            List<Coordinate> includedTiles = GetIncludedTiles(x, y, size);
            foreach (Coordinate c in includedTiles)
                if (entitiesBySquare[c.x, c.y] != null)
                    foreach (Entity e in entitiesBySquare[c.x, c.y])
                        if (e.x + e.type.size > x && e.y + e.type.size > y
                        && e.x < x + size && e.y < y + size) // If they collide, return true
                            if (!(e is Resource) || includeResource)
                                return e;
            // If nothing collides return false
            return null;
        }

        /// <summary>
        /// Generates and returns a map with the specified width,
        /// height, number of players (for generating starting 
        /// coordinates) and list of possible tiles.
        /// </summary>
        public static Map GenerateMap(List<Tile> possibleTiles, int width, int height, int numPlayers)
        {
            int[,] tiles = new int[height, width];
            Random random = new Random();
            
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    // Just generate it randomly from now
                    if (random.Next(10) >= 9)
                        tiles[i,j] = possibleTiles.Count - 1;
                    else
                        tiles[i,j] = 0;
                }
            }

            // We want starting positions to be out from the center, and evenly spaced
            List<Coordinate> startingPositions = new List<Coordinate>();
            Coordinate center = new Coordinate(width / 2, height / 2);
            Coordinate distances = new Coordinate(width / 4, height / 4);
            for (int i = 0; i < numPlayers; i++)
            {
                // Keep them the same angle apart
                double angle = Math.PI * 2 * (i / numPlayers);

                startingPositions.Add(new Coordinate((int)(center.x + Math.Sin(angle) * distances.x), (int)(center.y - Math.Cos(angle) * distances.y)));
            }

            return new Map(possibleTiles, tiles, width, height, startingPositions);
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
            try
            {
                image = Image.FromFile(GameInfo.TILE_IMAGE_BASE + imageName);
            }
            catch(IOException)
            {
                image = null;
            }
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
