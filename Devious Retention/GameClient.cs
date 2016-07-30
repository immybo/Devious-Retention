using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    /// <summary>
    /// There is one GameClient for every player or computer-controlled player
    /// in a game. The client, in combination with the GameWindow, allows the
    /// player to control their side of the game.
    /// </summary>
    public class GameClient
    {
        // TODO Music/sounds

        private World world;
        
        private LocalPlayer player;
        private Player[] players;

        public CTSConnection connection { get; set; }
        // Each player's GameInfo is changed over time due to technologies, factions, etc
        public List<GameInfo> definitions { get; private set; }
        public GameInfo info { get { return GetLocalDefinitions(); } private set { } } // This client's GameInfo (= definitions[playerNumber])

        // Doesn't need to be stored on the server, this is just used to determine time passing between events
        private int currentTick;

        // TODO Possibly migrate selected to window
        public List<Entity> selected;

        private GameWindow window;

        // Whether the building panel or the technology panel is open
        public bool buildingPanelOpen { get; private set; }

        
        /// <summary>
        /// When a GameClient is created, it is assumed that entities will be
        /// sent from the CTSConnection before the first tick.
        /// </summary>
        public GameClient(LocalPlayer player, Player[] players, World world, CTSConnection connection)
        {
            this.connection = connection;
            this.world = world;
            this.player = player;
            player.LoadLOS();
            this.players = players;

            selected = new List<Entity>();
            currentTick = 0;

            buildingPanelOpen = true;

            definitions = new List<GameInfo>();
            
            window = new GameWindow(world, player, this);

            Timer windowRefreshTimer = new Timer();
            windowRefreshTimer.Interval = GameInfo.WINDOW_REFRESH_TIME;
            windowRefreshTimer.Tick += new EventHandler(WindowRefreshTimerHandler);
            windowRefreshTimer.Start();
        }

        public GameInfo GetLocalDefinitions()
        {
            return player.Definitions;
        }
        
        /// <returns>The main window of the game.</returns>
        public Form GetWindow()
        {
            return window;
        }

        /// <summary>
        /// Updates the map to the given map.
        /// </summary>
        public void UpdateMap(Map newMap)
        {
            world.SetMap(newMap);
            player.LoadLOS();
        }

        /// <summary>
        /// Asks the server to move selected units to (x,y)
        /// </summary>
        public void MoveSelectedUnits(double x, double y)
        {
            // TODO change to specify which units to move, rather than just using selected
            List<Unit> selectedUnits = new List<Unit>();
            foreach (Entity e in selected)
                if (e is Unit && player.Owns(e))
                    selectedUnits.Add((Unit)e);
            
            // We can check here if it would go off the map
            foreach (Unit unit in selectedUnits)
            {
                // TODO reachability, find path to nearest reachable tile
                double adjustedX = x - unit.Type.size / 2;
                double adjustedY = y - unit.Type.size / 2;

                if (adjustedX < 0) adjustedX = 0;
                if (adjustedY < 0) adjustedY = 0;

                if (x + unit.Type.size/2 >= world.MapSize().x) adjustedX = world.MapSize().x - unit.Type.size/2;
                if (y + unit.Type.size/2 >= world.MapSize().y) adjustedY = world.MapSize().y - unit.Type.size/2;

                connection.RequestMove(unit, adjustedX, adjustedY);
            }
        }

        /// <summary>
        /// Tells the server to attack the given entity with the given entities.
        /// Entities which can't attack won't.
        /// </summary>
        /// <param name="e"></param>
        public void AttackEntity(Entity[] attackers, Entity entityToAttack)
        {
            Entity[] enemies = entityToAttack.Player.GetEnemies(attackers);
            // TODO refactor attackentity to take origin entities as well
            // Figure out which of the selected entities belong to the player
            List<Entity> available = new List<Entity>();
            foreach (Entity e in selected)
                if (player.Owns(e))
                    available.Add(e);

            connection.RequestAttack(available, entityToAttack);
        }

        public void AttackEntityWithSelected(Entity entityToAttack)
        {
            AttackEntity(selected.ToArray(), entityToAttack);
        }

        /// <summary>
        /// Attempts to create a building foundation of the given type
        /// at the given position. Does nothing if the player doesn't have enough resources.
        /// Returns whether or not the player has enough resources.
        /// 
        /// Does not yet remove the resources; only removes them when confirmation
        /// is received from the server that the foundation was created.
        /// </summary>
        public bool CreateFoundation(BuildingType building, double x, double y)
        {
            if (!player.CanAfford(building.resourceCosts))
                return false;
            connection.RequestBuilding(building, x, y);
            return true;
        }

        /// <summary>
        /// Attempts to create a unit from the given building.
        /// Returns whether or not the client has enough resources
        /// for this and the building can create that UnitType.
        /// 
        /// Does not yet remove the resources; only removes them when confirmation
        /// is received from the server that the unit was created.
        /// </summary>
        public bool CreateUnit(Building sourceBuilding, UnitType unit)
        {
            if (!sourceBuilding.buildingType.trainableUnits.Contains(unit.name)) return false;
            if (!player.CanAfford(unit.resourceCosts))
                return false;

            connection.RequestUnit(sourceBuilding, unit);
            return true;
        }

        /// <summary>
        /// Attempts to research the given technology.
        /// Returns whether or not the client has enough resources
        /// for this and the prerequisites are met.
        /// </summary>
        public bool ResearchTechnology(Technology technology)
        {
            // Make sure we have the prerequisites
            foreach (string s in technology.prerequisites)
                if (!info.technologies[s].researched)
                    return false;
            // And that we don't have clashing technologies
            foreach (string s in technology.clashing)
                if (info.technologies[s].researched)
                    return false;
            // And enough resources
            if (!player.CanAfford(technology.resourceCosts))
                return false;

            // Otherwise remove the resources and tell the server
            player.PayResources(technology.resourceCosts);
            connection.RequestTechnology(technology);

            return true;
        }

        /// <summary>
        /// Attempts to delete the currently selected unit or building,
        /// or the first one in the list of selected entities.
        /// Does nothing if no units or buildings that belong to
        /// the player are selected.
        /// </summary>
        public void DeleteSelected()
        {
            if (selected.Count == 0) return; // Do nothing if there aren't any selected units
            // Otherwise scroll through all the entities that are selected and find one that fits the criteria
            foreach(Entity e in selected)
            {
                if (player.Owns(e))
                {
                    connection.RequestDelete(e);
                    return;
                }
            }
            // If there aren't any do nothing
        }

        /// <summary>
        /// Creates a new entity from the parameters and spawns it into the game world.
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="type">The name of the type of entity to be created.</param>
        /// <param name="xPos">The initial x position of the new entity.</param>
        /// <param name="yPos">The initial y position of the new entity.</param>
        /// <param name="playerNumber">The player that the entity belongs to. Irrelevant if a resource.</param>
        /// <param name="resourceID">The resource which the entity is built on, only relevant if it's a building.</param>
        /// <returns>The entity that was spawned.</returns>
        public Entity SpawnEntity(int entityType, int id, string type, double xPos, double yPos, int playerNumber, int resourceID)
        {
            // TODO revamp definitions system so there's a central list of all definitions with a boolean attached for each player and a list of modifiers
            if (entityType == 0)
            {
                if (!players[playerNumber].Definitions.unitTypes.ContainsKey(type))
                    throw new ArgumentException("Invalid unit type " + type + " given when adding entity.");
                UnitType unitType = players[playerNumber].Definitions.unitTypes[type];
                Unit unit = new Unit(unitType, id, xPos, yPos, players[playerNumber]);
                world.AddEntity(unit);
                player.UpdateLOSAdd(unit);
                return unit;
            }
            else if (entityType == 1)
            {
                if (!players[playerNumber].Definitions.buildingTypes.ContainsKey(type))
                    throw new ArgumentException("Invalid building type " + type + " given when adding entity.");
                BuildingType buildingType = players[playerNumber].Definitions.buildingTypes[type];
                Building building = new Building(buildingType, id, xPos, yPos, players[playerNumber]);
                if (world.ContainsResource(resourceID)) building.resource = world.GetResource(resourceID);
                world.AddEntity(building);
                player.UpdateLOSAdd(building);
                return building;
            }
            else if (entityType == 2)
            {
                if (!players[playerNumber].Definitions.resourceTypes.ContainsKey(type))
                    throw new ArgumentException("Invalid resource type " + type + " given when adding entity.");
                ResourceType resourceType = players[playerNumber].Definitions.resourceTypes[type];
                Resource resource = new Resource(resourceType, id, xPos, yPos);
                world.AddEntity(resource);
                return resource;
            }

            throw new ArgumentException("Attempted to add an entity with an entity type that is out of range.");
        }

        /// <summary>
        /// Wrapper for SpawnEntity that also removes resources appropriately.
        /// </summary>
        public void AddEntity(int entityType, int id, string type, double xPos, double yPos, int playerNumber, int resourceID)
        {
            Entity entity = SpawnEntity(entityType, id, type, xPos, yPos, playerNumber, resourceID);
            if (player.Owns(entity))
            {
                player.PayResources(entity);
            }
        }

        /// <summary>
        /// Removes an entity.
        /// Throws an exception if the entity can't be found.
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="deletedEntityID">The ID of the entity to be deleted.</param>
        public void DeleteEntity(int entityType, int deletedEntityID)
        {
            Entity entity = null;

            // TODO factor out using switch case for type of entity
            if (entityType == 0)
            {
                if (!world.ContainsUnit(deletedEntityID))
                    throw new KeyNotFoundException("There exists no unit with ID " + deletedEntityID + ", but one was attempted to be deleted.");
                Unit unit = world.GetUnit(deletedEntityID);
                unit.Kill();
                entity = unit;
                unit.unitType.units.Remove(unit);
                player.UpdateLOSDelete(entity); // Make sure we update the line of sight of the player
            }
            else if (entityType == 1)
            {
                if (!world.ContainsBuilding(deletedEntityID))
                    throw new KeyNotFoundException("There exists no building with ID " + deletedEntityID + ", but one was attempted to be deleted.");
                Building building = world.GetBuilding(deletedEntityID);
                building.Kill();
                entity = building;
                building.buildingType.buildings.Remove(building);
                player.UpdateLOSDelete(entity);
            }
            else if (entityType == 2)
            {
                if (!world.ContainsResource(deletedEntityID))
                    throw new KeyNotFoundException("There exists no resource with ID " + deletedEntityID + ", but one was attempted to be deleted.");
                entity = world.GetResource(deletedEntityID);
            }

            world.RemoveEntity(entity);

            // Remove it from the selected entities if it was in there
            if (selected.Contains(entity)) selected.Remove(entity);
        }

        /// <summary>
        /// Changes one property of an entity, e.g. its position,
        /// hitpoints, built status..
        /// If that unit isn't found, throws an exception.
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="entityID">The ID of the entity to be changed. Nothing will happen if no entity with this ID exists.</param>
        /// <param name="propertyID">The ID of the property to be changed. Nothing will happen if this is invalid.</param>
        /// <param name="change">The modifier to the property.</param>
        public void ChangeEntityProperty(int entityType, int entityID, int propertyID, double change, double change2)
        {
            if (entityType == 0)
            {
                if (!world.ContainsUnit(entityID))
                    throw new KeyNotFoundException("There exists no unit with ID " + entityID + ", but the property of one was attempted to be changed.");
                Unit unit = world.GetUnit(entityID);

                if (propertyID == 0) unit.hitpoints += (int)change;
                else if (propertyID == 1)
                {
                    unit.ChangePosition(change, change2);
                    if (player.Owns(unit)) player.UpdateLOSMove(unit, change, change2);
                }
                else if (propertyID == 2)
                {
                    if ((int)change == 0) unit.attackTick++;
                    else unit.attackTick = 0;
                }
                else if (propertyID == 3)
                {
                    if ((int)change == 1) unit.movementTick++;
                    else unit.movementTick = 0;
                }
            }
            else if (entityType == 1)
            {
                if (!world.ContainsBuilding(entityID))
                    throw new KeyNotFoundException("There exists no building with ID " + entityID + ", but the property of one was attempted to be changed.");
                Building building = world.GetBuilding(entityID);

                if (propertyID == 0) building.hitpoints += (int)change;
                else if (propertyID == 1)
                {
                    if ((int)change == 0) building.attackTick++;
                    else building.attackTick = 0;
                }
            }
            else if (entityType == 2)
            {
                if (!world.ContainsResource(entityID))
                    throw new KeyNotFoundException("There exists no resource with ID " + entityID + ", but the property of one was attempted to be changed.");
                Resource resource = world.GetResource(entityID);

                if (propertyID == 0) resource.amount += change;
            }
        }

        /// <summary>
        /// Sets the technology with the given name's research state to be TRUE for the given player.
        /// </summary>
        public void SetTechnologyResearched(int playerNumber, string technologyName)
        {
            if (!players[playerNumber].Definitions.technologies.ContainsKey(technologyName)) return;
            Technology technology = players[playerNumber].Definitions.technologies[technologyName];
            technology.researched = true;
            technology.ApplyEffects(players[playerNumber].Definitions);
        }

        /// <summary>
        /// Either starts or stops (based on starting) the attack animation between two entities
        /// with the given types and IDs.
        /// </summary>
        public void AnimateAttack(bool starting, int attackerType, int attackerId, int defenderType, int defenderId)
        {
            // First, find the actual entities
            Entity attacker = attackerType == 0 ? (Entity)world.GetUnit(attackerId) : (Entity)world.GetBuilding(attackerId);
            Entity defender = starting ? defenderType == 0 ? (Entity)world.GetUnit(defenderId) : (Entity)world.GetBuilding(defenderId) : null;

            if (!attacker.CanAttack())
                throw new InvalidOperationException("Trying to attack with an entity which can't attack (" + attacker.Type.name + ").");
            if (!defender.Attackable())
                throw new InvalidOperationException("Trying to attack an entity which can't be attacked (" + defender.Type.name + ").");

            if (starting)
                attacker.BeginAttacking(defender);
            else
                attacker.HaltAttacking();
        }

        /// <summary>
        /// Ends the game from this client's perspective.
        /// </summary>
        /// <param name="won">Whether or not this player won the game.</param>
        public void EndGame(bool won)
        {
            // TODO Game ending
        }

        /// <summary>
        /// Updates everything by one tick.
        /// </summary>
        public void Tick()
        {
            currentTick++;
            TickResourceGathering();
        }

        /// <summary>
        /// Gathers one second's worth of resources from each building 
        /// that is gathering resources
        /// </summary>
        private void TickResourceGathering()
        {
            foreach(Building b in world.GetBuildings())
            {
                // Only tick if it is going to provide a resource
                if(b.buildingType.providesResource)
                {
                    player.AddResource(b.buildingType.resourceType, b.buildingType.gatherSpeed);
                }
                else if(b.buildingType.canBeBuiltOnResource && b.resource != null)
                {
                    if (!b.resource.Depleted())
                    {
                        double amount = 0;
                        if (b.resource.amount >= b.resource.resourceType.gatherSpeed)
                            amount = b.resource.resourceType.gatherSpeed;
                        else
                            amount = b.resource.amount;

                        player.AddResource(b.buildingType.builtOnResourceType, amount);

                        connection.InformResourceGather(amount, b.resource);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the game window.
        /// </summary>
        private void WindowRefreshTimerHandler(object source, EventArgs e)
        {
            window.Refresh();
        }

        /// <summary>
        /// Returns whether or not the player is currently allowed
        /// to build a building of the type with the given name.
        /// </summary>
        public bool CanBuild(BuildingType type)
        {
            // Do we have its prerequisite, if it has any?
            if(type.prerequisite != null && (!info.technologies.ContainsKey(type.prerequisite) || !info.technologies[type.prerequisite].researched))
            {
                return false;
            }

            // Otherwise it can be built
            return true;
        }

        /// <summary>
        /// Returns whether or not the player is currently allowed
        /// to research the technology with the given name.
        /// </summary>
        public bool CanResearch(Technology tech)
        {
            // If it's already researched, we can't do so again
            if (tech.researched) return false;
            // If any of the prerequisites isn't researched, we can't research it
            foreach(string s in tech.prerequisites)
                if (!info.technologies.ContainsKey(s) || !info.technologies[s].researched)
                    return false;
            // If any of the clashing technologies is researched, we can't research it
            foreach (string s in tech.clashing)
                if (info.technologies.ContainsKey(s) && info.technologies[s].researched)
                    return false;

            // Otherwise, we can research it
            return true; 
        }
    }
}
