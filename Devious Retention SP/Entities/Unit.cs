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
    /// Units are entities that:
    /// - Can be attacked
    /// - Can move
    /// - Can usually attack
    /// - Often have special abilities
    /// </summary>
    public abstract class Unit : Entity
    {
        public Unit(Player player, double x, double y)
            : base(player, x, y)
        {

        }

        public override bool Attackable()
        {
            return true;
        }

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
                Command.MOVE
            };
        }
    }
}