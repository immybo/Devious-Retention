using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    public class TestUnit : Gatherer, Attacker, Builder
    {
        private const float MOVEMENT_SPEED = 0.1f;

        public TestUnit(Player player, double x, double y, double size, int gatherAmount, int gatherTicks)
            : base(player, x, y, size, MOVEMENT_SPEED, "TestUnit", gatherAmount, gatherTicks)
        {
            this.MaxHitpoints = 100;
            this.Hitpoints = MaxHitpoints;
        }
        public TestUnit(Player player, double x, double y, double size)
            : base(player, x, y, size, MOVEMENT_SPEED, "TestUnit", 1, 1)
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
                List<Entity> entitiesAtPoint = world.GetEntitiesAtPoint(worldCoordinate);
                Entity availableEntity = null;
                foreach(Entity e in entitiesAtPoint)
                {
                    if(e.Player != Player && e is Attackable)
                    {
                        availableEntity = e;
                        break;
                    }
                }

                if(availableEntity != null)
                {
                    return new AttackCommand(this, (Attackable)availableEntity, world);
                }
            }

            return base.GetCommand(worldCoordinate, button, world);
        }

        public int GetAttackTime()
        {
            return 20;
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

        public float GetBuildSpeed()
        {
            return 1;
        }
    }
}
