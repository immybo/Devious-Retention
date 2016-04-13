using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Units are a type of entity which can move.
    /// The purpose of units is to fight the enemy's units and buildings.
    /// </summary>
    public class Unit : Entity
    {
        public static int nextID { get; private set; }
        
        // As most attributes will change only under circumstances where
        // the UnitType will change as well, this provides most attributes
        // so not many fields are needed.
        public UnitType unitType { get; private set; }
        // In addition to the maximum hitpoints provided by the type,
        // a unit must keep track of its current hitpoints.
        public int hitpoints;

        // If this unit hasn't been commanded to move or attack, these will be
        // null,-1,-1 (respectively). If it has, however, it will attempt to move
        // towards the spot or the unit, or attack the unit if it's within range.
        // Attacking will take priority (although they should never both be active).
        public Entity entityToAttack;
        public double xToMove;
        public double yToMove;
        private int direction;

        // The frame of attack animation this unit is on; when this reaches type.attackTicks, this unit will be considered ready to attack
        public int attackTick = 0;
        // " movement animation
        public int movementTick = 0;

        public Image image
        {
            get
            {
                return unitType.image;
            }
        }

        // Any projectiles this unit currently has
        public List<Coordinate> projectiles;

        /// <summary>
        /// A unit will get all of its attributes from
        /// a UnitType. Its position must also be given.
        /// </summary>
        public Unit(UnitType type, int id, double x, double y, int player)
        {
            this.unitType = type;
            this.Type = type;
            this.ID = id;
            this.X = x;
            this.Y = y;
            this.PlayerNumber = player;

            direction = 0;
            xToMove = -1;
            yToMove = -1;
            hitpoints = type.hitpoints;

            projectiles = new List<Coordinate>();
        }
        
        /// <param name="damage">The integer amount of damage dealt to this unit.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        public int TakeDamage(int damage, int damageType)
        {
            int realDamage = (int)(damage * (100 - unitType.resistances[damageType]) / 100);
            hitpoints -= realDamage;
            return realDamage;
        }

        /// <summary>
        /// Increases the hitpoints of this unit such that it retains the same
        /// percentage after a max hitpoints change to the new value.
        /// This should be called on all units of a type before it is called
        /// on the UnitType.
        /// </summary>
        public void ChangeMaxHP(int newMaxHP)
        {
            double newHPMultiplier = (double)newMaxHP / unitType.hitpoints;
            hitpoints = (int)(hitpoints * newHPMultiplier);
        }

        /// <summary>
        /// Resets the next ID to 0.
        /// </summary>
        public static void ResetNextID()
        {
            nextID = 0;
        }
        /// <summary>
        /// Increments the next ID by 1.
        /// </summary>
        public static void IncrementNextID()
        {
            nextID++;
        }

        public override Image GetImage()
        {
            return unitType.image;
        }

        public override void RenderHPBar(Graphics g, Rectangle bounds)
        {
            Brush brush;

            // Determine the colour
            double ratio = (double)hitpoints / Type.hitpoints;
            int barWidth = (int)(bounds.Width * ratio);
            if (ratio > 0.75) brush = Brushes.Green;
            else if (ratio > 0.3) brush = Brushes.Yellow;
            else brush = Brushes.Red;

            g.FillRectangle(brush, bounds.X, bounds.Y, barWidth, bounds.Height);
            g.DrawRectangle(Pens.Black, bounds);

            // Draw the border
            g.DrawRectangle(GameInfo.PLAYER_PENS[PlayerNumber], bounds);
        }
    }
}
