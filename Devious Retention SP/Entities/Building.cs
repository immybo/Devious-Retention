﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Buildings are entities that:
    /// - Can be attacked
    /// - Can't move
    /// - Can sometimes attack
    /// - Can sometimes train units
    /// - Can sometimes research technologies
    /// </summary>
    public abstract class Building : Entity, Attackable
    {
        public Building(Player player, double x, double y)
            : base(player, x, y)
        {
            
        }

        public abstract void Damage(int amount, int damageType);
        public abstract void Heal(int amount);
        public abstract bool IsDead();

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
            return new Command[]
            {
            };
        }
    }
}
