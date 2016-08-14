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
        /// Returns whether or not this entity may be attacked by an enemy.
        /// </summary>
        public abstract bool Attackable();
        /// <summary>
        /// Returns whether or not this entity can attack enemies.
        /// </summary>
        public abstract bool CanAttack();
        /// <summary>
        /// Returns whether or not this entity can move.
        /// </summary>
        public abstract bool CanMove();

        /// <summary>
        /// Attempts to inflict the given amount of damage of the
        /// given type onto this entity.
        /// </summary>
        public virtual void Damage(double damage, int damageType)
        {
            throw new NotImplementedException("Attempting to damage an entity which can't be damaged.");
        }

        /// <summary>
        /// Returns whether or not this entity is considered to be dead.
        /// </summary>
        public virtual bool IsDead()
        {
            throw new NotImplementedException("Attempting to read the life status of an entity which doesn't have one.");
        }

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

        public abstract Command[] ValidCommands();
        public abstract void SendCommand(Entity entity, PointF point, Command command);
        public abstract void SendKeyboardCommand(Entity entity, PointF point, Keys input);
        public abstract void SendMouseCommand(Entity entity, PointF point, MouseButtons input);
    }
}
