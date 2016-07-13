using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Each player represents a game client running on a machine.
    /// </summary>
    public class Player
    {
        // TODO players owning entities

        // Silly sounding names but they make sense...
        public int Number { get; private set; }
        public Color Color { get; private set; }
        public Brush Brush { get; private set; }
        public Pen Pen { get; private set; }

        private Faction faction;
        // Every player must have a separate set of definitions,
        // as they'll be changed by technologies and faction.
        public GameInfo Definitions { get; private set; }
        private Relation[] playerRelations;

        public Player(Relation[] baseRelations, int number, Color color, Faction faction, GameInfo definitions)
        {
            playerRelations = baseRelations;
            Number = number;
            Color = color;
            this.faction = faction;
            this.Definitions = definitions;
            Brush = new SolidBrush(color);
            Pen = new Pen(Brush);
        }

        /// <summary>
        /// Returns whether or not this player is the owner of
        /// the given entity.
        /// </summary>
        public bool Owns(Entity entity)
        {
            return entity.Player == this;
        }

        /// <summary>
        /// Generates and returns an array of entities which are
        /// enemies to this player, and are contained within
        /// the given array.
        /// </summary>
        public Entity[] GetEnemies(Entity[] entities)
        {
            List<Entity> enemies = new List<Entity>();
            foreach (Entity entity in entities)
                if (IsEnemy(entity))
                    enemies.Add(entity);
            return enemies.ToArray();
        }

        /// <summary>
        /// Finds out and returns whether or not the given entity is
        /// an enemy of this player.
        /// </summary>
        private bool IsEnemy(Entity entity)
        {
            return entity.Attackable() && GetRelation(entity.Player.Number) == Relation.ENEMY;
        }

        /// <summary>
        /// Returns The relation which this player has to the player
        /// with the given number.
        /// </summary>
        private Relation GetRelation(int playerNumber)
        {
            if (playerNumber < 0 || playerNumber >= playerRelations.Length)
                throw new ArgumentOutOfRangeException("Attempting to find relation with player #" + playerNumber + ", who does not exist.");
            return playerRelations[playerNumber];
        }

        public static Relation[] DefaultRelations(int playerNumber, int playerCount)
        {
            Relation[] relations = new Relation[playerCount];
            for (int i = 0; i < playerCount; i++)
                relations[i] = Relation.ENEMY;
            relations[playerNumber] = Relation.ALLIED;
            return relations;
        }

        public enum Relation
        {
            ALLIED,
            ENEMY
        }
    }

    /// <summary>
    /// A player which represents the local game client.
    /// We don't really need to know most things about remote players.
    /// </summary>
    public class LocalPlayer : Player
    {
        private World world;
        private bool[,] lineOfSight;

        // How many of each resource the player currently has
        // Resources are handled entirely client-side
        // metal, oil, energy, science
        private double[] currentResources;

        public LocalPlayer(Relation[] baseRelations, int number, Color color, Faction faction, GameInfo definitions, World world)
            : base(baseRelations, number, color, faction, definitions)
        {
            this.world = world;
            currentResources = new double[GameInfo.RESOURCE_TYPES];
        }

        /// <summary>
        /// Returns whether or not the player can afford all
        /// of the given resource costs.
        /// </summary>
        public bool CanAfford(int[] resourceCosts)
        {
            if (resourceCosts.Length != currentResources.Length)
                throw new ArgumentException("Not the right amount of resources passed to CanAfford. Passed: "
                    + resourceCosts.Length + " Actual: " + currentResources.Length);

            for (int i = 0; i < resourceCosts.Length; i++)
                if (resourceCosts[i] > currentResources[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Deducts the given amounts of resources from this
        /// player's stockpiles, disregarding whether or not
        /// the player can actually afford it.
        /// </summary>
        public void PayResources(int[] resourceCosts)
        {
            if (resourceCosts.Length != currentResources.Length)
                throw new ArgumentException("Not the right amount of resources passed to PayResources. Passed: "
                    + resourceCosts.Length + " Actual: " + currentResources.Length);

            for (int i = 0; i < resourceCosts.Length; i++)
                currentResources[i] -= resourceCosts[i];
        }
        /// <summary>
        /// Deducts the resource costs of the give nentity
        /// from this player's stockpiles, disregarding whether
        /// or not the player can actually afford it.
        /// </summary>
        public void PayResources(Entity entity)
        {
            PayResources(entity.Type.resourceCosts);
        }

        public void AddResource(int resourceIndex, double amount)
        {
            if (resourceIndex >= currentResources.Length || resourceIndex < 0)
                throw new ArgumentOutOfRangeException("Invalid resource index passed to add resource. Index: " + resourceIndex);
            currentResources[resourceIndex] += amount;
        }

        public double GetResource(int resourceIndex)
        {
            if (resourceIndex >= currentResources.Length || resourceIndex < 0)
                throw new ArgumentException("Invalid resource index given to GetResource. Index: " +
                                            resourceIndex + " Maximum: " + (currentResources.Length - 1));
            return currentResources[resourceIndex];
        }
        
        /// <summary>
        /// Updates the player's line of sight given that the
        /// given entity was just created.
        /// Assumes that this entity belongs to the player.
        /// Does nothing if this entity is a resource.
        /// </summary>
        public void UpdateLOSAdd(Entity e)
        {
            // TODO FIX LOS....
            // TODO Optimise LOS calculations

            // Resources don't have LOS
            if (e is Resource) return;

            int entityLOS = e.Type.lineOfSight;
            // Just round it down for simplicity
            int entityX = (int)(e.X + e.Type.size / 2);
            int entityY = (int)(e.Y + e.Type.size / 2);

            // Simple way of figuring out a circle
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    // Are we even on the map?
                    if (world.OutOfBounds(x, y)) continue;

                    // Find the distance from the entity (pythagoras)
                    int distance = (int)(Math.Sqrt(Math.Pow(entityX - x, 2) + Math.Pow(entityY - y, 2)));
                    // Do nothing if it's too far away
                    if (distance > entityLOS) continue;

                    // Otherwise add this square to LOS
                    lineOfSight[x, y] = true;
                }
            }
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given unit has just moved by (dX,dY).
        /// </summary>
        public void UpdateLOSMove(Unit unit, double dX, double dY)
        {
            if (!Owns(unit)) return;

            // The new LOS of the unit
            List<Coordinate> newTiles = new List<Coordinate>();
            // The old LOS of the unit
            List<Coordinate> oldTiles = new List<Coordinate>();

            // Figure out the old circle
            int oldUnitX = (int)(unit.X + unit.unitType.size / 2 - dX);
            int oldUnitY = (int)(unit.Y + unit.unitType.size / 2 - dY);

            for (int x = oldUnitX - unit.unitType.lineOfSight; x <= oldUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = oldUnitY - unit.unitType.lineOfSight; y <= oldUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(oldUnitX - x, 2) + Math.Pow(oldUnitY - y, 2)));
                    if (distance > unit.unitType.lineOfSight) continue;

                    // This is one of the tiles that the unit used to be able to see
                    oldTiles.Add(new Coordinate(x, y));
                }
            }

            // Figure out the new circle
            int newUnitX = (int)(unit.X + unit.unitType.size / 2);
            int newUnitY = (int)(unit.Y + unit.unitType.size / 2);
            for (int x = newUnitX - unit.unitType.lineOfSight; x <= newUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = newUnitY - unit.unitType.lineOfSight; y <= newUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(newUnitX - x, 2) + Math.Pow(newUnitY - y, 2)));
                    if (distance > unit.unitType.lineOfSight) continue;

                    // This is one of the tiles that the unit can now see
                    newTiles.Add(new Coordinate(x, y));
                }
            }

            // The tiles that it can't see any more
            List<Coordinate> nowInvisibleTiles = new List<Coordinate>();
            // The tiles that it couldn't see before but can now
            List<Coordinate> nowVisibleTiles = new List<Coordinate>();

            // Add tiles to the list of tiles that we can't see any more... only if we can't see them any more
            foreach (Coordinate oldTile in oldTiles)
                if (!newTiles.Contains(oldTile))
                    nowInvisibleTiles.Add(oldTile);
            // Add tiles to the list of tiles that we can see now, only if we couldn't see them before
            foreach (Coordinate newTile in newTiles)
                if (!oldTiles.Contains(newTile))
                    nowVisibleTiles.Add(newTile);

            // Set all the newly visible tiles to be within LOS
            foreach (Coordinate c in nowVisibleTiles)
            {
                if (world.OutOfBounds(c)) continue;
                lineOfSight[c.x, c.y] = true;
            }

            // And check if we can still see the old tiles
            foreach (Coordinate c in nowInvisibleTiles)
                lineOfSight[c.x, c.y] = HasLOSTo(c);
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given entity was just deleted.
        /// </summary>
        public void UpdateLOSDelete(Entity entity)
        {
            if (entity is Resource) return;
            if (!Owns(entity)) return;

            int entityLOS = entity.Type.lineOfSight;
            int entityX = (int)(entity.X + entity.Type.size / 2);
            int entityY = (int)(entity.Y + entity.Type.size / 2);
            // Go through all the tiles the entity could see and recheck if we can still see them
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(entityX - x, 2) + Math.Pow(entityY - y, 2)));
                    if (distance > entityLOS) continue;

                    // Check whether or not we can still see this tile
                    lineOfSight[x, y] = HasLOSTo(new Coordinate(x, y));
                }
            }
        }

        /// <summary>
        /// Returns whether or not the player has line of sight to
        /// the given coordinate.
        /// </summary>
        public bool HasLOSTo(Coordinate c)
        {
            // Scroll through units and buildings that belong to the player, and figure out which are within range
            // Stop if we find one that is
            HashSet<Entity> entities = new HashSet<Entity>();
            foreach (Unit u in world.GetUnits())
                if (Owns(u))
                    entities.Add(u);

            foreach (Building b in world.GetBuildings())
                if (Owns(b))
                    entities.Add(b);

            foreach (Entity e in entities)
            {
                // Distance between the entity and the tile
                double distance = Math.Sqrt(Math.Pow(e.X - c.x, 2) + Math.Pow(e.Y - c.y, 2));
                if (distance <= e.Type.lineOfSight) return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to figure out the player's line of sight, and stores it in "LOS"
        /// </summary>
        public void LoadLOS()
        {
            // Clear the current LOS
            lineOfSight = new bool[world.MapSize().x, world.MapSize().y];

            List<Entity> entities = new List<Entity>();
            foreach (Unit u in world.GetUnits())
                if (Owns(u))
                    entities.Add(u);
            foreach (Building b in world.GetBuildings())
                if (Owns(b))
                    entities.Add(b);

            foreach (Entity e in entities)
            {
                int entityLOS = e.Type.lineOfSight;
                // Just round it down for simplicity
                int entityX = (int)(e.X + e.Type.size / 2);
                int entityY = (int)(e.Y + e.Type.size / 2);

                // Simple way of figuring out a circle
                for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
                {
                    for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                    {
                        // Are we even on the map?
                        if (x < 0 || y < 0) continue;
                        if (x >= world.MapSize().x || y >= world.MapSize().y) continue;

                        // Find the distance from the entity (pythagoras)
                        int distance = (int)(Math.Sqrt(Math.Pow(entityX - x, 2) + Math.Pow(entityY - y, 2)));
                        // Do nothing if it's too far away
                        if (distance > entityLOS) continue;

                        // Otherwise add this square to LOS
                        lineOfSight[x, y] = true;
                    }
                }
            }
        }
    }
}
