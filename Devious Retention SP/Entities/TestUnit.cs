using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    public class TestUnit : Unit, Attacker
    {
        private const float MOVEMENT_SPEED = 0.1f;

        public TestUnit(Player player, double x, double y, double size)
            : base(player, x, y, size, MOVEMENT_SPEED, "TestUnit")
        {
            this.MaxHitpoints = 100;
            this.Hitpoints = MaxHitpoints;
        }

        public override void Damage(int amount, int damageType)
        {
            this.Hitpoints -= amount;
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            PointF topLeft = p.Transform(this.GetPosition());
            g.FillRectangle(new SolidBrush(this.Player.Color), new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(p.Scale().X*this.Size), (int)(p.Scale().Y*this.Size)));
        }

        public override Command GetCommand(PointF worldCoordinate, MouseButtons button, World world)
        {
            if (button.Equals(MouseButtons.Right))
            {
                IEntity e = world.GetEntityAtPoint(worldCoordinate);
                if (e != null && e is Attackable)
                {
                    return new AttackCommand(this, (Attackable)e, world);
                }
            }

            return base.GetCommand(worldCoordinate, button, world);
        }

        public int GetAttackTime()
        {
            return 1;
        }

        public int GetDamage()
        {
            return 10;
        }

        public int GetDamageType()
        {
            return 0;
        }

        public float GetRange()
        {
            return 2;
        }
    }
}
