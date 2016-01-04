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

        }

        /// <summary>
        /// Returns the tile in this map at the given co-ordinates.
        /// </summary>
        public Tile GetTile(int x, int y)
        {
            return null;
        }
    }

    /// <summary>
    /// A tile is immutable, because it defines a type of
    /// tile; i.e. what can move over it, the image to draw of it.
    /// </summary>
    public class Tile
    {
        private Image image { get; }
        // Whether building foundations can be put on this tile
        private bool buildable { get; }
        // Whether the given unit type can move through this tile
        private bool[] unitTypePassable { get; }

        private String name { get; }
        private int id { get; }

        public Tile(Image image, bool buildable, bool[] unitTypePassable, String name, int id)
        {

        }
    }
}
