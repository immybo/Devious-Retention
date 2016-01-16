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
    /// The main purpose of most units is to fight,
    /// however some units can also construct buildings.
    /// </summary>
    public class Unit : Entity
    {
        public int player { get; private set; }
        // As most attributes will change only under circumstances where
        // the UnitType will change as well, this provides most attributes
        // so not many fields are needed.
        public UnitType type { get; private set; }
        // In addition to the maximum hitpoints provided by the type,
        // a unit must keep track of its current hitpoints.
        public int hitpoints { get; private set; }

        // If this unit hasn't been commanded to move or attack, these will be
        // null,-1,-1 (respectively). If it has, however, it will attempt to move
        // towards the spot or the unit, or attack the unit if it's within range.
        // Attacking will take priority (although they should never both be active).
        private Entity entityToAttack;
        private double xToMove;
        private double yToMove;
        private int direction;

        // If this unit has been tasked to construct a building, this will specify the building
        Building buildingToConstruct;

        // The co-ordinates of the top-left corner of this unit
        public double x { get; private set; }
        public double y { get; private set; }

        // The frame of attack animation this unit is on; when this reaches type.attackTicks, this unit will attack
        private int attackTick = 0;

        /// <summary>
        /// A unit will get all of its attributes from
        /// a UnitType. Its position must also be given.
        /// </summary>
        public Unit(UnitType type, double x, double y, int player)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.player = player;
            direction = 0;
            xToMove = -1;
            yToMove = -1;
            hitpoints = type.hitpoints;
        }

        /// <summary>
        /// Attempts to move to and then construct the specified building
        /// iff thisunit can construct buildings. If this unit can't,
        /// it will instead merely move to the building location.
        /// </summary>
        public void Build(Building building)
        {
            buildingToConstruct = building;
            entityToAttack = null;
            // Figure out where to move to in order to be able to construct the building
            
            // If we're to the left of the building, go to its left side
            if (this.x < building.x) xToMove = building.x;
            // If we're on the right of the building, go to its right side
            else if (this.x > building.x + building.type.size) xToMove = building.x + building.type.size;
            // Otherwise we must just be able to retain the same x ordinate
            else xToMove = this.x;
            
            if (this.y < building.y) yToMove = building.y;
            else if (this.y > building.y + building.type.size) yToMove = building.y + building.type.size;
            else yToMove = this.y;
        }
        
        /// <summary>
        /// Begins this unit's movement towards the given location.
        /// If the unit can't find a path to the location (e.g. the location
        /// is within immoveable terrain), it will move as as close as it can
        /// to it.
        /// </summary>
        public void Move(double x, double y)
        {
            xToMove = x;
            yToMove = y;
        }

        /// <summary>
        /// Lowers this unit's hitpoints by the appropriate amount.
        /// Also declares this unit as dead if its new amount of hitpoints is below 0.
        /// </summary>
        /// <param name="damage">The integer amount of damage dealt to this unit.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        public void TakeDamage(int damage, int damageType)
        {
            int realDamage = (int)(damage * (100 - type.resistances[damageType]) / 100);
            hitpoints -= realDamage;
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
            double distance = Math.Sqrt(Math.Pow(x - entity.GetX(), 2) + Math.Pow(y - entity.GetY(), 2));
            entityToAttack = entity;
            buildingToConstruct = null;

            // If it's out of range, move towards it
            if(distance > type.range)
            {
                // Figure out what angle (radians) we are from the unit (-y=0, +y=pi)
                double adjacentLength = entity.GetY() - y; // positive if the entity is higher than this
                double oppositeLength = entity.GetX() - x; // positive if the entity is to the right of this

                double angle = Math.Atan2(oppositeLength,adjacentLength);

                // Figure out the target position, which is [range] distance from the entity on that angle
                xToMove = entity.GetX() + Math.Cos(angle) * type.range;
                yToMove = entity.GetY() + Math.Sin(angle) * type.range;
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
            double newHPMultiplier = (double)newMaxHP / type.hitpoints;
            hitpoints = (int)(hitpoints * newHPMultiplier);
        }

        /// <summary>
        /// Completes another tick of whatever action this unit is performing.
        /// Does nothing if this unit is not performing any action.
        /// </summary>
        public void Tick()
        {
            // Only performs one action every tick (e.g. can't move AND attack)
            
            if (AttackTick()) return;
            if (ConstructTick()) return;
            MoveTick();
        }

        /// <summary>
        /// Finds the closest path to and then travels one tick towards this unit's target
        /// location (does nothing if this unit has no target location).
        /// </summary>
        private void MoveTick()
        {
        }

        /// <summary>
        /// Produces one tick of work towards constructing the building that this
        /// unit is assigned to construct. Does nothing if this unit is not adjacent to
        /// that building, or if the building's construction has been completed.
        /// Returns whether or not it completed a tick of construction.
        /// </summary>
        private bool ConstructTick()
        {
            // Not building anything
            if (buildingToConstruct == null) return false;

            // Too far apart
            double distance = Math.Sqrt(Math.Pow(x - buildingToConstruct.x, 2) + Math.Pow(y - buildingToConstruct.y, 2));
            if (distance > GameInfo.ADJACENT_DISTANCE)
                return false;

            // TODO
            return false;
        }

        /// <summary>
        /// Produces one tick of work towards this unit's next attack.
        /// Does nothing if this unit has no target to attack, or if this unit is not
        /// within range of its target.
        /// Returns whether or not it completed a tick of attacking.
        /// </summary>
        private bool AttackTick()
        {
            return false;
        }

        /// <summary>
        /// Returns the current image for this unit, based on its direction,
        /// animations and type.
        /// </summary>
        public Image GetImage()
        {
            return type.image;
        }

        /// <summary>
        /// Returns the size of this unit's type.
        /// </summary>
        public double GetSize()
        {
            return type.size;
        }

        public double GetX()
        {
            return x;
        }
        public double GetY()
        {
            return y;
        }
        public int GetLOS()
        {
            return type.lineOfSight;
        }
        public int GetPlayerNumber()
        {
            return player;
        }
    }
}
