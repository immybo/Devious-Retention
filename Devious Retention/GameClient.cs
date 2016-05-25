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

        public CTSConnection connection { get; set; }
        // Each player's GameInfo is changed over time due to technologies, factions, etc
        public List<GameInfo> definitions { get; private set; }
        public GameInfo info { get; private set; } // This client's GameInfo (= definitions[playerNumber])

        // Doesn't need to be stored on the server, this is just used to determine time passing between events
        private int currentTick;

        // Entities are gotten from the server every tick
        public Dictionary<int, Resource> resources { get; private set; }
        public Dictionary<int, Unit> units { get; private set; }
        public Dictionary<int, Building> buildings { get; private set; }
        // Which entities are where; if at least part of an entity is on a square, it will be recorded in that square's list
        public List<Entity>[,] entitiesBySquare { get; private set; }

        // TODO Possibly migrate selected to window
        public List<Entity> selected;

        public Map map { get; private set; }

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
        public GameClient(int playerNumber, int numberOfPlayers, Map map, CTSConnection connection, List<Faction> factions)
        {
            this.map = map;
            //this.faction = faction;
            this.connection = connection;
            this.playerNumber = playerNumber;
            playerColor = GameInfo.PLAYER_COLORS[playerNumber];

            resources = new Dictionary<int, Resource>();
            buildings = new Dictionary<int, Building>();
            units = new Dictionary<int, Unit>();
            entitiesBySquare = new List<Entity>[map.width, map.height];

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
            
            window = new GameWindow(this);

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
            if (x < 0 || y < 0 || x >= map.width || y >= map.height) return;

            // If there's anything to attack on that square, check which one is under the cursor (if any)
            if(entitiesBySquare[(int)x, (int)y] != null && entitiesBySquare[(int)x, (int)y].Count > 0)
            {
                foreach(Entity e in entitiesBySquare[(int)x, (int)y])
                {
                    // If the entity is under the mouse, attack it (just attack the first one, don't do any decision making if there's more)
                    // (also make sure it isn't a resource & isn't allied with the player)
                    if(e.X + e.Type.size > x && e.Y + e.Type.size > y && e.X < x && e.Y < y && !(e is Resource) && !teammates.Contains(e.PlayerNumber))
                    {
                        AttackEntity(e);
                        return;
                    }
                }
            }
            // Otherwise, move the units
            MoveUnits(x, y);
        }

        /// <summary>
        /// Asks the server to move selected units to (x,y)
        /// </summary>
        public void MoveUnits(double x, double y)
        {
            List<Unit> selectedUnits = new List<Unit>();
            foreach (Entity e in selected)
                if (e is Unit)
                    selectedUnits.Add((Unit)e);
            
            // We can check here if it would go off the map
            foreach (Unit unit in selectedUnits)
            {
                double adjustedX = x - unit.Type.size / 2;
                double adjustedY = y - unit.Type.size / 2;
                if (adjustedX < 0) adjustedX = 0;
                if (adjustedY < 0) adjustedY = 0;

                if (x + unit.Type.size >= map.width) adjustedX = map.width - unit.Type.size;
                if (y + unit.Type.size >= map.height) adjustedY = map.height - unit.Type.size;

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
                units.Add(unit.ID, unit);
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
                if(resources.ContainsKey(resourceID)) building.resource = resources[resourceID];
                buildings.Add(building.ID, building);
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
                resources.Add(resource.ID, resource);
                entity = resource;
            }

            // Check which tiles it at least partially occupies
            foreach (Coordinate c in map.GetIncludedTiles(entity))
            {
                if (entitiesBySquare[c.x, c.y] == null) entitiesBySquare[c.x, c.y] = new List<Entity> { entity };
                else entitiesBySquare[c.x, c.y].Add(entity);
            }
        }

        /// <summary>
        /// Removes an entity.
        /// Does nothing if no entity of the given type and ID can be found.
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="deletedEntityID">The ID of the entity to be deleted.</param>
        public void DeleteEntity(int entityType, int deletedEntityID)
        {
            Entity entity = null;
            if (entityType == 0)
            {
                if (!units.ContainsKey(deletedEntityID)) return; // If the unit doesn't exist, do nothing
                entity = units[deletedEntityID];
                units[deletedEntityID].unitType.units.Remove(units[deletedEntityID]); // And finally remove it from both collections it appears in (in the type and in the client)
                units.Remove(deletedEntityID);
                window.UpdateLOSDelete(entity); // Make sure we update the line of sight of the player
            }
            else if (entityType == 1)
            {
                if (!buildings.ContainsKey(deletedEntityID)) return;
                entity = buildings[deletedEntityID];
                buildings[deletedEntityID].buildingType.buildings.Remove(buildings[deletedEntityID]);
                buildings.Remove(deletedEntityID);
                window.UpdateLOSDelete(entity);
            }
            else if (entityType == 2)
            {
                if (!resources.ContainsKey(deletedEntityID)) return;
                entity = resources[deletedEntityID];
                resources.Remove(deletedEntityID);
            }

            // Remove it from the selected entities if it was in there
            if (selected.Contains(entity)) selected.Remove(entity);
            // And from the lists of entities by tile
            foreach (Coordinate c in map.GetIncludedTiles(entity))
                entitiesBySquare[c.x, c.y].Remove(entity);
        }

        /// <summary>
        /// Changes one property of an entity, e.g. its position,
        /// hitpoints, built status..
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="entityID">The ID of the entity to be changed. Nothing will happen if no entity with this ID exists.</param>
        /// <param name="propertyID">The ID of the property to be changed. Nothing will happen if this is invalid.</param>
        /// <param name="change">The modifier to the property.</param>
        public void ChangeEntityProperty(int entityType, int entityID, int propertyID, double change, double change2)
        {
            if (entityType == 0)
            {
                if (!units.ContainsKey(entityID)) return;
                Unit unit = units[entityID];

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
                if (!buildings.ContainsKey(entityID)) return;
                Building building = buildings[entityID];

                if (propertyID == 0) building.hitpoints += (int)change;
                else if (propertyID == 1)
                {
                    if ((int)change == 0) building.attackTick++;
                    else building.attackTick = 0;
                }
            }
            else if (entityType == 2)
            {
                if (!resources.ContainsKey(entityID)) return;
                Resource resource = resources[entityID];

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
            Entity attacker = attackerType == 0 ? (Entity)units[attackerId] : (Entity)buildings[attackerId];
            Entity defender = started ? defenderType == 0 ? (Entity)units[defenderId] : (Entity)buildings[defenderId] : null;

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
            foreach(Building b in buildings.Values)
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
