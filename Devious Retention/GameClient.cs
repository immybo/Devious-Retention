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

        public CTSConnection connection { get; set; }
        // Each player's GameInfo is changed over time due to technologies, factions, etc
        public List<GameInfo> definitions { get; private set; }
        public GameInfo info { get; private set; } // This client's GameInfo (= definitions[playerNumber])

        // Doesn't need to be stored on the server, this is just used to determine time passing between events
        private int currentTick;

        // TODO Possibly migrate selected to window
        public List<Entity> selected;

        private GameWindow window;

        // This should be unique within a given game
        public int playerNumber { get; private set; }
        public Color playerColor { get; private set; }

        // How many of each resource the player currently has
        // Resources are handled entirely client-side
        // metal, oil, energy, science
        public double[] currentResources { get; private set; }

        // Entities which are currently attacking anything, for the purposes of animation and projectiles
        public List<Unit> attackingUnits { get; private set; }
        public List<Building> attackingBuildings { get; private set; }

        // Whether the building panel or the technology panel is open
        public bool buildingPanelOpen { get; private set; }

        // The faction this player belongs to
        private Faction faction;

        // The player numbers of players in this player's team
        private List<int> teammates;
        
        /// <summary>
        /// When a GameClient is created, it is assumed that entities will be
        /// sent from the CTSConnection before the first tick.
        /// </summary>
        public GameClient(int playerNumber, int numberOfPlayers, World gameWorld, CTSConnection connection, List<Faction> factions)
        {
            //this.faction = faction;
            this.connection = connection;
            this.playerNumber = playerNumber;
            playerColor = GameInfo.PLAYER_COLORS[playerNumber];

            this.world = gameWorld;

            selected = new List<Entity>();

            currentResources = new double[GameInfo.RESOURCE_TYPES];
            currentTick = 0;

            teammates = new List<int> { playerNumber };

            buildingPanelOpen = true;

            Unit.ResetNextID();
            Building.ResetNextID();
            Resource.ResetNextID();

            attackingUnits = new List<Unit>();
            attackingBuildings = new List<Building>();

            definitions = new List<GameInfo>();
            for (int i = 0; i < numberOfPlayers; i++)
                definitions.Add(new GameInfo());
            info = definitions[playerNumber];
            
            window = new GameWindow(world, this);

            Timer windowRefreshTimer = new Timer();
            windowRefreshTimer.Interval = GameInfo.WINDOW_REFRESH_TIME;
            windowRefreshTimer.Tick += new EventHandler(WindowRefreshTimerHandler);
            windowRefreshTimer.Start();

            for (int i = 0; i < currentResources.Length; i++)
                currentResources[i] += 1000;
        }
        
        /// <returns>The main window of the game.</returns>
        public Form GetWindow()
        {
            return window;
        }

        /// <summary>
        /// Processes a right click, given that there is at least one
        /// selected entity, at the given location; attacking or moving
        /// the units.
        /// </summary>
        public void RightClick(double x, double y)
        {
            // TODO Possibly migrate to window

            // If the right click is out of bounds, do nothing
            if (world.OutOfBounds(x, y)) return;

            // If there's an enemy there, attack it
            Entity[] overlappingEntities = world.GetEntitiesIn(x, y, 0, 0);
            Entity[] enemies = GetEnemies(overlappingEntities);
            if(enemies.Length > 0)
            {
                // Pick any random entity; no guarantee is made as to the order
                AttackEntity(enemies[0]);
            }

            // Otherwise, move the units
            else
            {
                MoveUnits(x, y);
            }
        }

        // TODO have player and team classes, check if enemy in there
        /// <summary>
        /// Generates and returns an array of entities which claim that
        /// they are enemies to this player, and  are contained within
        /// the given array.
        /// </summary>
        private Entity[] GetEnemies(Entity[] entities)
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
            return entity.Attackable() && !IsAllied(entity.PlayerNumber);
        }

        /// <summary>
        /// Returns whether or not the given player number is an ally
        /// to this player. Also returns true if the given player number
        /// is equal to this player's player number.
        /// </summary>
        private bool IsAllied(int playerNumber)
        {
            // TODO teams
            return this.playerNumber == playerNumber;
        }

        /// <summary>
        /// Asks the server to move selected units to (x,y)
        /// </summary>
        public void MoveUnits(double x, double y)
        {
            // TODO change to specify which units to move, rather than just using selected
            List<Unit> selectedUnits = new List<Unit>();
            foreach (Entity e in selected)
                if (e is Unit)
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
        /// Tells the server to attack the given entity with all available
        /// selected entities.
        /// Assumes that the given entity
        /// - Isn't a resource
        /// - Isn't allied with the player
        /// Does, however, check which of the selected entities are available
        /// to attack the entity.
        /// </summary>
        /// <param name="e"></param>
        public void AttackEntity(Entity entityToAttack)
        {
            // TODO refactor attackentity to take origin entities as well
            // Figure out which of the selected entities belong to the player
            List<Entity> available = new List<Entity>();
            foreach (Entity e in selected)
                if (e.PlayerNumber == playerNumber)
                    available.Add(e);

            connection.RequestAttack(available, entityToAttack);
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
            for (int i = 0; i < currentResources.Length; i++)
                if (currentResources[i] < building.resourceCosts[i])
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
            // And that we have enough resources
            for (int i = 0; i < currentResources.Length; i++)
                if (currentResources[i] < unit.resourceCosts[i])
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
            for(int i = 0; i < currentResources.Length; i++)
                if (currentResources[i] < technology.resourceCosts[i])
                    return false;

            // Otherwise remove the resources and tell the server
            for (int i = 0; i < currentResources.Length; i++)
                currentResources[i] -= technology.resourceCosts[i];
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
                if ((e is Unit || e is Building) && e.PlayerNumber == playerNumber)
                {
                    connection.RequestDelete(e);
                    return;
                }
            }
            // If there aren't any do nothing
        }

        /// <summary>
        /// Adds an entity. Does nothing if the entity type isn't found.
        /// </summary>
        /// <param name="isFree">Whether or not this entity doesn't cost resources.</param>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="type">The name of the type of entity to be created.</param>
        /// <param name="xPos">The initial x position of the new entity.</param>
        /// <param name="yPos">The initial y position of the new entity.</param>
        /// <param name="player">The player that the entity belongs to. Irrelevant if a resource.</param>
        /// <param name="resource">The resource which the entity is built on, only relevant if it's a building.</param>
        public void AddEntity(bool isFree, int entityType, int id, string type, double xPos, double yPos, int player, int resourceID)
        {
            Entity entity = null;
            if(entityType == 0)
            {
                if (!definitions[playerNumber].unitTypes.ContainsKey(type)) return; // do nothing if the unit type isn't found
                UnitType unitType = definitions[playerNumber].unitTypes[type];
                Unit unit = new Unit(unitType, id, xPos, yPos, player);
                world.AddEntity(unit);
                unitType.units.Add(unit);
                if(player == playerNumber)
                    window.UpdateLOSAdd(unit);
                entity = unit;

                // If the unit belongs to the player, remove the resources as well
                if (unit.PlayerNumber == playerNumber && !isFree)
                    for (int i = 0; i < currentResources.Length; i++)
                        currentResources[i] -= unit.unitType.resourceCosts[i];
            }
            else if(entityType == 1)
            {
                if (!definitions[playerNumber].buildingTypes.ContainsKey(type)) return; // do nothing if the building type isn't found
                BuildingType buildingType = definitions[playerNumber].buildingTypes[type];
                Building building = new Building(buildingType, id, xPos, yPos, player);
                if(world.ContainsResource(resourceID)) building.resource = world.GetResource(resourceID);
                world.AddEntity(building);
                buildingType.buildings.Add(building);
                if (player == playerNumber)
                    window.UpdateLOSAdd(building);
                entity = building;

                // If the building belongs to the player, remove the resources as well
                if (building.PlayerNumber == playerNumber && !isFree)
                    for (int i = 0; i < currentResources.Length; i++)
                        currentResources[i] -= building.buildingType.resourceCosts[i];
            }
            else if(entityType == 2)
            {
                if (!definitions[playerNumber].resourceTypes.ContainsKey(type)) return; // do nothing if the resource type isn't found
                ResourceType resourceType = definitions[playerNumber].resourceTypes[type];
                Resource resource = new Resource(resourceType, id, xPos, yPos);
                world.AddEntity(resource);
                entity = resource;
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
                entity = unit;
                unit.unitType.units.Remove(unit);
                window.UpdateLOSDelete(entity); // Make sure we update the line of sight of the player
            }
            else if (entityType == 1)
            {
                if (!world.ContainsBuilding(deletedEntityID))
                    throw new KeyNotFoundException("There exists no building with ID " + deletedEntityID + ", but one was attempted to be deleted.");
                Building building = world.GetBuilding(deletedEntityID);
                entity = building;
                building.buildingType.buildings.Remove(building);
                window.UpdateLOSDelete(entity);
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
                    if (unit.PlayerNumber == playerNumber) window.UpdateLOSMove(unit, change, change2);
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
            if (!definitions[playerNumber].technologies.ContainsKey(technologyName)) return;
            Technology technology = definitions[playerNumber].technologies[technologyName];
            technology.researched = true;
            technology.ApplyEffects(definitions[playerNumber]);
        }

        /// <summary>
        /// Either starts or stops (based on started) the attack animation between two entities
        /// with the given types and IDs.
        /// </summary>
        public void AnimateAttack(bool started, int attackerType, int attackerId, int defenderType, int defenderId)
        {
            // First, find the actual entities
            Entity attacker = attackerType == 0 ? (Entity)world.GetUnit(attackerId) : (Entity)world.GetBuilding(attackerId);
            Entity defender = started ? defenderType == 0 ? (Entity)world.GetUnit(defenderId) : (Entity)world.GetBuilding(defenderId) : null;

            // If we're starting it, add the attacker to the list of attackers and the defender correspondingly
            if (started)
            {
                if(attacker is Unit)
                {
                    attackingUnits.Add((Unit)attacker);
                    ((Unit)attacker).entityToAttack = defender;
                    ((Unit)attacker).attackTick = 0;
                }
                else
                {
                    attackingBuildings.Add((Building)attacker);
                    ((Building)attacker).entityToAttack = defender;
                    ((Building)attacker).attackTick = 0;
                }
            }
            // Otherwise remove them
            else
            {
                if(attacker is Unit)
                {
                    attackingUnits.Remove((Unit)attacker);
                }
                else
                {
                    attackingBuildings.Remove((Building)attacker);
                }
            }
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
            // Events that happen every tick
            TickAttackAnimations();

            // Events that happen every second
            if (currentTick % (int)(1000 / GameInfo.TICK_TIME) == 0)
            {
                TickResourceGathering();
            }

            currentTick++;
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
                    currentResources[b.buildingType.resourceType] += b.buildingType.gatherSpeed;
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

                        currentResources[b.buildingType.builtOnResourceType] += amount;

                        connection.InformResourceGather(amount, b.resource);
                    }
                }
            }
        }

        /// <summary>
        /// Ticks all currently attacking entities' animations (including projectiles).
        /// </summary>
        private void TickAttackAnimations()
        {
            // Units first
            foreach(Unit u in attackingUnits)
            {
                if(u.attackTick >= u.unitType.attackTicks)
                {
                    u.attackTick = 0;
                }
                else
                {
                    u.attackTick++;
                }
            }
            // Then buildings
            foreach(Building b in attackingBuildings)
            {
                if(b.attackTick >= b.buildingType.attackTicks)
                {
                    b.attackTick = 0;
                }
                else
                {
                    b.attackTick++;
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
