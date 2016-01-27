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
        private List<STCConnection> connections;
        
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

        /// <summary>
        /// To create a GameServer, all clients (STCConnections) must be provided,
        /// as well as a generic base GameInfo. Note that the GameServer treats every
        /// game as multiplayer, and so computer controlled players will merely
        /// spoof having a connection.
        /// </summary>
        public GameServer(List<STCConnection> connections, Map map)
        {
            this.connections = connections;
            this.map = map;

            units = new Dictionary<int, Unit>();
            buildings = new Dictionary<int, Building>();
            resources = new Dictionary<int, Resource>();
            entitiesBySquare = new List<Entity>[map.width,map.height];

            Timer tickTimer = new Timer();
            tickTimer.Interval = GameInfo.TICK_TIME;
            tickTimer.Tick += Tick;
        }

        /// <summary>
        /// Tells all clients that a tick has occured.
        /// </summary>
        private void Tick(object sender, EventArgs e)
        {
            foreach (STCConnection c in connections)
                c.Tick();
        }

        /// <summary>
        /// Attempts to place a foundation of the given building type for the given player at (x,y).
        /// Does nothing if no foundation could be placed there.
        /// </summary>
        public void CreateBuilding(int player, BuildingType buildingType, double x, double y)
        {
            Building building = new Building(buildingType, Building.nextID, x, y, player);
            Building.IncrementNextID();
            // Make sure that the building doesn't collide with any other entities
            List<Coordinate> buildingCoordinates = Map.GetIncludedTiles(map, building);

            foreach (Coordinate c in buildingCoordinates)
                if(entitiesBySquare[c.x, c.y] != null)
                    foreach (Entity e in entitiesBySquare[c.x, c.y])
                        if (e.GetX() + e.GetSize() > building.x && e.GetY() + e.GetSize() > building.y
                        && e.GetX() < building.x + building.GetSize() && e.GetY() < building.y + building.GetSize()) // If they collide, do nothing
                            return;

            // No collisions, so we can safetly place the foundation :)
            foreach (STCConnection c in connections)
                c.InformEntityAdd(building);

            buildings.Add(building.id, building);
            foreach(Coordinate c in buildingCoordinates)
            {
                if (entitiesBySquare[c.x, c.y] == null) entitiesBySquare[c.x, c.y] = new List<Entity> { building };
                else entitiesBySquare[c.x, c.y].Add(building);
            }

        }

        /// <summary>
        /// Attempts to create a unit of the given type from the given building.
        /// Does nothing if no space could be found around the building.
        /// </summary>
        public bool CreateUnit(Building sourceBuilding, UnitType unit)
        {
            return false;
        }

        /// <summary>
        /// Adds the given technology to the list of technologies that the
        /// given player has researched. This can't fail.
        /// </summary>
        public void ResearchTechnology(int player, Technology technology)
        {

        }

        /// <summary>
        /// Removes the appropriate amount of health from the target,
        /// given that the source attacks the target once.
        /// Does nothing if either entity is a resource, or if either
        /// entity does not exist in the server's list of entities.
        /// </summary>
        public void DamageEntity(Entity source, Entity target)
        {

        }

        /// <summary>
        /// Moves the given unit by (dX, dY).
        /// Does NO CHECKING as to whether the unit would actually be
        /// able to move by this much; this is assumed when the command
        /// is sent.
        /// </summary>
        public void MoveUnit(Unit unit, double dX, double dY)
        {

        }

        /// <summary>
        /// Removes the given entity from the list of entities.
        /// Does nothing if the given entity does not exist in the server's
        /// lists of entities.
        /// </summary>
        public void DeleteEntity(Entity entity)
        {

        }
    }
}
