using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public class MoveCommand : Command
    {
        private Unit unit;
        private PointF endPoint;
        private World world;

        public MoveCommand(Unit unit, PointF endPoint, World world)
        {
            this.unit = unit;
            endPoint.X = (float)(endPoint.X < 0 ? 0 : endPoint.X + unit.Size > world.Map.Width ? world.Map.Width - unit.Size : endPoint.X);
            endPoint.Y = (float)(endPoint.Y < 0 ? 0 : endPoint.Y + unit.Size > world.Map.Height ? world.Map.Height - unit.Size : endPoint.Y);
            this.endPoint = endPoint;

            this.world = world;
        }

        public override void Execute()
        {
            foreach(Command c in unit.GetPendingCommands()){
                if (c is MoveCommand) c.SetFinished();
            }
            unit.AddPendingCommand(this);
        }

        public override bool Tick()
        {
            if (!base.Tick()) return false;

            double distanceTo = Math.Sqrt(Math.Pow(endPoint.X - unit.X, 2) + Math.Pow(endPoint.Y - unit.Y, 2)); // this is just for floating point error
            if (distanceTo <= 0.01) return false;

            double xDifference = endPoint.X - unit.X;
            double yDifference = endPoint.Y - unit.Y;

            double moveSpeed = unit.MovementSpeed;

            double xMovespeed = moveSpeed * (xDifference / (Math.Abs(xDifference) + Math.Abs(yDifference)));
            double yMovespeed = moveSpeed * (yDifference / (Math.Abs(xDifference) + Math.Abs(yDifference)));

            double newX = Math.Abs(xMovespeed) >= Math.Abs(xDifference) ? endPoint.X : unit.X + xMovespeed;
            double newY = Math.Abs(yMovespeed) >= Math.Abs(yDifference) ? endPoint.Y : unit.Y + yMovespeed;

            unit.Teleport(newX, newY);
            return true;
        }
    }
}
