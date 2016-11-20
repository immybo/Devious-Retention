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
    /// <summary>
    /// Units are entities that:
    /// - Can be attacked
    /// - Can move
    /// - Can usually attack
    /// - Often have special abilities
    /// </summary>
    public abstract class Unit : Entity, Attackable
    {
        public int MaxHitpoints { get; protected set; }
        public int Hitpoints { get; protected set; }

        /// <summary>
        /// How far this unit can move every tick
        /// </summary>
        public float MovementSpeed { get; private set; }

        public Unit(Player player, double x, double y, double size, float movementSpeed, string name)
            : base(player, x, y, size, name)
        {
            this.MovementSpeed = movementSpeed;
        }
        
        public abstract void Damage(int amount, int damageType);
        public virtual void Heal(int amount)
        {
            if (Hitpoints < MaxHitpoints - amount)
                Hitpoints += amount;
            else
                Hitpoints = MaxHitpoints;
        }
        public virtual bool IsDead()
        {
            return Hitpoints <= 0;
        }

        public override Command GetCommand(PointF worldCoordinate, MouseButtons button, World world)
        {
            if (button.Equals(MouseButtons.Right))
            {
                return new MoveCommand(this, worldCoordinate, world);
            }

            return base.GetCommand(worldCoordinate, button, world);
        }

        public override Command GetCommand(Keys key, World world)
        {
            return base.GetCommand(key, world);
        }

        /// <summary>
        /// Schedules a command on this unit to move within the given range of 
        /// another entity. Calls the callback method on the given
        /// object when this command is finished. Does not move, but does
        /// callback, if this unit was already within range. Moves only
        /// to within range of the other entity's original position if it
        /// moves.
        /// </summary>
        public void MoveWithinRange(IEntity other, float range, ICallback callback, World world)
        {
            if(Entity.WithinRange(this, other, range))
            {
                callback.Callback();
                return;
            }

            PointF newPoint = Entity.GetClosestPoint(this, other, range, world);
            MoveCommand movement = new MoveCommand(this, newPoint, world);
            movement.Execute();
            this.RegisterCallback(movement, callback);
        }
    }
}
