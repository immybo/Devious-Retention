using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A Server-to-Client Connection sends information from the server over 
    /// the socket connection, and receives information from the client, passing
    /// that on to its server.
    /// </summary>
    class STCConnection
    {
        private String ip;
        private Socket socket;
        private GameServer server;

        public STCConnection(String ip, GameServer server)
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
        /// Informs the client of the new set of entities;
        /// replacing any old entities. Typically used
        /// at the start of a game only.
        /// </summary>
        public void InformEntities(HashSet<Entity> entities)
        {

        }

        /// <summary>
        /// Informs the client of the new attributes of
        /// a given entity, or that a new entity has been created.
        /// </summary>
        public void InformEntity(Entity entity)
        {

        }

        /// <summary>
        /// Informs the client that a given entity
        /// has been deleted.
        /// </summary>
        public void InformEntityDeletion(Entity entity)
        {

        }

        /// <summary>
        /// Informs the client that the map has been
        /// changed to the given one. Typically only used
        /// at the start of a game.
        /// </summary>
        public void InformMap(Map map)
        {

        }

        /// <summary>
        /// Informs the client that the game has ended.
        /// </summary>
        /// <param name="won">Whether or not this player won the game.</param>
        public void InformGameOver(Boolean won)
        {

        }

        /// <summary>
        /// Informs the client to update the game.
        /// </summary>
        public void Tick()
        {

        }
    }
}
