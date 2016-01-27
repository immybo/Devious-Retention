﻿using System;
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
        private CTSConnection connection;
        // Each player's GameInfo is changed over time due to technologies, factions, etc
        public List<GameInfo> definitions { get; private set; }
        public GameInfo info { get; private set; } // This client's GameInfo (= definitions[playerNumber])

        // Entities are gotten from the server every tick
        public Dictionary<int, Resource> resources { get; private set; }
        public Dictionary<int, Unit> units { get; private set; }
        public Dictionary<int, Building> buildings { get; private set; }
        // Which entities are where; if at least part of an entity is on a square, it will be recorded in that square's list
        private List<Entity>[,] entitiesBySquare;

        public List<Entity> selected;

        public Map map { get; private set; }

        private GameWindow window;

        // This should be unique within a given game
        public int playerNumber { get; private set; }
        public Color playerColor { get; private set; }

        // How many of each resource the player currently has
        // Resources are handled entirely client-side
        // metal, oil, energy, science
        public int[] currentResources { get; private set; }

        // Whether the building panel or the technology panel is open
        public bool buildingPanelOpen { get; private set; }

        // The faction this player belongs to
        private Faction faction;
        
        /// <summary>
        /// When a GameClient is created, it is assumed that entities will be
        /// sent from the CTSConnection before the first tick.
        /// </summary>
        public GameClient(int playerNumber, int numberOfPlayers, Map map, GameWindow window, CTSConnection connection, List<Faction> factions)
        {
            this.map = map;
            this.window = window;
            window.client = this;
            window.InitLOS();
            // this.faction = faction;
            // this.connection = connection;
            this.playerNumber = playerNumber;
            playerColor = GameInfo.PLAYER_COLORS[playerNumber];

            resources = new Dictionary<int, Resource>();
            buildings = new Dictionary<int, Building>();
            units = new Dictionary<int, Unit>();

            selected = new List<Entity>();

            currentResources = new int[GameInfo.RESOURCE_TYPES];

            buildingPanelOpen = true;

            Unit.ResetNextID();
            Building.ResetNextID();
            Resource.ResetNextID();

            definitions = new List<GameInfo>();
            for (int i = 0; i < numberOfPlayers; i++)
                definitions.Add(new GameInfo());
            info = definitions[playerNumber];

            Timer windowRefreshTimer = new Timer();
            windowRefreshTimer.Interval = GameInfo.WINDOW_REFRESH_TIME;
            windowRefreshTimer.Tick += new EventHandler(WindowRefreshTimerHandler);
            windowRefreshTimer.Start();
        }

        /// <summary>
        /// Returns whether or not the player has at least the 
        /// given amount of each resource.
        /// </summary>
        public bool CanAfford(int[] resources)
        {
            return false;
        }

        /// <summary>
        /// Removes the given resource amounts from the player.
        /// Assumes that the player has at least the given resource amounts.
        /// </summary>
        public void SpendResources(int[] resources)
        {

        }

        /// <summary>
        /// Selects all entities within the specified rectangle.
        /// </summary>
        public void DragSelect(double startX, double startY, double endX, double endY)
        {

        }

        /// <summary>
        /// Attempts to select an entity at the given position.
        /// Does nothing if there is no entity at the given position.
        /// </summary>
        public void ClickSelect(double x, double y)
        {

        }

        /// <summary>
        /// Processes a click on the currently selected entity pane.
        /// Uses pixel co-ordinates rather than the usual map co-ordinates.
        /// </summary>
        public void ClickEntityPane(int x, int y)
        {

        }

        /// <summary>
        /// Attempts to create a building foundation of the given type
        /// at the given position. Returns whether or not the client has 
        /// enough resources for this and the player can create that BuildingType.
        /// 
        /// Does not yet remove the resources; only removes them when confirmation
        /// is received from the server that the foundation was created.
        /// </summary>
        public bool CreateFoundation(BuildingType building, double x, double y)
        {
            return false;
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
            return false;
        }

        /// <summary>
        /// Attempts to research the given technology.
        /// Returns whether or not the client has enough resources
        /// for this and the prerequisites are met.
        /// </summary>
        public bool ResearchTechnology(Technology technology)
        {
            return false;
        }

        /// <summary>
        /// Adds an entity. Does nothing if the entity type isn't found.
        /// </summary>
        /// <param name="entityType">0=unit, 1=building, 2=resource</param>
        /// <param name="type">The name of the type of entity to be created.</param>
        /// <param name="xPos">The initial x position of the new entity.</param>
        /// <param name="yPos">The initial y position of the new entity.</param>
        /// <param name="player">The player that the entity belongs to. Irrelevant if a resource.</param>
        public void AddEntity(int entityType, int id, string type, double xPos, double yPos, int player)
        {
            Entity entity = null;
            if(entityType == 0)
            {
                if (!definitions[playerNumber].unitTypes.ContainsKey(type)) return; // do nothing if the unit type isn't found
                UnitType unitType = definitions[playerNumber].unitTypes[type];
                Unit unit = new Unit(unitType, id, xPos, yPos, player);
                units.Add(unit.id, unit);
                unitType.units.Add(unit);
                window.UpdateLOSAdd(unit);
                entity = unit;

                // If the unit belongs to the player, remove the resources as well
                if (unit.player == playerNumber)
                    for (int i = 0; i < currentResources.Length; i++)
                        currentResources[i] -= unit.type.resourceCosts[i];
            }
            else if(entityType == 1)
            {
                if (!definitions[playerNumber].buildingTypes.ContainsKey(type)) return; // do nothing if the building type isn't found
                BuildingType buildingType = definitions[playerNumber].buildingTypes[type];
                Building building = new Building(buildingType, id, xPos, yPos, player);
                buildings.Add(building.id, building);
                buildingType.buildings.Add(building);
                window.UpdateLOSAdd(building);
                entity = building;

                // If the building belongs to the player, remove the resources as well
                if (building.player == playerNumber)
                    for (int i = 0; i < currentResources.Length; i++)
                        currentResources[i] -= building.type.resourceCosts[i];
            }
            else if(entityType == 2)
            {
                if (!definitions[playerNumber].resourceTypes.ContainsKey(type)) return; // do nothing if the resource type isn't found
                ResourceType resourceType = definitions[playerNumber].resourceTypes[type];
                Resource resource = new Resource(resourceType, id, xPos, yPos);
                resources.Add(resource.id, resource);
                entity = resource;
            }
            
            // Check which tiles it at least partially occupies
            foreach(Coordinate c in Map.GetIncludedTiles(map, entity))
                entitiesBySquare[c.x, c.y].Add(entity);
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
                window.UpdateLOSDelete(entity); // Make sure we update the line of sight of the player

                units[deletedEntityID].type.units.Remove(units[deletedEntityID]); // And finally remove it from both collections it appears in (in the type and in the client)
                units.Remove(deletedEntityID);
            }
            else if (entityType == 1)
            {
                if (!buildings.ContainsKey(deletedEntityID)) return;

                entity = buildings[deletedEntityID];
                window.UpdateLOSDelete(entity);

                buildings[deletedEntityID].type.buildings.Remove(buildings[deletedEntityID]);
                buildings.Remove(deletedEntityID);
            }
            else if (entityType == 2)
            {
                if (!resources.ContainsKey(deletedEntityID)) return;
                entity = resources[deletedEntityID];
                resources.Remove(deletedEntityID);
            }

            // Remove it from the selected entities if it was in there
            if (selected.Contains(entity)) selected.Remove(entity);
            // And from the lists of entites by tile
            foreach (Coordinate c in Map.GetIncludedTiles(map, entity))
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
        public void ChangeEntityProperty(int entityType, int entityID, int propertyID, double change)
        {
            if (entityType == 0)
            {
                if (!units.ContainsKey(entityID)) return;
                Unit unit = units[entityID];

                if (propertyID == 0) unit.hitpoints += (int)change;
                else if (propertyID == 1) unit.x += change;
                else if (propertyID == 2) unit.y += change;
                else if(propertyID == 3)
                {
                    if ((int)change == 1) unit.BeginBattleAnimation();
                    else unit.StopBattleAnimation();
                }
                else if(propertyID == 4)
                {
                    if ((int)change == 1) unit.BeginMovementAnimation();
                    else unit.StopMovementAnimation();
                }
            }
            else if (entityType == 1)
            {
                if (!buildings.ContainsKey(entityID)) return;
                Building building = buildings[entityID];

                if (propertyID == 0) building.hitpoints += (int)change;
                else if (propertyID == 1) building.built = true;
            }
            else if(entityType == 2)
            {
                if (!resources.ContainsKey(entityID)) return;
                Resource resource = resources[entityID];

                if (propertyID == 0) resource.amount -= (int)change;
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
        /// Ends the game from this client's perspective.
        /// </summary>
        /// <param name="won">Whether or not this player won the game.</param>
        public void EndGame(bool won)
        {

        }

        /// <summary>
        /// Updates everything by one tick.
        /// </summary>
        public void Tick()
        {
        }

        /// <summary>
        /// Refreshes the game window.
        /// </summary>
        private void WindowRefreshTimerHandler(object source, EventArgs e)
        {
            window.Refresh();
        }
    }
}
