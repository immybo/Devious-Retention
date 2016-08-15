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
        public Resource(Player player, double x, double y)
            : base(player, x, y)
        {

        }

        public abstract int MaxResourceCount();
        public abstract int CurrentResourceCount();
        public abstract void Gather(int amount);

        public override void Draw(Graphics g, PositionTransformation p)
        {
            throw new NotImplementedException();
        }

        public override void SendCommand(Entity entity, PointF point, Command command)
        {
            throw new NotImplementedException();
        }

        public override void SendKeyboardCommand(Entity entity, PointF point, Keys input)
        {
            throw new NotImplementedException();
        }

        public override void SendMouseCommand(Entity entity, PointF point, MouseButtons input)
        {
            throw new NotImplementedException();
        }

        public override Command[] ValidCommands()
        {
            throw new NotImplementedException();
        }
    }
}
