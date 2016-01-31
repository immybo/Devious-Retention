using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Devious_Retention
{
    /// <summary>
    /// A UnitType defines attributes for a specific type of unit.
    /// These attributes can be changed through technologies, and so
    /// each client has a set of UnitTypes specific to it.
    /// </summary>
    public class UnitType : ICloneable, EntityType
    {
        public string name { get; private set; }
        public string description { get; private set; }
        // The initial amount of hitpoints for units of this type.
        public int hitpoints { get; set; }
        // The base amount of damage that this unit will do to other units per attack.
        public int damage { get; set; }
        // So that unit counters can effectively be made, there are multiple different types of damage.
        // Although every attack must do at least one damage, a resistance against a type of damage reduces it by a percentage.
        public int damageType { get; private set; }
        public int[] resistances { get; set; }
        public int trainingTimeMillis { get; set; }
        // Ticks
        public int trainingTime { get; set; }
        // How many tiles units of this type can move per second
        public double speed { get; set; }
        // How many tiles this unit can "see"
        public int lineOfSight { get; set; }
        // The radius of the collision circle, and the size of the image along each axis, of this unit
        public double size { get; private set; }
        // Every UnitType can only have up to one prerequisite technology
        // Before this technology is researched, no units of this type can be created
        // Not every UnitType has to have a prerequisite, however
        public String prerequisite { get; private set; }

        // An aggressive unit will attempt to attack nearby enemy units
        public bool aggressive { get; private set; }
        // What type of unit this is, e.g. infantry, armored, flying, ship
        public int type { get; private set; }

        // How many tiles away this unit can attack from
        public int range { get; set; }
        // How many ticks it takes this unit to attack
        public int attackTicks { get; private set; }
        private int attackSpeedMilliseconds;

        public int[] resourceCosts { get; set; }

        private string imageName;
        public Image image { get; private set; }
        private string iconName;
        public Image icon { get; private set; }

        public List<Unit> units { get; set; }

        /// <summary>
        /// Anything attempting to create a UnitType from a file must first
        /// parse the string into these attributes.
        /// </summary>
        public UnitType(string name, int hitpoints, int damage, int damageType, double size, int lineOfSight, int[] resistances, int trainingTimeMillis, double speed,
                        string prerequisite, bool aggressive, int type, string imageName, string iconName, int range, int attackSpeedMilliseconds, int[] resourceCosts, string description)
        {
            this.name = name;
            this.description = description;
            this.hitpoints = hitpoints;
            this.damage = damage;
            this.damageType = damageType;
            this.size = size;
            this.lineOfSight = lineOfSight;
            this.resistances = resistances;
            this.trainingTimeMillis = trainingTimeMillis;
            this.trainingTime = (int)(trainingTimeMillis / GameInfo.TICK_TIME);
            this.speed = speed;
            this.prerequisite = prerequisite;
            this.aggressive = aggressive;
            this.type = type;
            this.imageName = imageName;
            this.range = range;
            this.attackSpeedMilliseconds = attackSpeedMilliseconds;
            this.attackTicks = (int)(attackSpeedMilliseconds / GameInfo.TICK_TIME);
            if (attackTicks <= 0) attackTicks = 1;
            this.resourceCosts = resourceCosts;

            try
            {
                image = Image.FromFile(GameInfo.UNIT_IMAGE_BASE + imageName);
                icon = Image.FromFile(GameInfo.UNIT_ICON_BASE + iconName);
            }
            catch(IOException)
            {
                image = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
                icon = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
            }

            units = new List<Unit>();
        }

        /// <summary>
        /// Returns:
        /// "name hitpoints damage damageType lineOfSight size resistance1 resistance2 .. resistanceX trainingTime speed
        ///     prerequisiteName aggressive type imageName iconName range attackMilliseconds resourcecost1 resourcecost2 ... resourcecostx description"
        /// </summary>
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            builder.Append(hitpoints + " ");
            builder.Append(damage + " ");
            builder.Append(damageType + " ");
            builder.Append(lineOfSight + " ");
            builder.Append(size + " ");
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                builder.Append(resistances[i] + " ");
            builder.Append(trainingTime + " ");
            builder.Append(speed + " ");
            builder.Append(prerequisite + " ");
            builder.Append(aggressive + " ");
            builder.Append(type + " ");
            builder.Append(imageName + " ");
            builder.Append(iconName + " ");
            builder.Append(range + " ");
            builder.Append(attackSpeedMilliseconds + " ");
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                builder.Append(resourceCosts[i] + " ");
            builder.Append(description + " ");

            return builder.ToString();
        }

        /// <summary>
        /// Returns a new UnitType completely identical to this one.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new UnitType(name, hitpoints, damage, damageType, size, lineOfSight, resistances, trainingTimeMillis,
                speed, prerequisite, aggressive, type, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts, description);
        }
    }
}
