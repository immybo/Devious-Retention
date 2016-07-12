using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Devious_Retention
{
    /// <summary>
    /// A map is merely a list of which tiles are at which co-ordinates.
    /// </summary>
    public class Map
    {
        private static readonly Dictionary<string, string> mapTypes = new Dictionary<string, string>()
        {
            { "Rocky Plains", "RockyPlains" }
        };

        public int width { get; private set; }
        public int height { get; private set; }
        private int[,] tiles;
        private Tile[] possibleTiles;
        public List<Coordinate> startingPositions { get; private set; }

        /// <summary>
        /// When a map is constructed, a list of all possible tiles must be
        /// provided in order. Then, the actual tiles on the map are given
        /// as integers; indices in the list of possible tiles.
        /// </summary>
        public Map(Tile[] possibleTiles, int[,] tiles, int width, int height, List<Coordinate> startingPositions)
        {
            this.possibleTiles = possibleTiles;
            this.tiles = tiles;
            this.width = width;
            this.height = height;
            this.startingPositions = startingPositions;
        }

        /// <summary>
        /// Returns the type of tile at the specified coordinate.
        /// </summary>
        public Tile GetTile(int x, int y)
        {
            return possibleTiles[tiles[x, y]];
        }

        /// <summary>
        /// Returns the list of coordinates which the given entity
        /// is at least partially on.
        /// Uses the given map to make sure none of them are out of bounds.
        /// </summary>
        public List<Coordinate> GetIncludedTiles(Entity entity)
        {
            return GetIncludedTiles(entity.X, entity.Y, entity.Type.size);
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
                        if (e.X + e.Type.size > x && e.Y + e.Type.size > y
                        && e.X < x + size && e.Y < y + size) // If they collide, return true
                            if (!(e is Resource) || includeResource)
                                return e;
            // If nothing collides return false
            return null;
        }

        /// <summary>
        /// Packs a Map object into a string such that it can
        /// be unpacked into an equal Map object by Map.FromString.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(startingPositions.Count+",");
            foreach(Coordinate c in startingPositions)
            {
                builder.Append(c.x + "," + c.y + ",");
            }

            builder.Append(width + "," + height + ",");
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    builder.Append(tiles[i,j]+",");
                }
            }
            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        /// <summary>
        /// Unpacks a Map object from a string such that, if the
        /// string was generated by map.ToString, with the same list
        /// of possible tiles, an equal map will be returned.
        /// </summary>
        public static Map FromString(string mapString, Tile[] possibleTiles)
        {
            int position = 0;
            string[] mapStringComponents = mapString.Split(',');

            int numStartingPositions = int.Parse(mapStringComponents[position++]);
            List<Coordinate> startingPositions = new List<Coordinate>();
            for(int i = 0; i < numStartingPositions; i++)
            {
                int x = int.Parse(mapStringComponents[position++]);
                int y = int.Parse(mapStringComponents[position++]);
                startingPositions.Add(new Coordinate(x, y));
            }

            int width = int.Parse(mapStringComponents[position++]);
            int height = int.Parse(mapStringComponents[position++]);
            int[,] tiles = new int[height, width];
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    tiles[i, j] = int.Parse(mapStringComponents[position++]);
                }
            }

            return new Map(possibleTiles, tiles, width, height, startingPositions);
        }

        /// <summary>
        /// Generates and returns a map with the specified width,
        /// height, number of players (for generating starting 
        /// coordinates) and list of possible tiles.
        /// 
        /// The map type describes which style of map will be 
        /// generated. 
        /// </summary>
        public static Map GenerateMap(MapType mapType, int width, int height, int numPlayers)
        {
            int[,] tiles = new int[height, width];

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

            Map map = new Map(mapType.GetPossibleTiles(), tiles, width, height, startingPositions);
            mapType.PopulateMap(map);
            return map;
        }

        public static MapType GetMapType(string name)
        {
            if (!mapTypes.ContainsKey(name))
                throw new ArgumentException("There is no map type with the name " + name);

            // We don't really have to use reflection here, but it's pretty cool to do so
            Console.WriteLine(typeof(MapTypes.RockyPlains).ToString());
            Type mapTypeType = Type.GetType("Devious_Retention.MapTypes."+mapTypes[name]);
            return (MapType)Activator.CreateInstance(mapTypeType);
        }

        public static string[] GetPossibleMapTypes()
        {
            return (string[])mapTypes.Keys.ToArray().Clone();
        }

        public abstract class MapType
        {
            public abstract void PopulateMap(Map initialMap);
            public abstract Tile[] GetPossibleTiles();
            protected void SetTile(Map map, int x, int y, int tile)
            {
                map.tiles[y,x] = tile;
            }
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

        public string name { get; private set; }

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
