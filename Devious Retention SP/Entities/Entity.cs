using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    public abstract class Entity : Drawable
    {
        /// <summary>
        /// A command is some action that an entity can do.
        /// </summary>
        public enum Command
        {
            ATTACK,
            MOVE
        }

        public string Name { get; private set; }

        public double X { get; private set; }
        public double Y { get; private set; }

        public Player Player { get; private set; }

        public int ID { get; private set; }
        private static int nextID = 0;

        /// <summary>
        /// Creates an entity, generating a new unique ID for it.
        /// Note that the ID is only unique to the local client;
        /// i.e. this should only be used on the server.
        /// </summary>
        public Entity(Player player, double x, double y)
        {
            ID = nextID++;
            this.Player = player;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Ticks anything that this entity is running; for example,
        /// ticks attack and movement animations if applicable.
        /// </summary>
        public void Tick()
        {
            // Default implementation: do nothing
        }

        public abstract void Draw(Graphics g, PositionTransformation p);

        /// <summary>
        /// Teleports this entity to a given position, disregarding
        /// its ability to move normally. No checking is done here
        /// for whether the new position is outside of the map.
        /// </summary>
        public void Teleport(double newX, double newY)
        {
            this.X = newX;
            this.Y = newY;
        }

        /// <summary>
        /// Changes this entity's position by the given amount,
        /// disregarding its ability to move to/through the space.
        /// </summary>
        public void ChangePosition(double x, double y)
        {
            this.X += x;
            this.Y += y;
            
        }

        /// <summary>
        /// Returns the list of commands that this entity can perform.
        /// </summary>
        public abstract Command[] ValidCommands();

        /// <summary>
        /// Tells this entity to perform a command.
        /// If the command can't be performed by this entity, throws an exception.
        /// </summary>
        /// <param name="entity">The entity to perform the command on, or null if there is none.</param>
        /// <param name="point">The point to perform the command to. May just be the coordinate of the entity.</param>
        /// <param name="command">The command to perform.</param>
        public abstract void SendCommand(Entity entity, PointF point, Command command);

        /// <summary>
        /// Sends a key or key combination as a command to this entity.
        /// The entity will either perform the related command if it exists, or
        /// do nothing if there is no command associated with that key or key
        /// combination.
        /// </summary>
        /// <param name="entity">The entity to perform the command on, or null if there is none.</param>
        /// <param name="point">The point to perform the command to. May just be the coordinate of the entity.</param>
        /// <param name="input">The input to interpret.</param>
        public abstract void SendKeyboardCommand(Entity entity, PointF point, Keys input);

        /// <summary>
        /// Sends a mouse button press or combination to this entity.
        /// The entity will either perform the related command if it exists,
        /// or do nothing if there is no command associated with it.
        /// </summary>
        /// <param name="entity">The entity to perform the command on, or null if there is none.</param>
        /// <param name="point">The point to perform the command to. May just be the coordinate of the entity.</param>
        /// <param name="input">The input to interpret.</param>
        public abstract void SendMouseCommand(Entity entity, PointF point, MouseButtons input);
    }
}
