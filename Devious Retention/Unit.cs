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
        private Entity entityToAttack;

        private bool isMoving;
        private double toMoveX;
        private double toMoveY;

        // The frame of attack animation this unit is on; when this reaches type.attackTicks, this unit will be considered ready to attack
        public int attackTick = -1;
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
        public Unit(UnitType type, int id, double x, double y, Player player)
            : base(player, id, x, y)
        {
            Init(type);
        }
        /// <summary>
        /// Builds a unit with a new unique ID.
        /// </summary>
        public Unit(UnitType type, double x, double y, Player player)
            : base(player, x, y)
        {
            Init(type);
        }

        private void Init(UnitType type)
        {
            this.unitType = type;
            hitpoints = type.hitpoints;

            projectiles = new List<Coordinate>();
        }
        
        public override void Damage(Entity source)
        {
            hitpoints -= GetDamage(source, this);
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

        public override Image GetImage()
        {
            return unitType.image;
        }

        public override void Tick()
        {
            if (Attacking())
            {
                if (entityToAttack.IsDead())
                    HaltAttacking();

                attackTick++;
                if (attackTick == unitType.attackTicks)
                {
                    attackTick = 0;
                    entityToAttack.Damage(this);
                }
            }
            if (Moving())
            {
                movementTick++;
                double distToEnd = Math.Sqrt(Math.Pow(toMoveX - X, 2) + Math.Pow(toMoveY - Y, 2));
                double tickSpeed = Type.speed / 1000 * GameInfo.TICK_TIME;

                if(distToEnd < tickSpeed)
                {
                    Move(toMoveX, toMoveY);
                    isMoving = false;
                }
                else
                {
                    // Figure out how much to move in each direction
                    double xToGo = toMoveX - X;
                    double yToGo = toMoveY - Y;
                    double total = Math.Abs(xToGo) + Math.Abs(yToGo);
                    double xProportion = xToGo / total;
                    double yProportion = yToGo / total;

                    double newX = X + xProportion * tickSpeed;
                    double newY = Y + yProportion * tickSpeed;
                    Move(newX, newY);
                }
            }
        }

        /// <summary>
        /// Returns whether or not this unit attacked on the previous
        /// tick, if it is attacking.
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

        public override bool Attackable()
        {
            return true;
        }

        public override bool Attacking()
        {
            return entityToAttack != null;
        }

        public override bool CanAttack()
        {
            return true;
        }
        
        public override void BeginMovement(double newX, double newY)
        {
            isMoving = true;
            toMoveX = newX;
            toMoveY = newY;
        }

        public override void HaltMovement()
        {
            isMoving = false;
        }

        public override bool Moving()
        {
            return isMoving;
        }

        public override bool CanMove()
        {
            return true;
        }

        public override EntityType GetEntityType()
        {
            return unitType;
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
