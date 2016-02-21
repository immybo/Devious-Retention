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
    /// resource desposits. Buildings are created by placing a
    /// foundation and waiting.
    /// </summary>
    public class Building : Entity
    {
        public static int nextID { get; private set; }
        // Unique
        public int id { get; private set; }

        public int playerNumber { get; private set; }

        // Each building belongs to a BuildingType, from which most of its attributes can be gotten
        public BuildingType buildingType { get; private set; }
        public EntityType type { get; private set; }

        // The current amount of hitpoints must, however, be stored in each individual building
        public int hitpoints;

        // Unbuilt buildings will have a different image, and will not be able to perform their functions until built fully
        public bool built;

        // A queue of UnitTypes to be created
        public Queue<UnitType> trainingQueue { get; private set; }
        // How long until the next UnitType on the queue is created (ticks) - only matters if trainingQueue isn't empty
        public int trainingQueueTime { get; private set; }

        // The resource that this building is on, if any
        public Resource resource { get; set; }

        // The co-ordinates of the top-left corner of this building
        public double x { get; set; }
        public double y { get; set; }

        public Entity entityToAttack;

        public int attackTick = 0;

        public Image image
        {
            get
            {
                return buildingType.image;
            }
        }

        // Any projectiles currently belonging to this building
        public List<Coordinate> projectiles;

        /// <summary>
        /// A building will get most of its initial attributes from a BuildingType.
        /// Its position must also be given.
        /// </summary>
        public Building(BuildingType type, int id, double x, double y, int player)
        {
            this.buildingType = type;
            this.type = type;
            this.id = id;
            this.x = x;
            this.y = y;
            this.playerNumber = player;

            trainingQueue = new Queue<UnitType>();
            trainingQueueTime = 0;
            built = false;

            projectiles = new List<Coordinate>();
        }

        /// <summary>
        /// Attempts to queue a new unit of the given type in this building.
        /// Does nothing if that unit type can't be made at this building type.
        /// </summary>
        public void QueueUnit(UnitType unit)
        {
            // Check if that UnitType can actually be trained
            if (!buildingType.trainableUnits.Contains(unit.name))
                return;

            trainingQueue.Enqueue(unit);
            if (trainingQueue.Count == 1) trainingQueueTime = unit.trainingTime;
        }

        /// <summary>
        /// Lowers this building's current hitpoints by the appropriate amount,
        /// from the given damage and damage type.
        /// </summary>
        public int TakeDamage(int damage, int damageType)
        {
            int realDamage = (int)(damage * (100 - buildingType.resistances[damageType]) / 100);
            hitpoints -= realDamage;
            return realDamage;
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
            double newHPMultiplier = (double)newMaxHP / buildingType.hitpoints;
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
