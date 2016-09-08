using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    class MoveCommand : Command
    {
        private Unit unit;
        private PointF endPoint;
        private World world;

        public MoveCommand(Unit unit, PointF endPoint, World world)
        {
            this.unit = unit;
            this.endPoint = endPoint;
            this.world = world;
        }

        public void Execute()
        {
            unit.AddPendingCommand(this);
        }

        public bool Tick()
        {
            double xDifference = endPoint.X - unit.X;
            double yDifference = endPoint.Y - unit.Y;

            double moveSpeed = unit.MovementSpeed;

            double xMovespeed = moveSpeed * (xDifference / (xDifference + yDifference));
            double yMovespeed = moveSpeed * (yDifference / (xDifference + yDifference));
            xMovespeed = xDifference > 0 ? xMovespeed : -xMovespeed;
            yMovespeed = yDifference > 0 ? yMovespeed : -yMovespeed;

            double newX = Math.Abs(xMovespeed) >= Math.Abs(xDifference) ? endPoint.X : unit.X + xMovespeed;
            double newY = Math.Abs(yMovespeed) >= Math.Abs(yDifference) ? endPoint.Y : unit.Y + yMovespeed;

            unit.Teleport(newX, newY);

            return !(newX == endPoint.X && newY == endPoint.Y);
        }

        private void MoveTick()
        {

        }
    }
}
