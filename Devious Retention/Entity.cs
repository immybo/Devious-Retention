using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// An entity is anything that lies on top of the tile map of the game.
    /// This interface is usually used for rendering, as it simplifies things.
    /// Also, entities are treated the same for selection purposes.
    /// </summary>
    public abstract class Entity
    {
        // TODO More polymorphism for entity. Most things split up into 3, shouldn't have to use instanceof.

        public const int HP_BAR_VERTICAL_OFFSET = 20; // how far up the HP bars are above entities' tops
        
        public EntityType Type
        {
            get
            {
                return GetEntityType();
            }
        }

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
            Init(player, x, y);
        }

        /// <summary>
        /// Creates an entity with the given ID.
        /// </summary>
        public Entity(Player player, int id, double x, double y)
        {
            this.ID = id;
            Init(player, x, y);
        }

        private void Init(Player player, double x, double y)
        {
            this.Player = player;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Returns the appropriate image for this entity at this point in time
        /// </summary>
        public abstract Image GetImage();

        /// <summary>
        /// Ticks anything that this entity is running.
        /// </summary>
        public virtual void Tick()
        {
            // Default implementation: Nothing to tick
        }

        /// <summary>
        /// Draws the HP bar of this entity.
        /// Note that not all entities have HP bars. Does nothing if this one doesn't.
        /// </summary>
        /// <param name="g">The graphics to draw the HP bar on</param>
        /// <param name="bounds">The bounds of the HP bar</param>
        public abstract void RenderHPBar(Graphics g, Rectangle bounds);

        /// <summary>
        /// Returns whether or not this entity may be attacked by an enemy.
        /// </summary>
        public virtual bool Attackable()
        {
            return false;
        }

        /// <summary>
        /// Attempts to perform one tick's worth of damage from the source
        /// entity onto this entity.
        /// </summary>
        public virtual void Damage(Entity source)
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
        /// Kills this entity if it can be killed.
        /// </summary>
        public virtual void Kill()
        {
            throw new NotImplementedException("Attempting to kill an entity which can't be killed.");
        }

        /// <summary>
        /// Returns whether or not this entity is attacking an enemy.
        /// </summary>
        public virtual bool Attacking()
        {
            return false;
        }

        /// <summary>
        /// Returns the entity that this entity is attacking, if it
        /// is attacking one.
        /// </summary>
        public virtual Entity AttackedEntity()
        {
            throw new NotImplementedException("Attempting to get the attacked entity of an entity that can't attack.");
        }

        /// <summary>
        /// Attempts to attack the given entity with this one.
        /// </summary>
        public virtual void BeginAttacking(Entity defender)
        {
            throw new NotImplementedException("Attempting to attack with an entity (" + this.Type.name + ") that can't attack.");
        }

        /// <summary>
        /// Stops this entity from attacking anything that it was
        /// attacking before. No operation is done if this entity
        /// was not attacking anything.
        /// </summary>
        public virtual void HaltAttacking()
        {
            throw new NotImplementedException("Attempting to halt attacking on an entity for which the method isn't implemented.");
        }

        /// <summary>
        /// Returns whether or not this entity can attack enemies.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanAttack()
        {
            return false;
        }

        /// <summary>
        /// Teleports this entity to a given position, disregarding
        /// its ability to move normally. No checking is done here
        /// for whether the new position is outside of the map.
        /// </summary>
        public void Move(double newX, double newY)
        {
            this.X = newX;
            this.Y = newY;
        }

        /// <summary>
        /// Begins the process of this entity moving to the given
        /// position. 
        /// </summary>
        public virtual void BeginMovement(double newX, double newY)
        {
            throw new NotImplementedException("Attempting to move with an entity (" + this.Type.name + ") that can't move.");
        }

        public virtual void HaltMovement()
        {
            throw new NotImplementedException("Attempting to halt movement on an entity for which the method isn't implemented.");
        }

        /// <summary>
        /// Returns whether or not this entity is moving.
        /// </summary>
        public virtual bool Moving()
        {
            return false;
        }

        /// <summary>
        /// Returns whether or not this entity can move.
        /// </summary>
        public virtual bool CanMove()
        {
            return false;
        }

        /// <summary>
        /// Returns the entity type of this entity.
        /// </summary>
        public abstract EntityType GetEntityType();

        /// <summary>
        /// Draws this entity.
        /// </summary>
        /// <param name="g">The graphics to draw the entity on</param>
        /// <param name="bounds">The bounds to draw the entity within</param>
        public void Render(Graphics g, Rectangle bounds)
        {
            g.DrawImage(GetImage(), bounds);

            // HP bar goes above the rest of the image
            Rectangle hpBarBounds = new Rectangle();
            hpBarBounds.X = bounds.X;
            hpBarBounds.Y = bounds.Y - 35;
            hpBarBounds.Width = bounds.Width;
            hpBarBounds.Height = 20;

            RenderHPBar(g, hpBarBounds);
        }

        /// <summary>
        /// Changes this entity's position by the given amount
        /// </summary>
        public void ChangePosition(double x, double y)
        {
            this.X += x;
            this.Y += y;
        }

        /// <summary>
        /// If the attacker can attack the defender, returns the
        /// amount of damage it would deal.
        /// </summary>
        public static int GetDamage(Entity attacker, Entity defender)
        {
            if (!attacker.CanAttack())
                throw new ArgumentException("Trying to get the damage with an attacker that can't attack.");
            if (!defender.Attackable())
                throw new ArgumentException("Trying to get the damage with a defender that can't be attacked.");

            int damageType = attacker.Type.damageType;
            int damage = attacker.Type.damage;
            int realDamage = (int)((double)damage * (100 - defender.Type.resistances[damageType]) / 100);
            return realDamage;
        }
    }
}
