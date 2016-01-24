﻿using System;
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

        // The co-ordinates of the top-left corner of this unit
        public double x { get; private set; }
        public double y { get; private set; }

        // The frame of attack animation this unit is on; when this reaches type.attackTicks, this unit will attack
        private int attackTick = 0;

        /// <summary>
        /// A unit will get all of its attributes from
        /// a UnitType. Its position must also be given.
        /// </summary>
        public Unit(UnitType type, int id, double x, double y, int player)
        {
            this.type = type;
            this.id = id;
            this.x = x;
            this.y = y;
            this.player = player;

            direction = 0;
            xToMove = -1;
            yToMove = -1;
            hitpoints = type.hitpoints;
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

        public int GetID()
        {
            return id;
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
