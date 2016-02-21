﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    /// <summary>
    /// There is one GameServer per game, whether the game is
    /// single- or multi-player. The GameServer relays actions
    /// to different clients, and receives actions from them
    /// to process.
    /// </summary>
    class GameServer
    {
        public List<STCConnection> connections { get; set; }
        private GameInfo info;
        private int currentTick;
        
        // One map for every player, contains only technologies that they have researched
        private List<Dictionary<String, Technology>> researched;

        // This map is used to determine what can and can't happen; e.g. whether or not a building can be placed at the given location
        private Map map;

        // Lists of all entities in the game
        private Dictionary<int, Unit> units;
        private Dictionary<int, Building> buildings;
        private Dictionary<int, Resource> resources;

        // Which entities are where; if at least part of an entity is on a square, it will be recorded in that square's list
        private List<Entity>[,] entitiesBySquare;

        // What units have been commanded to move
        private List<Unit> movingUnits;
        // What entities have been commanded to attack
        private List<Unit> attackingUnits;
        private List<Building> attackingBuildings;

        /// <summary>
        /// To create a GameServer, all clients (STCConnections) must be provided.
       ///  Note that the GameServer treats every
        /// game as multiplayer, and so computer controlled players will merely
        /// spoof having a connection.
        /// </summary>
        public GameServer(List<STCConnection> connections, Map map)
        {
            if (connections != null)
                this.connections = connections;
            else
                this.connections = new List<STCConnection>();

            this.map = map;

            info = new GameInfo();

            units = new Dictionary<int, Unit>();
            buildings = new Dictionary<int, Building>();
            resources = new Dictionary<int, Resource>();
            entitiesBySquare = new List<Entity>[map.width,map.height];
            for (int i = 0; i < map.width; i++)
                for (int j = 0; j < map.height; j++)
                    entitiesBySquare[i, j] = new List<Entity>();

            researched = new List<Dictionary<String, Technology>>();
            for (int i = 0; i < this.connections.Count; i++)
                researched.Add(new Dictionary<String, Technology>());

            movingUnits = new List<Unit>();
            attackingUnits = new List<Unit>();
            attackingBuildings = new List<Building>();

            Timer tickTimer = new Timer();
            tickTimer.Interval = GameInfo.TICK_TIME;
            tickTimer.Tick += Tick;
            tickTimer.Start();
            currentTick = 0;
        }

        /// <summary>
        /// Processes a tick
        /// </summary>
        private void Tick(object sender, EventArgs e)
        {
            MoveAllUnits(); // Move all the units that need moving
            AttackAllEntities(); // Attack all the entities that need attacking

            foreach (STCConnection c in connections) // Inform all the clients
                c.Tick();

            currentTick++;
        }

        /// <summary>
        /// Attempts to place a foundation of the given building type for the given player at (x,y).
        /// Does nothing if no foundation could be placed there, or if the building type couldn't be found
        /// </summary>
        public void CreateBuilding(int player, string buildingTypeName, double x, double y)
        {
            if (!info.buildingTypes.ContainsKey(buildingTypeName)) return;
            BuildingType buildingType = info.buildingTypes[buildingTypeName];

            Building building = new Building(buildingType, Building.nextID, x, y, player);
            Building.IncrementNextID();
            // Make sure that the building doesn't collide with any other entities
            if (Map.Collides(building.x, building.y, buildingType.size, map, entitiesBySquare, false) == null) return;

            buildings.Add(building.id, building);
            List<Coordinate> buildingCoordinates = Map.GetIncludedTiles(map, building);
            foreach (Coordinate c in buildingCoordinates)
            {
                if (entitiesBySquare[c.x, c.y] == null) entitiesBySquare[c.x, c.y] = new List<Entity> { building };
                else
                {
                    // Also see if there are any resource that this is built on
                    if (buildingType.canBeBuiltOnResource)
                        foreach(Entity e in entitiesBySquare[c.x, c.y])
                            if (e is Resource && ((Resource)e).resourceType.resourceType == buildingType.builtOnResourceType)
                                building.resource = (Resource)e;

                    entitiesBySquare[c.x, c.y].Add(building);
                }
            }

            // No collisions, so we can safetly place the foundation :)
            foreach (STCConnection c in connections)
                c.InformEntityAdd(building, false);
        }

        /// <summary>
        /// Attempts to create a unit of the given type from the given building.
        /// Does nothing if no space could be found around the building,
        /// or if the building or unit type couldn't be found.
        /// </summary>
        public void CreateUnit(int buildingId, string unitTypeName)
        {
            if (!buildings.ContainsKey(buildingId)) return;
            if (!info.unitTypes.ContainsKey(unitTypeName)) return;

            Building building = buildings[buildingId];
            UnitType type = info.unitTypes[unitTypeName];

            double placeX = -1;
            double placeY = -1;
            // Try and see if there's space anywhere around the building
            double x = building.x - type.size;
            double y = building.y - type.size - 0.1;
            // On top
            for (x = building.x - type.size - 0.1; x <= building.x + building.type.size + 0.1; x += 0.1)
                if (Map.Collides(x, y, type.size, map, entitiesBySquare, true) != null){ placeX = x; placeY = y; }
            // On the right
            x = building.x + building.type.size + 0.1;
            if(placeX == -1)
                for(y = building.y - type.size - 0.1; y <= building.y + building.type.size + 0.1; y += 0.1)
                    if (Map.Collides(x, y, type.size, map, entitiesBySquare, true) != null){ placeX = x; placeY = y; }
            // On the bottom
            y = building.y + building.type.size + 0.1;
            if(placeX == -1)
                for(x = building.x + building.type.size + 0.1; x >= building.x - type.size - 0.1; x -= 0.1)
                    if (Map.Collides(x, y, type.size, map, entitiesBySquare, true) != null){ placeX = x; placeY = y; }
            // On the left
            x = building.x - type.size - 0.1;
            if(placeX == -1)
                for(y = building.y + building.type.size + 0.1; y >= building.y - type.size - 0.1; y -= 0.1)
                    if (Map.Collides(x, y, type.size, map, entitiesBySquare, true) != null){ placeX = x; placeY = y; }

            // Was there a place for it?
            if (placeX == -1)
                return;
            // Now place it
            Unit unit = new Unit(type, Unit.nextID, placeX, placeY, building.playerNumber);
            Unit.IncrementNextID();

            foreach (STCConnection c in connections)
                c.InformEntityAdd(unit, false);

            units.Add(unit.id, unit);
            List<Coordinate> unitCoordinates = Map.GetIncludedTiles(map, unit);
            foreach (Coordinate c in unitCoordinates)
                entitiesBySquare[c.x, c.y].Add(unit);
        }

        /// <summary>
        /// Creates an entity and informs all clients.
        /// </summary>
        public void SpawnEntity(EntityType type, int player, double x, double y)
        {
            Entity entity = null;

            if (type is UnitType)
            {
                entity = new Unit((UnitType)type, Unit.nextID, x, y, player);
                Unit.IncrementNextID();
                units.Add(entity.id, (Unit)entity);
            }
            else if(type is BuildingType)
            {
                entity = new Building((BuildingType)type, Building.nextID, x, y, player);
                Building.IncrementNextID();
                buildings.Add(entity.id, (Building)entity);
            }
            else if(type is ResourceType)
            {
                entity = new Resource((ResourceType)type, Resource.nextID, x, y);
                Resource.IncrementNextID();
                resources.Add(entity.id, (Resource)entity);
            }

            // Make sure to add it to the list of entities by coordinate as well as the regular entity dictionaries
            List<Coordinate> entityCoordinates = Map.GetIncludedTiles(map, entity);
            foreach (Coordinate c in entityCoordinates)
                entitiesBySquare[c.x, c.y].Add(entity);

            if (entity == null) return;
            foreach (STCConnection c in connections)
                c.InformEntityAdd(entity, true);
        }

        /// <summary>
        /// Adds the technology with the given name to the list of technologies that the
        /// given player has researched. This can't fail (unless something screwed up and the
        /// technology wasn't found)
        /// </summary>
        public void ResearchTechnology(int player, string technologyName)
        {
            researched[player-1].Add(technologyName, info.technologies[technologyName]);
            foreach (STCConnection c in connections)
                c.InformTechnologyResearch(player, info.technologies[technologyName]);
        }

        /// <summary>
        /// Gives the given unit a command to move to (x,y).
        /// </summary>
        public void CommandUnitToMove(int unitID, double x, double y)
        {
            if (!units.ContainsKey(unitID)) return;
            Unit unit = units[unitID];
            // If we're attacking with that unit, halt the attack
            if (attackingUnits.Contains(unit)) StopEntityAttack(unit);
            if(!movingUnits.Contains(unit)) movingUnits.Add(unit);
            unit.xToMove = x;
            unit.yToMove = y;
        }

        /// <summary>
        /// Removes the entity of the given type with the given ID from the
        /// list of entities, and informs the clients.
        /// Does nothing if the given entity does not exist in the server's
        /// lists of entities.
        /// </summary>
        public void DeleteEntity(int entityType, int entityID)
        {
            Entity entity;
            if(entityType == 0)
            {
                if (!units.ContainsKey(entityID)) return;
                entity = units[entityID];
                units.Remove(entityID);

                if (attackingUnits.Contains((Unit)entity)) // Stop attacking with it if necessary
                    StopEntityAttack(entity);
            }
            else if(entityType == 1)
            {
                if (!buildings.ContainsKey(entityID)) return;
                entity = buildings[entityID];
                buildings.Remove(entityID);

                if (attackingBuildings.Contains((Building)entity)) // Stop attacking with it if necessary
                    StopEntityAttack(entity);
            }
            else
            {
                if (!resources.ContainsKey(entityID)) return;
                entity = resources[entityID];
                resources.Remove(entityID);
            }

            // Update both the clients and entities by square
            foreach (STCConnection c in connections)
                c.InformEntityDeletion(entityType, entityID);
            foreach (Coordinate c in Map.GetIncludedTiles(map, entity))
                entitiesBySquare[c.x, c.y].Remove(entity);
        }

        public void GatherResource(double amount, int resourceID)
        {
            if (!resources.ContainsKey(resourceID)) return;
            Resource resource = resources[resourceID];
            foreach (STCConnection c in connections)
                c.InformEntityChange(resource, 0, -amount, 0);
        }

        /// <summary>
        /// Sets the given attackers to attack the given defender, or move towards it if they are
        /// not in range (or do nothing if they're a building and not within range).
        /// </summary>
        public void AttackEntity(int defenderType, int defenderId, List<int> attackerTypes, List<int> attackerIds)
        {
            // First, sort them into units and buildings
            List<Unit> attackerUnits = new List<Unit>();
            List<Building> attackerBuildings = new List<Building>();
            for(int i = 0; i < attackerTypes.Count; i++)
            {
                if (attackerTypes[i] == 0)
                    attackerUnits.Add(units[attackerIds[i]]);
                else
                    attackerBuildings.Add(buildings[attackerIds[i]]);
            }

            Entity defender = defenderType == 0 ? (Entity)units[defenderId] : (Entity)buildings[defenderId];

            foreach(Unit u in attackerUnits)
            {
                u.entityToAttack = defender;

                if(movingUnits.Contains(u))
                    movingUnits.Remove(u);
                if(!attackingUnits.Contains(u))
                    attackingUnits.Add(u);

                foreach (STCConnection c in connections)
                    c.InformEntityAttack(u, defender, true);
            }
            foreach(Building b in attackerBuildings)
            {
                b.entityToAttack = defender;
                if(!attackingBuildings.Contains(b))
                    attackingBuildings.Add(b);

                foreach (STCConnection c in connections)
                    c.InformEntityAttack(b, defender, true);
            }
        }

        /// <summary>
        /// Stops the given entity from attacking whatever defender it is attacking.
        /// </summary>
        private void StopEntityAttack(Entity attacker)
        {
            Entity defender = null;
            if(attacker is Unit)
            {
                attackingUnits.Remove((Unit)attacker);
                defender = ((Unit)attacker).entityToAttack;
            }
            else if(attacker is Building)
            {
                attackingBuildings.Remove((Building)attacker);
                defender = ((Building)attacker).entityToAttack;
            }

            foreach (STCConnection c in connections)
                c.InformEntityAttack(attacker, defender, false);
        }

        /// <summary>
        /// Moves all units which have been commanded to move by one tick's worth of movement.
        /// </summary>
        private void MoveAllUnits()
        {
            List<Unit> toRemove = new List<Unit>();
            foreach (Unit u in movingUnits)
            {
                if (u.xToMove == u.x && u.yToMove == u.y) { toRemove.Add(u); continue; }

                double dX = 0;
                double dY = 0;

                if (u.xToMove >= u.x + u.type.speed / 1000 * GameInfo.TICK_TIME) dX = u.type.speed / 1000 * GameInfo.TICK_TIME;
                else if (u.xToMove <= u.x - u.type.speed / 1000 * GameInfo.TICK_TIME) dX = -u.type.speed / 1000 * GameInfo.TICK_TIME;
                else dX = u.xToMove - u.x;

                if (u.yToMove >= u.y + u.type.speed / 1000 * GameInfo.TICK_TIME) dY = u.type.speed / 1000 * GameInfo.TICK_TIME;
                else if (u.yToMove <= u.y - u.type.speed / 1000 * GameInfo.TICK_TIME) dY = -u.type.speed / 1000 * GameInfo.TICK_TIME;
                else dY = u.yToMove - u.y;

                // Update the list of entities by square
                List<Coordinate> previousCoords = Map.GetIncludedTiles(map, u);
                List<Coordinate> coordsToRemove = new List<Coordinate>();

                u.x += dX;
                u.y += dY;

                List<Coordinate> newCoords = Map.GetIncludedTiles(map, u);

                foreach (Coordinate c in previousCoords)
                    if (!newCoords.Contains(c))
                        coordsToRemove.Add(c);
                    else
                        newCoords.Remove(c);

                foreach (Coordinate c in newCoords)
                    entitiesBySquare[c.x, c.y].Add(u);


                foreach (STCConnection c in connections)
                    c.InformEntityChange(u, 1, dX, dY);
            }

            foreach (Unit u in toRemove)
                movingUnits.Remove(u);
        }

        /// <summary>
        /// Moves all units which have been commanded to attack forward by one tick in the attack cycle.
        /// </summary>
        private void AttackAllEntities()
        {
            List<Entity> defenders = new List<Entity>();

            List<Unit> toRemoveUnits = new List<Unit>();
            List<Building> toRemoveBuildings = new List<Building>();
            // Units first
            foreach(Unit u in attackingUnits)
            {
                // Make sure the defender isn't dead yet
                if((u.entityToAttack is Unit && !units.ContainsValue((Unit)u.entityToAttack)) ||
                    (u.entityToAttack is Building && !buildings.ContainsValue((Building)u.entityToAttack)))
                {
                    toRemoveUnits.Add(u);
                    continue;
                }

                if (!defenders.Contains(u.entityToAttack))
                    defenders.Add(u.entityToAttack);

                double distance = Math.Sqrt(Math.Pow(u.x - u.entityToAttack.x, 2) + Math.Pow(u.y - u.entityToAttack.y, 2));
                // If any units have also been commanded to move, check if they're now within range
                if (movingUnits.Contains(u))
                {
                    if(distance <= u.type.range)
                    {
                        movingUnits.Remove(u);
                    }
                }

                // Otherwise, command units which are not within range to move to the defender's position
                // They will stop once they get close enough due to the above command
                if(distance > u.type.range && !movingUnits.Contains(u))
                {
                    u.xToMove = u.entityToAttack.x;
                    u.yToMove = u.entityToAttack.y;
                    movingUnits.Add(u);
                }

                // Tick the attack cycle of those which are within range
                else
                {
                    // If it hasn't reached the final phase of its attack cycle, increment it by one tick
                    if(u.attackTick < u.unitType.attackTicks)
                    {
                        u.attackTick++;
                    }
                    // Otherwise, attack with it and reset the counter
                    else
                    {
                        u.attackTick = 0;
                        double damage = 0;

                        // Damage it
                        if (u.entityToAttack is Unit)
                            damage = ((Unit)u.entityToAttack).TakeDamage(u.type.damage, u.type.damageType);
                        else
                            damage = ((Building)u.entityToAttack).TakeDamage(u.type.damage, u.type.damageType);

                        foreach(STCConnection c in connections)
                        {
                            c.InformEntityChange(u.entityToAttack, 0, -damage, 0);
                        }
                    }
                }
            }
            // Then buildings
            foreach (Building b in attackingBuildings)
            {
                // Make sure the defender isn't dead yet
                if ((b.entityToAttack is Unit && !units.ContainsValue((Unit)b.entityToAttack)) ||
                    (b.entityToAttack is Building && !buildings.ContainsValue((Building)b.entityToAttack)))
                {
                    toRemoveBuildings.Add(b);
                    continue;
                }

                if (!defenders.Contains(b.entityToAttack))
                    defenders.Add(b.entityToAttack);

                double distance = Math.Sqrt(Math.Pow(b.x - b.entityToAttack.x, 2) + Math.Pow(b.y - b.entityToAttack.y, 2));
                // Tick the attack cycle of those which are within range
                if (distance <= b.type.range)
                {
                    // If it hasn't reached the final phase of its attack cycle, increment it by one tick
                    if (b.attackTick < b.buildingType.attackTicks)
                    {
                        b.attackTick++;
                    }
                    // Otherwise, attack with it and reset the counter
                    else
                    {
                        b.attackTick = 0;
                        double damage = 0;
                        if (b.entityToAttack is Unit)
                            damage = ((Unit)b.entityToAttack).TakeDamage(b.type.damage, b.type.damageType);
                        else
                            damage = ((Building)b.entityToAttack).TakeDamage(b.type.damage, b.type.damageType);

                        foreach (STCConnection c in connections)
                        {
                            c.InformEntityChange(b.entityToAttack, 0, -damage, 0);
                        }
                    }
                }
            }

            // Check which defenders are dead
            foreach(Entity e in defenders)
            {
                if(e is Unit && ((Unit)e).hitpoints <= 0)
                {
                    units.Remove(e.id);
                    foreach(STCConnection c in connections)
                    {
                        c.InformEntityDeletion(0, e.id);
                    }
                }
                else if(e is Building && ((Building)e).hitpoints <= 0)
                {
                    buildings.Remove(e.id);
                    foreach(STCConnection c in connections)
                    {
                        c.InformEntityDeletion(1, e.id);
                    }
                }
            }

            // Stop attacking with attackers where the defender has died
            foreach (Unit u in toRemoveUnits)
                attackingUnits.Remove(u);
            foreach (Building b in toRemoveBuildings)
                attackingBuildings.Remove(b);
        }
    }
}
