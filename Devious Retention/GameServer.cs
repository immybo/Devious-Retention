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
            AttackAllEntities(); // Attack all the entities that need attacking

            world.Tick();

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
            if(unit.Attacking()) unit.HaltAttacking();
            unit.BeginMovement(x, y);
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
                entity.Kill();
            }
            else if (entity is Building)
            {
                if (!world.ContainsBuilding(entity.ID)) return;
                world.RemoveEntity(world.GetBuilding(entity.ID));
                entity.Kill();
            }
            else
            {
                if (!world.ContainsResource(entity.ID)) return;
                world.RemoveEntity(world.GetResource(entity.ID));
            }

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
            List<Entity> attackers = new List<Entity>();
            for(int i = 0; i < attackerTypes.Count; i++)
            {
                if (attackerTypes[i] == 0)
                    attackers.Add(world.GetUnit(attackerIds[i]));
                else
                    attackers.Add(world.GetBuilding(attackerIds[i]));
            }

            Entity defender = defenderType == 0 ? (Entity)world.GetUnit(defenderId) : (Entity)world.GetBuilding(defenderId);

            foreach(Entity e in attackers)
            {
                if(e.CanMove())
                    e.HaltMovement();
                e.BeginAttacking(defender);

                foreach (STCConnection c in connections)
                    c.InformEntityAttack(e, defender, true);
            }
        }

        /// <summary>
        /// Stops the given entity from attacking whatever defender it is attacking.
        /// </summary>
        private void StopEntityAttack(Entity attacker)
        {
            attacker.HaltAttacking();

            foreach (STCConnection c in connections)
                c.InformEntityAttack(attacker, null, false);
        }

        /// <summary>
        /// Stops the given unit from moving wherever it's moving.
        /// </summary>
        private void StopUnitMovement(Unit unit)
        {
            unit.HaltMovement();
        }

        /// <summary>
        /// Moves all units which have been commanded to attack forward by one tick in the attack cycle.
        /// </summary>
        private void AttackAllEntities()
        {
            List<Tuple<Entity, Entity>> combatants = new List<Tuple<Entity, Entity>>();
            foreach (Unit u in world.GetUnits())
                if (u.Attacking())
                    combatants.Add(new Tuple<Entity, Entity>(u, u.AttackedEntity()));
            foreach (Building b in world.GetBuildings())
                if (b.Attacking())
                    combatants.Add(new Tuple<Entity, Entity>(b, b.AttackedEntity()));
            
            foreach(Tuple<Entity,Entity> fight in combatants)
            {
                int damage = Entity.GetDamage(fight.Item1, fight.Item2);

                foreach (STCConnection c in connections)
                {
                    c.InformEntityChange(fight.Item2, 0, -damage, 0);
                }
            }
        }
    }
}
