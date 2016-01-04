using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A Client-to-Server Connection sends information from the client over 
    /// the socket connection, and receives information from the server, passing
    /// that on to its client.
    /// </summary>
    public class CTSConnection
    {
        private String ip;
        private Socket socket;
        private GameClient client;

        public CTSConnection(String ip, GameClient client)
        {

        }

        /// <summary>
        /// Attempts to create a socket connection to the given IP.
        /// Returns whether or not it succeeded in doing so.
        /// </summary>
        public bool Connect()
        {
            return false;
        }

        /// <summary>
        /// Closes the socket connection.
        /// </summary>
        public void Disconnect()
        {

        }

        /// <summary>
        /// Sends a request for a building to be created at the specified location.
        /// </summary>
        public void RequestBuilding(BuildingType building, double x, double y)
        {

        }

        /// <summary>
        /// Sends a request for a unit to be created at the specified building.
        /// </summary>
        public void RequestUnit(Building sourceBuilding, UnitType unit)
        {

        }

        /// <summary>
        /// Sends a request for the given technology to be researched.
        /// </summary>
        public void RequestTechnology(Technology technology)
        {

        }

        /// <summary>
        /// Sends a request for the given unit to be moved by the given amount.
        /// </summary>
        public void RequestMove(Unit unit, double dX, double dY)
        {

        }

        /// <summary>
        /// Sends a request for the given entity to be deleted.
        /// </summary>
        public void RequestDelete(Entity entity)
        {

        }
    }
}
