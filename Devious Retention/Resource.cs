using System;
using System.Collections.Generic;
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
        // Every resource belongs to a type, which gives most of its statistics
        private ResourceType type { get; }

        // The current amount of the given resource in this resource
        private int amount { get; }

        // The position of the top-left of this resource
        private double x { get; }
        private double y { get; }

        /// <summary>
        /// Most of a resource's statistics are gathered from its type,
        /// however its position must also be provided.
        /// </summary>
        public Resource(ResourceType type, double x, double y)
        {

        }

        /// <summary>
        /// Reduces the resource's amount of remaining resource
        /// by the given amount.
        /// </summary>
        public void GatherFrom(int amount)
        {

        }

        /// <summary>
        /// Returns the image that is currently appropriate for this resource.
        /// </summary>
        public Image GetImage()
        {

        }
    }
}
