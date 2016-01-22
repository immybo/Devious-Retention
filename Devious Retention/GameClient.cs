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
        private CTSConnection connection;
        // Each player's GameInfo is changed over time due to technologies, factions, etc
        public List<GameInfo> definitions { get; private set; }
        // Entities are gotten from the server every tick
        public HashSet<Resource> resources { get; private set; }
        public HashSet<Unit> units { get; private set; }
        public HashSet<Building> buildings { get; private set; }

        public List<Entity> selected { get; private set; }

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

            resources = new HashSet<Resource>();
            buildings = new HashSet<Building>();
            units = new HashSet<Unit>();

            selected = new List<Entity>();

            currentResources = new int[GameInfo.RESOURCE_TYPES];

            buildingPanelOpen = true;

            Unit.ResetNextID();
            Building.ResetNextID();
            Resource.ResetNextID();

            GameInfo.ReadDefinitions();

            definitions = new List<GameInfo>();
            for (int i = 0; i < numberOfPlayers; i++)
                definitions[i] = new GameInfo();
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
        /// </summary>
        public bool CreateFoundation(BuildingType building, double x, double y)
        {
            return false;
        }

        /// <summary>
        /// Attempts to create a unit from the given building.
        /// Returns whether or not the client has enough resources
        /// for this and the building can create that UnitType.
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
        public void AddEntity(int entityType, string type, double xPos, double yPos, int player)
        {
            if(entityType == 0)
            {
                if (!definitions[playerNumber].unitTypes.ContainsKey(type)) return; // do nothing if the unit type isn't found
                UnitType unitType = definitions[playerNumber].unitTypes[type];
                units.Add(new Unit(unitType, xPos, yPos, player));
            }
            else if(entityType == 1)
            {
                if (!definitions[playerNumber].buildingTypes.ContainsKey(type)) return; // do nothing if the building type isn't found
                BuildingType buildingType = definitions[playerNumber].buildingTypes[type];
                buildings.Add(new Building(buildingType, xPos, yPos, player));
            }
            else if(entityType == 2)
            {
                if (!definitions[playerNumber].resourceTypes.ContainsKey(type)) return; // do nothing if the resource type isn't found
                ResourceType resourceType = definitions[playerNumber].resourceTypes[type];
                resources.Add(new Resource(resourceType, xPos, yPos));
            }
        }

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="type">0=unit, 1=building, 2=resource</param>
        /// <param name="deletedEntityID">The ID of the entity to be deleted.</param>
        public void DeleteEntity(int type, int deletedEntityID)
        {

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

        }

        /// <summary>
        /// Sets the technology with the given name's research state to be TRUE for the given player.
        /// </summary>
        public void SetTechnologyResearched(int playerNumber, string technologyName)
        {

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
    }
}
