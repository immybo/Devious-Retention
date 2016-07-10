using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention.MapTypes
{
    class RockyPlains : Map.MapType
    {
        private static readonly double mountainChance = 0.05;
        private static readonly double mountainDensity = 0.4;
        private static readonly int minMountainSize = 2;
        private static readonly int maxMountainSize = 5;

        public override void PopulateMap(Map map)
        {
            Random rand = new Random();
            for(int i = 0; i < map.width; i++)
            {
                for(int j = 0; j < map.height; j++)
                {
                    SetTile(map, i, j, 0);
                }
            }

            int numMountains = (int)(mountainChance * (0.5+rand.NextDouble()) * map.width * map.height);

            for(int i = 0; i < numMountains; i++)
            {
                int mountainSize = rand.Next(minMountainSize, maxMountainSize);
                int mountainX = rand.Next(0, map.width);
                int mountainY = rand.Next(0, map.height);
                GenerateMountain(map, mountainDensity, mountainSize, mountainX, mountainY);
            }
        }

        private void GenerateMountain(Map map, double density, int size, int x, int y)
        {
            double distanceToCenter = 1;
            Random rand = new Random();
            for(int i = 0; i < size; i++)
            {
                if (x + i < 0 || x + i >= map.width) continue;

                for(int j = 0; j < size; j++)
                {
                    if (y + j < 0 || y + j >= map.height) continue;

                    distanceToCenter = Math.Sqrt(Math.Pow(x - i, 2) + Math.Pow(y - i, 2));
                    if (distanceToCenter < 0.1) distanceToCenter = 0.1;

                    double rockChance = density / (size/distanceToCenter);
                    SetTile(map, x + i, y + j, rand.NextDouble() > rockChance ? 0 : 1);
                }
            }
        }

        public override Tile[] GetPossibleTiles()
        {
            return GameInfo.tiles.Values.ToArray();
        }
    }
}
