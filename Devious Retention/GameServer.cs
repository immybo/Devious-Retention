using System;
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
        private List<Dictionary<string, Technology>> researched;
        
        private World world;

        // What units have been commanded to move
        private List<Unit> movingUnits;
        // What entities have been commanded to attack
        private List<Unit> attackingUnits;
        private List<Building> attackingBuildings;

        private Player[] players;

        /// <summary>
        /// To create a GameServer, all clients (STCConnections) must be provided.
       ///  Note that the GameServer treats every
        /// game as multiplayer, and so computer controlled players will merely
        /// spoof having a connection.
        /// </summary>
        public GameServer(List<STCConnection> connections, int[] aiNumbers, World world)
        {
            if (connections != null)
                this.connections = connections;
            else
                this.connections = new List<STCConnection>();

            this.world = world;

            // Init players and relations
            players = new Player[connections.Count + aiNumbers.Length];

            Player.Relation[] defaultRelations = new Player.Relation[players.Length];
            for(int i = 0; i < defaultRelations.Length; i++)
            {
                defaultRelations[i] = Player.Relation.ENEMY;
            }

            for(int i = 0; i < players.Length; i++)
            {
                players[i] = new Player(Player.DefaultRelations(i, players.Length), i, GameInfo.PLAYER_COLORS[i],
                                        null, new GameInfo());
            }

            info = new GameInfo();

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
        /// Tells all clients to update their map to the current one on the server.
        /// </summary>
        public void SyncMap()
        {
            foreach(STCConnection c in connections)
            {
                c.InformMap(world.Map);
            }
        }

        /// <summary>
        /// Attempts to place a foundation of the given building type for the given player at (x,y).
        /// Does nothing if no foundation could be placed there, or if the building type couldn't be found
        /// </summary>
        public void CreateBuilding(int player, string buildingTypeName, double x, double y)
        {
            if (!info.buildingTypes.ContainsKey(buildingTypeName)) return;

            BuildingType buildingType = info.buildingTypes[buildingTypeName];
            Building building = new Building(buildingType, x, y, players[player]);

            // Make sure that the building doesn't collide with any other entities
            if (world.EntityCollisions(building).Count() > 0) return;
            world.AddEntity(building);

            // No collisions, so we can safetly place the building :)
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
            // TODO better space checks on creating unit from building
            if (!world.ContainsBuilding(buildingId)) return;
            if (!info.unitTypes.ContainsKey(unitTypeName)) return;

            Building building = world.GetBuilding(buildingId);
            UnitType type = info.unitTypes[unitTypeName];

            double placeX = -1;
            double placeY = -1;
            // Try and see if there's space anywhere around the building
            double x = building.X - type.size;
            double y = building.Y - type.size - 0.1;
            // On top
            for (x = building.X - type.size - 0.1; x <= building.X + building.Type.size + 0.1; x += 0.1)
                if (!world.Collides(x,y,type)){ placeX = x; placeY = y; }
            // On the right
            x = building.X + building.Type.size + 0.1;
            if(placeX == -1)
                for(y = building.Y - type.size - 0.1; y <= building.Y + building.Type.size + 0.1; y += 0.1)
                    if (!world.Collides(x, y, type)) { placeX = x; placeY = y; }
            // On the bottom
            y = building.Y + building.Type.size + 0.1;
            if(placeX == -1)
                for(x = building.X + building.Type.size + 0.1; x >= building.X - type.size - 0.1; x -= 0.1)
                    if (!world.Collides(x, y, type)) { placeX = x; placeY = y; }
            // On the left
            x = building.X - type.size - 0.1;
            if(placeX == -1)
                for(y = building.Y + building.Type.size + 0.1; y >= building.Y - type.size - 0.1; y -= 0.1)
                    if (!world.Collides(x, y, type)) { placeX = x; placeY = y; }

            // Was there a place for it?
            if (placeX == -1)
                return;
            // Now place it
            Unit unit = new Unit(type, placeX, placeY, building.Player);
            
            world.AddEntity(unit);
            foreach (STCConnection c in connections)
                c.InformEntityAdd(unit, false);
        }

        /// <summary>
        /// Creates an entity and informs all clients.
        /// </summary>
        public void SpawnEntity(EntityType type, int player, double x, double y)
        {
            Entity entity = null;

            if (type is UnitType)
                entity = new Unit((UnitType)type, x, y, players[player]);
            else if(type is BuildingType)
                entity = new Building((BuildingType)type, x, y, players[player]);
            else if(type is ResourceType)
                entity = new Resource((ResourceType)type, x, y);

            world.AddEntity(entity);
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
            if (!world.ContainsUnit(unitID))
                throw new ArgumentException("Unit with invalid ID " + unitID + " commanded to move.");

            Unit unit = world.GetUnit(unitID);
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
                if (!world.ContainsUnit(entityID)) return;
                entity = world.GetUnit(entityID);
            }
            else if(entityType == 1)
            {
                if (!world.ContainsBuilding(entityID)) return;
                entity = world.GetBuilding(entityID);
            }
            else
            {
                if (!world.ContainsResource(entityID)) return;
                entity = world.GetResource(entityID);
            }
            DeleteEntity(entity);
        }
        /// <summary>
        /// Removes given entity from the
        /// list of entities, and informs the clients.
        /// Does nothing if the given entity does not exist in the server's
        /// lists of entities.
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteEntity(Entity entity)
        {
            if (entity is Unit)
            {
                if (!world.ContainsUnit(entity.ID)) return;
                world.RemoveEntity(world.GetUnit(entity.ID));

                if (attackingUnits.Contains((Unit)entity)) // Stop attacking with it if necessary
                    StopEntityAttack(entity);
            }
            else if (entity is Building)
            {
                if (!world.ContainsBuilding(entity.ID)) return;
                world.RemoveEntity(world.GetBuilding(entity.ID));

                if (attackingBuildings.Contains((Building)entity)) // Stop attacking with it if necessary
                    StopEntityAttack(entity);
            }
            else
            {
                if (!world.ContainsResource(entity.ID)) return;
                world.RemoveEntity(world.GetResource(entity.ID));
            }

            // Also stop whatever is attacking it from attacking it, if necessary
            List<Entity> entitiesToStop = new List<Entity>();
            foreach (Unit u in attackingUnits)
                if (u.entityToAttack.Equals(entity))
                    entitiesToStop.Add(u);
            foreach (Building b in attackingBuildings)
                if (b.entityToAttack.Equals(entity))
                    entitiesToStop.Add(b);
            foreach (Entity e in entitiesToStop)
                StopEntityAttack(e);

            // Update both the clients and entities by square
            foreach (STCConnection c in connections)
                c.InformEntityDeletion(entity is Unit ? 0 : entity is Building ? 1 : 2, entity.ID);
        }

        public void GatherResource(double amount, int resourceID)
        {
            if (!world.ContainsResource(resourceID)) return;
            Resource resource = world.GetResource(resourceID);
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
                    attackerUnits.Add(world.GetUnit(attackerIds[i]));
                else
                    attackerBuildings.Add(world.GetBuilding(attackerIds[i]));
            }

            Entity defender = defenderType == 0 ? (Entity)world.GetUnit(defenderId) : (Entity)world.GetBuilding(defenderId);

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
        /// Stops the given unit from moving wherever it's moving.
        /// </summary>
        private void StopUnitMovement(Unit unit)
        {
            movingUnits.Remove(unit);
        }

        /// <summary>
        /// Moves all units which have been commanded to move by one tick's worth of movement.
        /// </summary>
        private void MoveAllUnits()
        {
            // TODO Collision using quad-trees
            List<Unit> toRemove = new List<Unit>();
            foreach (Unit u in movingUnits)
            {
                if (u.xToMove == u.X && u.yToMove == u.Y) { toRemove.Add(u); continue; }

                double dX = 0;
                double dY = 0;

                if (u.xToMove >= u.X + u.Type.speed / 1000 * GameInfo.TICK_TIME) dX = u.Type.speed / 1000 * GameInfo.TICK_TIME;
                else if (u.xToMove <= u.X - u.Type.speed / 1000 * GameInfo.TICK_TIME) dX = -u.Type.speed / 1000 * GameInfo.TICK_TIME;
                else dX = u.xToMove - u.X;

                if (u.yToMove >= u.Y + u.Type.speed / 1000 * GameInfo.TICK_TIME) dY = u.Type.speed / 1000 * GameInfo.TICK_TIME;
                else if (u.yToMove <= u.Y - u.Type.speed / 1000 * GameInfo.TICK_TIME) dY = -u.Type.speed / 1000 * GameInfo.TICK_TIME;
                else dY = u.yToMove - u.Y;

                // Figure out if we actually can move through there
                // Because we want to be simple, just check the up to four squares that the unit will end up being partially in
                Coordinate[] endSquares = new Coordinate[4];
                endSquares[0] = new Coordinate((int)(u.X+ dX), (int)(u.Y+ dY));
                endSquares[1] = new Coordinate((int)(u.X+ dX), (int)(u.Y + dY + u.Type.size));
                endSquares[2] = new Coordinate((int)(u.X + dX + u.Type.size), (int)(u.Y + dY));
                endSquares[3] = new Coordinate((int)(u.X + dX + u.Type.size), (int)(u.Y + dY + u.Type.size));

                for(int i = 0; i < 4; i++)
                {
                    // Make sure the tile is actually on the map (movement will be blocked if it isn't anyway)
                    if (endSquares[i].x < 0 || endSquares[i].y < 0 || endSquares[i].x >= world.MapSize().x || endSquares[i].y >= world.MapSize().y) continue;
                    
                    // If it does collide, just stop its movement.
                    if (world.Collides(endSquares[i].x, endSquares[i].y, u.Type))
                    {
                        toRemove.Add(u);
                        dX = 0;
                        dY = 0;
                    }
                }
                
                u.ChangePosition(dX, dY);
                foreach (STCConnection c in connections)
                    c.InformEntityChange(u, 1, dX, dY);
            }

            foreach (Unit u in toRemove)
                StopUnitMovement(u);
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
                if((u.entityToAttack is Unit && !world.ContainsUnit(u.entityToAttack.ID)) ||
                    (u.entityToAttack is Building && !world.ContainsBuilding(u.entityToAttack.ID)))
                {
                    toRemoveUnits.Add(u);
                    continue;
                }

                if (!defenders.Contains(u.entityToAttack))
                    defenders.Add(u.entityToAttack);

                double distance = Math.Sqrt(Math.Pow(u.X - u.entityToAttack.X, 2) + Math.Pow(u.Y - u.entityToAttack.Y, 2));
                // If any units have also been commanded to move, check if they're now within range
                if (movingUnits.Contains(u))
                {
                    if(distance <= u.Type.range)
                    {
                        movingUnits.Remove(u);
                    }
                }

                // Otherwise, command units which are not within range to move to the defender's position
                // They will stop once they get close enough due to the above command
                if(distance > u.Type.range && !movingUnits.Contains(u))
                {
                    u.xToMove = u.entityToAttack.X;
                    u.yToMove = u.entityToAttack.Y;
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
                            damage = ((Unit)u.entityToAttack).TakeDamage(u.Type.damage, u.Type.damageType);
                        else
                            damage = ((Building)u.entityToAttack).TakeDamage(u.Type.damage, u.Type.damageType);

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
                if ((b.entityToAttack is Unit && !world.ContainsUnit(b.entityToAttack.ID)) ||
                    (b.entityToAttack is Building && !world.ContainsBuilding(b.entityToAttack.ID)))
                {
                    toRemoveBuildings.Add(b);
                    continue;
                }

                if (!defenders.Contains(b.entityToAttack))
                    defenders.Add(b.entityToAttack);

                double distance = Math.Sqrt(Math.Pow(b.X - b.entityToAttack.X, 2) + Math.Pow(b.Y - b.entityToAttack.Y, 2));
                // Tick the attack cycle of those which are within range
                if (distance <= b.Type.range)
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
                            damage = ((Unit)b.entityToAttack).TakeDamage(b.Type.damage, b.Type.damageType);
                        else
                            damage = ((Building)b.entityToAttack).TakeDamage(b.Type.damage, b.Type.damageType);

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
                    DeleteEntity(e);
                }
                else if(e is Building && ((Building)e).hitpoints <= 0)
                {
                    DeleteEntity(e);
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
