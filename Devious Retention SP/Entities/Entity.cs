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

        // Commands which are waiting for the current command to finish before they can start
        private Queue<Command> pendingCommands;
        private Dictionary<Command, ICallback> callbacks;

        // Synchronous add buffer for pendingCommands
        private Queue<Command> pendingCommandsToAdd;

        // The command which is being executed at the moment
        private Command executingCommand;

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

            pendingCommands = new Queue<Command>();
            pendingCommandsToAdd = new Queue<Command>();
            callbacks = new Dictionary<Command, ICallback>();
        }

        /// <summary>
        /// Ticks anything that this entity is running; for example,
        /// ticks attack and movement animations if applicable.
        /// </summary>
        public void Tick(World world)
        {
            foreach (Command c in pendingCommandsToAdd)
                pendingCommands.Enqueue(c);
            pendingCommandsToAdd.Clear();

            if(executingCommand == null && pendingCommands.Count > 0)
            {
                executingCommand = pendingCommands.Dequeue();
            }

            if(executingCommand != null)
            {
                Command currentCommand = executingCommand;
                bool done = !currentCommand.Tick();
                if (done)
                {
                    if (callbacks.ContainsKey(currentCommand))
                        callbacks[currentCommand].Callback();
                    // Potentially, ticking the command could change the executing command
                    // so we keep a reference to it that they can't change.
                    if (executingCommand == currentCommand)
                        executingCommand = null;
                }
            }

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

        /// <summary>
        /// Adds a command which will execute after the current
        /// command is completed.
        /// If the current command is manually removed rather than
        /// being completed, pending commands will never execute.
        /// </summary>
        public void AddPendingCommand(Command c)
        {
            pendingCommandsToAdd.Enqueue(c);
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
        /// Returns whether or not this entity is currently executing a command.
        /// </summary>
        public bool IsExecutingCommand()
        {
            return executingCommand != null;
        }

        /// <summary>
        /// Returns the currently executing command, or null if
        /// !this.IsExecutingCommand().
        /// </summary>
        public Command GetExecutingCommand()
        {
            return executingCommand;
        }

        /// <summary>
        /// Removes the currently executing command, if there is one,
        /// and adds the given command to be executed.
        /// Also removes all pending commands.
        /// </summary>
        public void OverrideExecutingCommand(Command newCommand)
        {
            pendingCommands.Clear();
            pendingCommandsToAdd.Clear();
            executingCommand = newCommand;
        }

        /// <summary>
        /// Returns whether or not the two given entities
        /// are within the given range of each other.
        /// </summary>
        public static bool WithinRange(IEntity e1, IEntity e2, float range)
        {
            PointF e1C = e1.GetCenterPosition();
            PointF e2C = e2.GetCenterPosition();

            double xDiff = e1C.X - e2C.X;
            double yDiff = e1C.Y - e2C.Y;
            double totalDiff = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            return totalDiff <= range;
        }

        /// <summary>
        /// Returns the point which
        /// - The center point of which is within the given range of the second entity
        /// - The first entity can move to
        /// - Has the shortest path from the first entity's
        ///   current position in the given world
        /// </summary>
        /// <returns></returns>
        public static PointF GetClosestPoint(IEntity firstEntity, IEntity secondEntity, float range, World world)
        {
            PointF attackerPoint = firstEntity.GetCenterPosition();
            PointF defenderPoint = secondEntity.GetCenterPosition();

            PointF vector = new PointF(defenderPoint.X - attackerPoint.X, defenderPoint.Y - attackerPoint.Y);
            double vectorLength = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

            if (vectorLength <= range)
                return attackerPoint;

            PointF inRangeVector = new PointF(vector.X - (float)(vector.X / vectorLength * range), vector.Y - (float)(vector.Y / vectorLength * range));

            PointF newPoint = new PointF(attackerPoint.X + inRangeVector.X, attackerPoint.Y + inRangeVector.Y);
            return newPoint;
        }
    }
}
