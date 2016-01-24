using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A resource is an entity that is immoveable and not player controlled.
    /// They spawn at the beginning of a game, and have a definite amount of the 
    /// resource, which can be exhausted through collecting the resource by
    /// placing a building on top of it. After exhaustion, the resource disappears.
    /// </summary>
    public class Resource : Entity
    {
        public static int nextID { get; private set; }
        // Unique
        public int id { get; private set; }

        // Every resource belongs to a type, which gives most of its statistics
        public ResourceType type { get; private set; }

        // The current amount of the given resource in this resource
        public int amount;

        // The position of the top-left of this resource
        public double x { get; private set; }
        public double y { get; private set; }

        /// <summary>
        /// Most of a resource's statistics are gathered from its type,
        /// however its position must also be provided.
        /// </summary>
        public Resource(ResourceType type, int id, double x, double y)
        {
            this.type = type;
            this.id = id;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Returns whether or not this resource has
        /// less than or equal to 0 resource remaining
        /// </summary>
        public bool Depleted()
        {
            return amount <= 0;
        }

        /// <summary>
        /// Returns the image that is currently appropriate for this resource.
        /// </summary>
        public Image GetImage()
        {
            return type.image;
        }

        /// <summary>
        /// Returns the size of this resource's type
        /// </summary>
        public double GetSize()
        {
            return type.size;
        }

        public double GetX()
        {
            return x;
        }
        public double GetY()
        {
            return y;
        }

        /// <summary>
        /// Returns 0! Should never be called, but if it is,
        /// returning 0 should cause nothing special to happen.
        /// </summary>
        public int GetLOS()
        {
            return 0;
        }
        /// <summary>
        /// Returns 0.
        /// </summary>
        public int GetPlayerNumber()
        {
            return 0;
        }

        public int GetID()
        {
            return id;
        }


        /// <summary>
        /// Resets the next ID to 0.
        /// </summary>
        public static void ResetNextID()
        {
            nextID = 0;
        }
        /// <summary>
        /// Increments the next ID by 1.
        /// </summary>
        public static void IncrementNextID()
        {
            nextID++;
        }
    }
}
