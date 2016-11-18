using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Resources are entities that:
    /// - Can't be attacked
    /// - Can't move
    /// - Can't attack
    /// - Can be gathered from
    /// </summary>
    public abstract class Resource : Entity, Gatherable
    {
        private int resourceType;
        private int currentResourceAmount;
        private int maxResourceAmount;

        public Resource(double x, double y, double size, string name, int resourceType, int maxResourceAmount, int currentResourceAmount)
            : base(new NullPlayer(null), x, y, size, name)
        {
            this.resourceType = resourceType;
            this.currentResourceAmount = currentResourceAmount;
            this.maxResourceAmount = maxResourceAmount;
        }

        public int MaxResourceCount()
        {
            return maxResourceAmount;
        }
        public int CurrentResourceCount()
        {
            return currentResourceAmount;
        }

        public void Gather(Player player, int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException("Can't gather a negative amount from a resource!");

            int gatheredAmount = amount;
            if (currentResourceAmount < gatheredAmount)
                gatheredAmount = currentResourceAmount;
            currentResourceAmount -= gatheredAmount;
            player.ChangeResource(resourceType, gatheredAmount);
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            throw new NotImplementedException();
        }
    }
}
