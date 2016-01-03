using System;
using System.Collections.Generic;
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
        private Unit unitToAttack;
        private double xToMove;
        private double yToMove;
        private int direction;

        // If this unit has been tasked to construct a building, this will specify the building
        Building buildingToConstruct;

        // The co-ordinates of the top-left corner of this unit
        public double x { get; private set; }
        public double y { get; private set; }

        /// <summary>
        /// A unit will get all of its attributes from
        /// a UnitType. Its position must also be given.
        /// </summary>
        public Unit(UnitType type, double x, double y)
        {

        }

        /// <summary>
        /// Attempts to move to and then construct the specified building
        /// iff thisunit can construct buildings. If this unit can't,
        /// it will instead merely move to the building location.
        /// </summary>
        public void Build(BuildingType building)
        {

        }
        
        /// <summary>
        /// Begins this unit's movement towards the given location.
        /// If the unit can't find a path to the location (e.g. the location
        /// is within immoveable terrain), it will move as as close as it can
        /// to it.
        /// </summary>
        public void Move(double x, double y)
        {

        }

        /// <summary>
        /// Lowers this unit's hitpoints by the appropriate amount.
        /// Also declares this unit as dead if its new amount of hitpoints is below 0.
        /// </summary>
        /// <param name="damage">The integer amount of damage dealt to this unit.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        public void TakeDamage(int damage, int damageType)
        {

        }

        /// <summary>
        /// Attempts to move within range of and then attack the given entity,
        /// until the entity dies or this unit is commanded to do something else.
        /// Does nothing if the target is a resource.
        /// </summary>
        public void Attack(Entity entity)
        {

        }

        /// <summary>
        /// Increases the hitpoints of this unit such that it retains the same
        /// percentage after a max hitpoints change to the new value.
        /// This should be called on all units of a type before it is called
        /// on the UnitType.
        /// </summary>
        public void ChangeMaxHP(int newMaxHP)
        {

        }

        /// <summary>
        /// Completes another tick of whatever action this unit is performing.
        /// Does nothing if this unit is not performing any action.
        /// </summary>
        public void Tick()
        {

        }

        /// <summary>
        /// Returns the current image for this unit, based on its direction,
        /// animations and type.
        /// </summary>
        public Image GetImage()
        {

        }
    }
}
