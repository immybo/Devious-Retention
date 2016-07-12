using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// The world and all of its contents.
    /// </summary>
    public class World
    {
        private Dictionary<int, Resource> resources;
        private Dictionary<int, Unit> units;
        private Dictionary<int, Building> buildings;
        // Which entities are where; if at least part of an entity is on a square, it will be recorded in that square's list
        private List<Entity>[,] entitiesBySquare;
        private Map map;

        /// <summary>
        /// Creates a new world with the given map,
        /// however with no entities on it.
        /// </summary>
        public World(Map map)
        {
            resources = new Dictionary<int, Resource>();
            units = new Dictionary<int, Unit>();
            buildings = new Dictionary<int, Building>();

            this.map = map;
            entitiesBySquare = new List<Entity>[map.width, map.height];
        }

        /// <summary>
        /// Returns whether or not the given world coordinates
        /// are out of the bounds of this world.
        /// </summary>
        public bool OutOfBounds(double x, double y)
        {
            if (x >= map.width || x < 0) return true;
            if (y >= map.height || y < 0) return true;
            return false;
        }
        /// <summary>
        /// Returns whether or not any point within the rectangle
        /// defined by (x, y, x+size, y+size) is out of bounds
        /// of the world.
        /// </summary>
        public bool OutOfBounds(double x, double y, double size)
        {
            if (OutOfBounds(x - size, y - size)) return false;
            if (OutOfBounds(x - size, y + size)) return false;
            if (OutOfBounds(x + size, y + size)) return false;
            if (OutOfBounds(x + size, y - size)) return false;
            return true;
        }
        public bool OutOfBounds(Coordinate coord)
        {
            return OutOfBounds(coord.x, coord.y);
        }

        /// <summary>
        /// Returns whether or not it would be valid for a building of
        /// the given type to be placed at the given coordinate.
        /// </summary>
        public bool ValidBuildingPlacement(BuildingType b, double x, double y)
        {
            if (OutOfBounds(x, y, b.size))
                return false;
            if (EntityCollisions(x, y, b.size).Length > 0)
                return false;
            if (TileCollisions(x, y, b).Length > 0)
                return false;
            return true;
        }

        /// <summary>
        /// Returns an array of all entities which a solid square object
        /// at (x,y) with the given size would collide with.
        /// </summary>
        /// <returns></returns>
        public Entity[] EntityCollisions(double x, double y, double size)
        {
            return null;
        }

        /// <summary>
        /// Returns an array of all coordinates of tiles with which an entity
        /// of the given type at the given position would collide.
        /// </summary>
        /// <returns></returns>
        public Coordinate[] TileCollisions(double x, double y, EntityType type)
        {
            return null;
        }

        /// <summary>
        /// Finds an entity at the given world coordinates.
        /// Priority will be given to entities in this order:
        /// 1. Units
        /// 2. Buildings
        /// 3. Resources
        /// If there is more than one entity in the same category
        /// which qualifies as being at the given coordinates,
        /// no guarantee is made as to which one will be chosen.
        /// 
        /// If there are no entities at the given position,
        /// or if the given position is out of bounds, this
        /// method will return null.
        /// </summary>
        public Entity GetEntityAt(double x, double y)
        {
            if (OutOfBounds(x, y)) return null;
            
            // TODO this is why we need a better system (e.g. quad tree)
            foreach (Entity e in units.Values)
                if (EntityIntersectsPoint(e, x, y))
                    return e;
            foreach (Entity e in buildings.Values)
                if (EntityIntersectsPoint(e, x, y))
                    return e;
            foreach (Entity e in resources.Values)
                if (EntityIntersectsPoint(e, x, y))
                    return e;

            return null;
        }

        /// <summary>
        /// Finds all entities within a rectangle defined by the given
        /// world coordinates, and returns a set of them.
        /// If there are no such entities, returns an empty set.
        /// </summary>
        public Entity[] GetEntitiesIn(double left, double top, double width, double height)
        {
            HashSet<Entity> entities = new HashSet<Entity>();

            foreach (Entity e in units.Values)
                entities.Add(e);
            foreach (Entity e in buildings.Values)
                entities.Add(e);
            foreach (Entity e in resources.Values)
                entities.Add(e);

            HashSet<Entity> enclosedEntities = new HashSet<Entity>();
            foreach (Entity e in entities)
            {
                if (EntityWithinArea(e, left, top, width, height))
                {
                    enclosedEntities.Add(e);
                }
            }

            return enclosedEntities.ToArray();
        }
        public Coordinate[] GetIncludedTiles(double x, double y, double size)
        {
            return map.GetIncludedTiles(x, y, size).ToArray();
        }
        
        private bool EntityIntersectsPoint(Entity e, double x, double y)
        {
            return e.X + e.Type.size > x && e.X < x
                && e.Y + e.Type.size > y && e.Y < y;
        }
        private bool EntityWithinArea(Entity e, double x, double y, double width, double height)
        {
            return e.X + e.Type.size >= x && e.X <= x + width 
                && e.Y + e.Type.size >= y && e.Y <= y + height;
        }

        // TODO ideally some of these should only be used internally
        public void AddEntity(Resource resource)
        {
            resources[resource.ID] = resource;
            // TODO update entities by square
        }
        public void AddEntity(Unit unit)
        {
            units[unit.ID] = unit;
        }
        public void AddEntity(Building building)
        {
            buildings[building.ID] = building;
        }
        public bool ContainsResource(int resourceID)
        {
            return resources.ContainsKey(resourceID);
        }
        public bool ContainsUnit(int unitID)
        {
            return units.ContainsKey(unitID);
        }
        public bool ContainsBuilding(int buildingID)
        {
            return buildings.ContainsKey(buildingID);
        }
        public Resource GetResource(int resourceID)
        {
            return resources[resourceID];
        }
        public Unit GetUnit(int unitID)
        {
            return units[unitID];
        }
        public Building GetBuilding(int buildingID)
        {
            return buildings[buildingID];
        }
        public Building[] GetBuildings()
        {
            return buildings.Values.ToArray();
        }
        public Unit[] GetUnits()
        {
            return units.Values.ToArray();
        }
        public Resource[] GetResources()
        {
            return resources.Values.ToArray();
        }

        public void RemoveEntity(Entity entity)
        {
            // TODO remove from entities by square as well
            if (resources.ContainsKey(entity.ID))
                resources.Remove(entity.ID);
            else if (units.ContainsKey(entity.ID))
                units.Remove(entity.ID);
            else if (buildings.ContainsKey(entity.ID))
                buildings.Remove(entity.ID);
            else
                throw new KeyNotFoundException("Non-existant entity with ID " + entity.ID + " attempted to be removed.");
        }

        /// <summary>
        /// Returns a coordinate representing the size of the map.
        /// </summary>
        public Coordinate MapSize()
        {
            return new Coordinate(map.width, map.height);
        }
        /// <summary>
        /// Returns the type of tile which is at the given coordinate.
        /// </summary>
        public Tile GetTile(Coordinate coord)
        {
            return map.GetTile(coord.x, coord.y);
        }
        public Tile GetTile(int x, int y)
        {
            return map.GetTile(x, y);
        }

        public void SetMap(Map newMap)
        {
            map = newMap;
        }
    }
}
