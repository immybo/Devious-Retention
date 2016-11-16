using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    public interface IEntity : Drawable
    {
        string Name { get; }

        double X { get; }
        double Y { get; }
        double Size { get; }

        Player Player { get; }

        int ID { get; }

        /// <summary>
        /// Ticks anything that this entity is running; for example,
        /// ticks attack and movement animations if applicable.
        /// </summary>
        void Tick();

        /// <summary>
        /// Teleports this entity to a given position, disregarding
        /// its ability to move normally. No checking is done here
        /// for whether the new position is outside of the map.
        /// </summary>
        void Teleport(double newX, double newY);

        /// <summary>
        /// Changes this entity's position by the given amount,
        /// disregarding its ability to move to/through the space.
        /// </summary>
        void ChangePosition(double x, double y);

        PointF GetPosition();

        Command GetCommand(PointF worldCoordinate, MouseButtons button, World world);
        Command GetCommand(Keys key, World world);

        void AddPendingCommand(Command c);

        void RemovePendingCommand(Command c);

        Command[] GetPendingCommands();
    }
}
