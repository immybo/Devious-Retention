﻿using System;
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
        // Each building belongs to a BuildingType, from which most of its attributes can be gotten
        public BuildingType buildingType { get; private set; }

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

        public Entity entityToAttack;

        public int attackTick = -1;

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
        public Building(BuildingType type, int id, double x, double y, Player player)
            : base(player, id, x, y)
        {
            Init(type);
        }
        /// <summary>
        /// Builds a building with a new unique ID.
        /// </summary>
        public Building(BuildingType type, double x, double y, Player player)
            : base(player, x, y)
        {
            Init(type);
        }

        private void Init(BuildingType type)
        {
            this.buildingType = type;

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

        public override void Damage(Entity source)
        {
            hitpoints -= GetDamage(source, this);
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

        public override void BeginAttacking(Entity defender)
        {
            entityToAttack = defender;
            attackTick = 0;
        }

        public override void HaltAttacking()
        {
            attackTick = -1;
            entityToAttack = null;
        }

        public override bool Attacking()
        {
            return entityToAttack != null;
        }

        public override bool CanAttack()
        {
            return buildingType.aggressive;
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

        public override Image GetImage()
        {
            return buildingType.image;
        }

        public override void Tick()
        {
            if (Attacking())
            {
                if (entityToAttack.IsDead())
                    HaltAttacking();

                attackTick++;
                if (attackTick == buildingType.attackTicks)
                {
                    attackTick = 0;
                    entityToAttack.Damage(this);
                }
            }
        }

        /// <summary>
        /// Returns whether or not this building attacked on 
        /// the previous tick, if it was attacking.
        /// </summary>
        public override bool JustAttacked()
        {
            return attackTick == 0;
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
            g.DrawRectangle(Player.Pen, bounds);
        }

        public override bool Attackable()
        {
            return true;
        }
        public override EntityType GetEntityType()
        {
            return buildingType;
        }

        public override bool IsDead()
        {
            return hitpoints <= 0;
        }

        public override void Kill()
        {
            hitpoints = 0;
        }

        public override Entity AttackedEntity()
        {
            return entityToAttack;
        }
    }
}
