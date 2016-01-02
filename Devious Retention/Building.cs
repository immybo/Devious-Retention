using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Buildings are immoveable, player-controlled entities.
    /// Some buildings can train units, and some buildings
    /// can attack enemy units and buildings. Some also gather
    /// resources, whether on their own or in combination with
    /// resource desposits.
    /// </summary>
    public class Building : Entity
    {
        // Each building belongs to a BuildingType, from which most of its attributes can be gotten
        private BuildingType type { get; }

        // The current amount of hitpoints must, however, be stored in each individual building
        private int hitpoints { get; }

        // Unbuilt buildings will have a different image, and will not be able to perform their functions until built fully
        private bool built { get; }

        // A map of UnitTypes to be created, to time until they are created (ticks)
        private Map<UnitType, int> trainingQueue { get; }

        // The resource that this building is on, if any
        private Resource resource;

        // The co-ordinates of the top-left corner of this building
        private double x { get; }
        private double y { get; }

        /// <summary>
        /// A building will get most of its initial attributes from a BuildingType.
        /// Its position must also be given.
        /// </summary>
        public Building(BuildingType type, double x, double y)
        {

        }

        /// <summary>
        /// Attempts to queue a new unit of the given type in this building.
        /// Does nothing if that unit type can't be made at this building type.
        /// </summary>
        public void QueueUnit(UnitType unit)
        {
        }

        /// <summary>
        /// Lowers this building's current hitpoints by the appropriate amount,
        /// from the given damage and damage type.
        /// </summary>
        public void TakeDamage(int damage, int damageType)
        {
        }

        /// <summary>
        /// Attempts to damage the given entity.
        /// If this building can't attack, or the given entity
        /// is a resource, does nothing.
        /// </summary>
        /// <param name="entity"></param>
        public void Attack(Entity entity)
        {

        }

        /// <summary>
        /// Increases the hitpoints of this building such that it retains the same
        /// percentage after a max hitpoints change to the new value.
        /// This should be called on all buildings of a type before it is called
        /// on the BuildingType.
        /// </summary>
        public void ChangeMaxHP(int newMaxHP)
        {

        }

        /// <summary>
        /// Completes another tick of whatever action this building is performing.
        /// Does nothing if this building is not currently performing any actions.
        /// </summary>
        public void Tick()
        {

        }

        /// <summary>
        /// Returns the image that is currently appropriate for this building.
        /// </summary>
        /// <returns></returns>
        public Image GetImage()
        {

        }
    }
}
