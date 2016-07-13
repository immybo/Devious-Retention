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
        /// Draws the HP bar of this entity.
        /// Note that not all entities have HP bars. Does nothing if this one doesn't.
        /// </summary>
        /// <param name="g">The graphics to draw the HP bar on</param>
        /// <param name="bounds">The bounds of the HP bar</param>
        public abstract void RenderHPBar(Graphics g, Rectangle bounds);

        /// <summary>
        /// Returns whether or not this entity may be attacked by an enemy.
        /// </summary>
        public abstract bool Attackable();

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
    }
}
