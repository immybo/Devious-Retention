using System;
using Devious_Retention;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DeviousRetentionTests
{
    [TestClass]
    public class MapTests
    {
        private List<Tile> possibleTiles;

        [TestInitialize]
        public void GeneratePossibleTiles()
        {
            Tile tile1 = new Tile("0", "", false, new bool[] { false }, new int[] { 0, 0, 0 });
            Tile tile2 = new Tile("1", "", false, new bool[] { false }, new int[] { 0, 0, 0 });
            possibleTiles = new List<Tile> { tile1, tile2 };
        }

        /// <summary>
        /// Tests the automatic map generation.
        /// </summary>
        [TestMethod]
        public void MapGenerationTest()
        {
            Map map = Map.GenerateMap(possibleTiles, 20, 20, 2);

            // Make sure that the starting positions are roughly opposite each other
            Assert.IsTrue(map.startingPositions[0].x > 8, "Player one starting position too far to the left. Expected=10 Actual=" + map.startingPositions[0].x);
            Assert.IsTrue(map.startingPositions[0].x < 12, "Player one starting position too far to the right. Expected=10 Actual=" + map.startingPositions[0].x);
            Assert.IsTrue(map.startingPositions[0].y > 3, "Player one starting position too far up. Expected=5 Actual=" + map.startingPositions[0].y);
            Assert.IsTrue(map.startingPositions[0].y < 7, "Player one starting position too far down. Expected=5 Actual=" + map.startingPositions[0].y);

            Assert.IsTrue(map.startingPositions[1].x > 8, "Player two starting position too far to the left. Expected=10 Actual=" + map.startingPositions[1].x);
            Assert.IsTrue(map.startingPositions[1].x < 12, "Player two starting position too far to the right. Expected=10 Actual=" + map.startingPositions[1].x);
            Assert.IsTrue(map.startingPositions[1].y > 13, "Player two starting position too far up. Expected=15 Actual=" + map.startingPositions[1].y);
            Assert.IsTrue(map.startingPositions[1].y < 17, "Player two starting position too far down. Expected=15 Actual=" + map.startingPositions[1].y);

            // Testing for more players isn't really possible -
            // If we wrote tests in for loops (i.e. testing all player amounts / all positions up to a point),
            // we would just be using the same code as in the method being tested, which is pointless.
            // If we instead manually checked against the coordinates in some cases, we would be calculating
            // the coordinates with the same equation... which is pointless again.
            // I suppose that's also true for the code above, but I've written it now.


        }

        /// <summary>
        /// Tests that maps have the correct attributes after creation.
        /// </summary>
        [TestMethod]
        public void MapAttributesTest()
        {
            Map map = Map.GenerateMap(possibleTiles, 1, 2, 12);

            Assert.AreEqual(map.width, 1, "Map width is incorrect. Expected=1 Actual=" + map.width);
            Assert.AreEqual(map.height, 2, "Map height is incorrect. Expected=2 Actual=" + map.height);
        }

        /// <summary>
        /// Tests that the GetTile(x,y) method returns the correct Tile.
        /// </summary>
        [TestMethod]
        public void GetTilesTest()
        {
            Map map = Map.GenerateMap(possibleTiles, 5, 5, 1);

            for(int x = 0; x < map.width; x++)
            {
                for(int y = 0; y < map.height; y++)
                {
                    Assert.AreEqual(map.GetTile(x, y), possibleTiles[map.tiles[x, y]], "Map tile ("+x+","+y+") wasn't consistent with expected value. Expected="+ possibleTiles[map.tiles[x, y]].name + " Actual=" + map.GetTile(x, y).name);
                }
            }
        }

        /// <summary>
        /// Tests that the Collides method correctly detects collisions.
        /// </summary>
        [TestMethod]
        public void TestCollides()
        {
            Map map = Map.GenerateMap(possibleTiles, 6, 6, 2);

            List<Entity>[,] entitiesBySquare = new List<Entity>[6,6];
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    entitiesBySquare[i, j] = new List<Entity>();

            UnitType defaultType = new UnitType("testUnitType", 1, 1, 1, 1, 1, new int[] { 0, 0, 0 }, 1, 1, "", false, 1, "", "", 0, 0, new int[] { 0, 0, 0, 0 }, "");
            entitiesBySquare[1, 1].Add(new Unit(defaultType, 1, 1, 1, 0));
            entitiesBySquare[3, 4].Add(new Unit(defaultType, 2, 3, 4, 0));
            Unit unit = new Unit(defaultType, 3, 1.5, 4.5, 0);
            entitiesBySquare[1, 4].Add(unit);
            entitiesBySquare[2, 4].Add(unit);
            entitiesBySquare[1, 5].Add(unit);
            entitiesBySquare[2, 5].Add(unit);

            Assert.IsTrue(map.Collides(0.5, 0.5, 1, entitiesBySquare, true) != null, "There should be a collision at (0.5,0.5) with size 1.");
            Assert.IsTrue(map.Collides(0, 0, 1.5, entitiesBySquare, true) != null, "There should be a collision at (0,0) with size 1.5.");
            Assert.IsTrue(map.Collides(3.45, 4.45, 0.1, entitiesBySquare, true) != null, "There should be a collision at (3,4) with size 0.1.");
            Assert.IsTrue(map.Collides(2, 3, 3, entitiesBySquare, true) != null, "There should be a collision at (2,3) with size 3.");
            Assert.IsTrue(map.Collides(3.9,4.9,1, entitiesBySquare, true) != null, "There should be a collision at (3.9,4.9) with size 1.");

            Assert.IsTrue(map.Collides(0, 0, 0.5, entitiesBySquare, true) == null, "There should not be a collision at (0,0) with size 0.5.");
            Assert.IsTrue(map.Collides(3, 1, 0.5, entitiesBySquare, true) == null, "There should not be a collision at (3,1) with size 0.5.");
        }
    }
}
