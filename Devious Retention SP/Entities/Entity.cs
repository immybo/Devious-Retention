using Devious_Retention_SP.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    public abstract class Entity : IEntity
    {
        public string Name { get; private set; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Size { get; private set; }

        public Player Player { get; private set; }

        public int ID { get; private set; }
        private static int nextID = 0;

        // Commands which are ongoing and must be ticked
        private List<Command> pendingCommands;

        private List<Command> pendingCommandsToAdd;
        private List<Command> pendingCommandsToRemove;

        private Dictionary<Command, ICallback> callbacks;

        /// <summary>
        /// Creates an entity, generating a new unique ID for it.
        /// Note that the ID is only unique to the local client;
        /// i.e. this should only be used on the server.
        /// </summary>
        public Entity(Player player, double x, double y, double size, string name)
        {
            ID = nextID++;
            this.Player = player;
            this.X = x;
            this.Y = y;
            this.Size = size;
            this.Name = name;
            pendingCommands = new List<Command>();
            pendingCommandsToAdd = new List<Command>();
            pendingCommandsToRemove = new List<Command>();
            callbacks = new Dictionary<Command, ICallback>();
        }

        /// <summary>
        /// Ticks anything that this entity is running; for example,
        /// ticks attack and movement animations if applicable.
        /// </summary>
        public void Tick(World world)
        {
            foreach (Command c in pendingCommandsToAdd)
            {
                // TODO there should only really be one pending command... tidy this up
                pendingCommands.Add(c);
            }
            foreach (Command c in pendingCommandsToRemove)
            {
                if (callbacks.ContainsKey(c))
                    callbacks[c].Callback();
                pendingCommands.Remove(c);
            }
            pendingCommandsToAdd.Clear();
            pendingCommandsToRemove.Clear();

            foreach (Command c in pendingCommands)
                if (!c.Tick())
                    pendingCommandsToRemove.Add(c);

            if(this is Attackable && ((Attackable)this).IsDead())
            {
                world.RemoveEntity(this);
            }
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
        
        public PointF GetPosition()
        {
            return new PointF((float)X, (float)Y);
        }

        public PointF GetCenterPosition()
        {
            return new PointF((float)(X + Size/2), (float)(Y + Size/2));
        }

        public virtual Command GetCommand(PointF worldCoordinate, MouseButtons button, World world)
        {
            return new NullCommand();
        }
        public virtual Command GetCommand(Keys key, World world)
        {
            return new NullCommand();
        }

        public void AddPendingCommand(Command c)
        {
            pendingCommandsToAdd.Add(c);
        }

        public void RemovePendingCommand(Command c)
        {
            pendingCommandsToRemove.Remove(c);
        }

        public Command[] GetPendingCommands()
        {
            return pendingCommands.ToArray();
        }

        public void RegisterCallback(Command command, ICallback callback)
        {
            callbacks.Add(command, callback);
        }

        /// <summary>
        /// Returns whether or not the two given entities
        /// are within the given range of each other.
        /// </summary>
        public static bool WithinRange(IEntity e1, IEntity e2, float range)
        {
            double xDiff = e1.X - e2.X;
            double yDiff = e1.Y - e2.Y;
            double totalDiff = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            return totalDiff <= range;
        }

        /// <summary>
        /// Returns the point which
        /// - Is within the given range of the second entity
        /// - The first entity can move to
        /// - Has the shortest path from the first entity's
        ///   current position in the given world
        /// </summary>
        /// <returns></returns>
        public static PointF GetClosestPoint(IEntity firstEntity, IEntity secondEntity, float range, World world)
        {
            PointF defenderPoint = firstEntity.GetCenterPosition();
            PointF attackerPoint = secondEntity.GetCenterPosition();

            PointF vector = new PointF(defenderPoint.X - attackerPoint.X, defenderPoint.Y - attackerPoint.Y);
            double vectorLength = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

            if (vectorLength <= range - 0.5)
                return attackerPoint;

            PointF inRangeVector = new PointF((float)(vector.X * (range/vectorLength)), (float)(vector.Y * (range/vectorLength)));

            PointF newPoint = new PointF(defenderPoint.X - inRangeVector.X, defenderPoint.Y - inRangeVector.Y);
            return newPoint;
        }
    }
}
