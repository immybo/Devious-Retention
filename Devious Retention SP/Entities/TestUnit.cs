﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    class TestUnit : Unit
    {
        private const float MOVEMENT_SPEED = 0.1f;

        public TestUnit(Player player, double x, double y, double size)
            : base(player, x, y, size, MOVEMENT_SPEED)
        {
            this.maxHitpoints = 100;
        }

        public override void Damage(int amount, int damageType)
        {
            this.hitpoints -= amount;
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            PointF topLeft = p.Transform(this.GetPosition());
            g.FillRectangle(new SolidBrush(Color.Blue), new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(p.Scale().X*this.Size), (int)(p.Scale().Y*this.Size)));
        }
    }
}