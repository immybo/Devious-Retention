using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    public class TestBuilding : Building, Attacker
    {
        private const float MOVEMENT_SPEED = 0.1f;

        public TestBuilding(Player player, double x, double y, double size, float buildResistance)
            : base(player, x, y, size, "TestBuilding", buildResistance)
        {
            this.MaxHitpoints = 100;
            this.Hitpoints = 1;
        }

        public override void Damage(int amount, int damageType)
        {
            this.Hitpoints -= amount;
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            PointF topLeft = p.Transform(this.GetPosition());
            Rectangle rect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(p.Scale().X * this.Size), (int)(p.Scale().Y * this.Size));
            g.FillRectangle(new SolidBrush(this.Player.Color), rect);
            g.DrawString("Building", new Font("Arial", 20), new SolidBrush(Color.Black), new PointF(topLeft.X + 20, topLeft.Y + 20));
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
    }
}
