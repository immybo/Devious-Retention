using System;
using System.Collections.Generic;
using System.Drawing;
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
        public int player { get; private set; }

        // Each building belongs to a BuildingType, from which most of its attributes can be gotten
        public BuildingType type { get; private set; }

        // The current amount of hitpoints must, however, be stored in each individual building
        public int hitpoints { get; private set; }

        // Unbuilt buildings will have a different image, and will not be able to perform their functions until built fully
        public bool built { get; private set; }

        // A queue of UnitTypes to be created
        public Queue<UnitType> trainingQueue { get; private set; }
        // How long until the next UnitType on the queue is created (ticks) - only matters if trainingQueue isn't empty
        public int trainingQueueTime { get; private set; }

        // The resource that this building is on, if any
        public Resource resource { get; private set; }

        // The co-ordinates of the top-left corner of this building
        public double x { get; private set; }
        public double y { get; private set; }

        private Entity entityToAttack;

        private int attackTick = 0;

        /// <summary>
        /// A building will get most of its initial attributes from a BuildingType.
        /// Its position must also be given.
        /// </summary>
        public Building(BuildingType type, double x, double y, int player)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.player = player;
            trainingQueue = new Queue<UnitType>();
            trainingQueueTime = 0;
            built = false;
        }

        /// <summary>
        /// Attempts to queue a new unit of the given type in this building.
        /// Does nothing if that unit type can't be made at this building type.
        /// </summary>
        public void QueueUnit(UnitType unit)
        {
            // Check if that UnitType can actually be trained
            if (!type.trainableUnits.Contains(unit.name))
                return;

            trainingQueue.Enqueue(unit);
            if (trainingQueue.Count == 1) trainingQueueTime = unit.trainingTime;
        }

        /// <summary>
        /// Lowers this building's current hitpoints by the appropriate amount,
        /// from the given damage and damage type.
        /// </summary>
        public void TakeDamage(int damage, int damageType)
        {
            int realDamage = (int)(damage * (100 - type.resistances[damageType]) / 100);
            hitpoints -= realDamage;
        }

        /// <summary>
        /// Attempts to damage the given entity.
        /// If this building can't attack, or the given entity
        /// is a resource, does nothing.
        /// </summary>
        /// <param name="entity"></param>
        public void Attack(Entity entity)
        {
            if (entity is Resource) return;
            entityToAttack = entity;
        }

        /// <summary>
        /// Increases the hitpoints of this building such that it retains the same
        /// percentage after a max hitpoints change to the new value.
        /// This should be called on all buildings of a type before it is called
        /// on the BuildingType.
        /// </summary>
        public void ChangeMaxHP(int newMaxHP)
        {
            double newHPMultiplier = (double)newMaxHP / type.hitpoints;
            hitpoints = (int)(hitpoints * newHPMultiplier);
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
            return type.image;
        }

        /// <summary>
        /// Returns the size of this building's type
        /// </summary>
        public Double GetSize()
        {
            return type.size;
        }

        public Double GetX()
        {
            return x;
        }
        public Double GetY()
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
