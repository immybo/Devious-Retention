using System;
using System.Collections.Generic;
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
        // The client's GameInfo is changed over time by technologies, etc
        private GameInfo info;
        // Entities are gotten from the server every tick
        public HashSet<Resource> resources { get; private set; }
        public HashSet<Unit> units { get; private set; }
        public HashSet<Building> buildings { get; private set; }

        public List<Entity> selected { get; private set; }
        // The type of building that has been selected from the create buildings panel
        public BuildingType selectedBuilding { get; private set; }

        public Map map { get; private set; }

        private GameWindow window;

        // This should be unique within a given game
        private int playerNumber;

        // Where the top-left of the screen is, in map co-ordinates.
        public double screenY { get; private set; }
        public double screenX { get; private set; }

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
        public GameClient(int playerNumber, Map map, GameWindow window, GameInfo info, CTSConnection connection, Faction faction)
        {

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
        /// Returns whether ot not the client has enough resources
        /// for this and the prerequisites are met.
        /// </summary>
        public bool ResearchTechnology(Technology technology)
        {
            return false;
        }

        /// <summary>
        /// Sets the sets of entities to the given set.
        /// </summary>
        public void SetEntities(HashSet<Entity> entities)
        {

        }

        /// <summary>
        /// Updates the given entities, and adds them to the client's
        /// sets of entities if they weren't in there before. Uses the
        /// entities' IDs to figure out which is which.
        /// </summary>
        public void UpdateEntities(HashSet<Entity> changedEntities)
        {

        }

        /// <summary>
        /// Removes the given entities from the client's sets of entities
        /// if they were in there. Does nothing if they weren't.
        /// </summary>
        public void DeleteEntities(HashSet<Entity> deletedEntities)
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
