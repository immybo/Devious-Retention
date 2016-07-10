using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention.MapTypes
{
    class RockyPlains : Map.IMapType
    {
        public void PopulateMap(Map map)
        {
            for(int i = 0; i < map.width; i++)
            {
                for(int j = 0; j < map.height; j++)
                {
                    map.tiles[j, i] = 0;
                }
            }
        }

        public Tile[] GetPossibleTiles()
        {
            return GameInfo.tiles.Values.ToArray();
        }
    }
}
