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
        // Unique
        public int id { get; private set; }

        public int playerNumber { get; private set; }
        // As most attributes will change only under circumstances where
        // the UnitType will change as well, this provides most attributes
        // so not many fields are needed.
        public UnitType unitType { get; private set; }
        public EntityType type { get; private set; }
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

        // The co-ordinates of the top-left corner of this unit
        public double x { get; set; }
        public double y { get; set; }

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
            this.type = type;
            this.id = id;
            this.x = x;
            this.y = y;
            this.playerNumber = player;

            direction = 0;
            xToMove = -1;
            yToMove = -1;
            hitpoints = type.hitpoints;

            projectiles = new List<Coordinate>();
        }
        
        /// <summary>
        /// Begins this unit's movement towards the given location.
        /// If the unit can't find a path to the location (e.g. the location
        /// is within immoveable terrain), it will move as as close as it can
        /// to it.
        /// </summary>
       /* public void Move(double x, double y)
        {
            xToMove = x;
            yToMove = y;
        }*/

        /// <summary>
        /// Lowers this unit's hitpoints by the appropriate amount.
        /// Also declares this unit as dead if its new amount of hitpoints is below 0.
        /// </summary>
        /// <param name="damage">The integer amount of damage dealt to this unit.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        public int TakeDamage(int damage, int damageType)
        {
            int realDamage = (int)(damage * (100 - unitType.resistances[damageType]) / 100);
            hitpoints -= realDamage;
            return realDamage;
        }

        /// <summary>
        /// Attempts to move within range of and then attack the given entity,
        /// until the entity dies or this unit is commanded to do something else.
        /// Does nothing if the target is a resource.
        /// </summary>
        public void Attack(Entity entity)
        {
            if (entity is Resource) return;

            // the distance to the target
            double distance = Math.Sqrt(Math.Pow(x - entity.x, 2) + Math.Pow(y - entity.y, 2));
            entityToAttack = entity;

            // If it's out of range, move towards it
            if(distance > unitType.range)
            {
                // Figure out what angle (radians) we are from the unit (-y=0, +y=pi)
                double adjacentLength = entity.y - y; // positive if the entity is higher than this
                double oppositeLength = entity.x - x; // positive if the entity is to the right of this

                double angle = Math.Atan2(oppositeLength,adjacentLength);

                // Figure out the target position, which is [range] distance from the entity on that angle
                xToMove = entity.x + Math.Cos(angle) * unitType.range;
                yToMove = entity.y + Math.Sin(angle) * unitType.range;
            }
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
    }
}
