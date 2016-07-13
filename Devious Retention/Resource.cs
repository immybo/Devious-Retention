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
        // Every resource belongs to a type, which gives most of its statistics
        public ResourceType resourceType { get; private set; }

        // The current amount of the given resource in this resource
        public double amount;

        private Image image;

        /// <summary>
        /// Most of a resource's statistics are gathered from its type,
        /// however its position must also be provided.
        /// </summary>
        public Resource(ResourceType type, double x, double y)
            : base(null, x, y)
        {
            Init(type);
        }
        public Resource(ResourceType type, int id, double x, double y)
            : base(null, id, x, y)
        {
            Init(type);
        }

        private void Init(ResourceType type)
        {
            this.resourceType = type;
            this.amount = type.resourceAmount;
        }

        public override Image GetImage()
        {
            return resourceType.image;
        }

        /// <summary>
        /// Returns whether or not this resource has
        /// less than or equal to 0 resource remaining
        /// </summary>
        public bool Depleted()
        {
            return amount <= 0;
        }

        public override void RenderHPBar(Graphics g, Rectangle bounds)
        {
            return; // not applicable
        }

        public override bool Attackable()
        {
            return false;
        }
        public override EntityType GetEntityType()
        {
            return resourceType;
        }
    }
}
