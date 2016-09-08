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
        public Resource(Player player, double x, double y, double size)
            : base(player, x, y, size)
        {

        }

        public abstract int MaxResourceCount();
        public abstract int CurrentResourceCount();
        public abstract void Gather(int amount);

        public override void Draw(Graphics g, PositionTransformation p)
        {
            throw new NotImplementedException();
        }
    }
}
