using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Devious_Retention
{
    /// <summary>
    /// A BuildingType defines attributes for a specific type of building.
    /// These attributes can be changed through technologies, and so
    /// each client has a set of BuildingTypes specific to it.
    /// </summary>
    public class BuildingType : ICloneable, EntityType
    {
        public string name { get; private set; }
        public string description { get; private set; }
        // The initial amount of hitpoints for buildings of this type.
        public int hitpoints { get; set; }
        // The base amount of damage that this building will do to other units per attack.
        // Note that some buildings don't attack; this will be 0 in that case.
        public int damage { get; set; }
        // So that unit counters can effectively be made, there are multiple different types of damage.
        // Although every attack must do at least one damage, a resistance against a type of damage reduces it by a percentage.
        public int damageType { get; private set; }
        // How many tiles this building can "see"
        public int lineOfSight { get; set; }
        // How many tiles this building takes up along each axis
        public double size { get; private set; }
        public int[] resistances { get; set; }
        private int buildTimeMillis;
        // Ticks
        public int buildTime { get; set; }
        // Every BuildingType can only have up to one prerequisite technology
        // Before this technology is researched, no buildings of this type can be created
        // Not every BuildingType has to have a prerequisite, however
        public String prerequisite { get; private set; }

        // Most buildings don't provide any resources.
        // Those that do, however, passively provide an infinite amount of that resource, usually at a slow rate.
        public bool providesResource { get; set; }
        public int resourceType { get; set; }
        public double gatherSpeed { get; set; }

        // Some buildings can be built on top of resource sites, and they will extract that resource.
        // If they do, the gather rate is set by that specific resource type.
        public bool canBeBuiltOnResource { get; private set; }
        public int builtOnResourceType { get; private set; }

        // This is different from units:
        // Buildings that are not aggressive will never attack and can't be commanded to.
        // Buildings that are act the same as aggressive units (except can't move towards the enemy).
        public bool aggressive { get; private set; }

        public int[] resourceCosts { get; set; }

        // What types of unit can be trained from this type of building
        public string[] trainableUnits { get; private set; }

        private string imageName;
        public Image image { get; private set; }
        private string iconName;
        public Image icon { get; private set; }

        // How many tiles away this building can attack from (only relevant if aggressive)
        public int range { get; set; }
        // How many ticks it takes this building to attack (only relevant if aggressive)
        public int attackTicks { get; private set; }
        private int attackSpeedMilliseconds;

        public List<Building> buildings { get; set; }

        // Unused
        public double speed { get; set; } = -1;

        /// <summary>
        /// Anything attempting to create a BuildingType from a file must first
        /// parse the string into these attributes.
        /// </summary>
        public BuildingType(string name, int hitpoints, int damage, int damageType, int lineOfSight, double size, int[] resistances, int buildTimeMillis, string prerequisite, bool providesResource, int resourceType,
            double gatherSpeed, bool canBeBuiltOnResource, int builtOnResourceType, bool aggressive, string imageName, string iconName, int range, int attackSpeedMilliseconds, int[] resourceCosts, string[] trainableUnits, string description)
        {
            this.name = name;
            this.description = description;
            this.hitpoints = hitpoints;
            this.damageType = damageType;
            this.lineOfSight = lineOfSight;
            this.size = size;
            this.resistances = resistances;
            this.buildTimeMillis = buildTimeMillis;
            this.buildTime = (int)(buildTimeMillis / GameInfo.TICK_TIME);
            this.prerequisite = prerequisite;
            this.providesResource = providesResource;
            this.resourceType = resourceType;
            this.gatherSpeed = gatherSpeed;
            this.canBeBuiltOnResource = canBeBuiltOnResource;
            this.builtOnResourceType = builtOnResourceType;
            this.aggressive = aggressive;
            this.imageName = imageName;
            this.range = range;
            this.attackSpeedMilliseconds = attackSpeedMilliseconds;
            this.attackTicks = (int)(attackSpeedMilliseconds / GameInfo.TICK_TIME);
            if (attackTicks <= 0) attackTicks = 1;
            this.resourceCosts = resourceCosts;
            this.trainableUnits = trainableUnits;

            try
            { 
                image = Image.FromFile(GameInfo.BUILDING_IMAGE_BASE + imageName);
                icon = Image.FromFile(GameInfo.BUILDING_ICON_BASE + iconName);
            }
            // If the image can't be loaded, load a default one instead (which hopefully can!)
            catch(IOException)
            {
                image = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
                icon = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
            }

            buildings = new List<Building>();
        }

        /// <summary>
        /// Returns:
        /// "name hitpoints damage damageType lineOfSight size resistance1 resistance2 .. resistanceX buildTime
        ///     prerequisiteName providesResource resourceType gatherSpeed canBeBuiltOnResource builtOnResourceType aggressive imageName
        ///  iconName range attackSpeedMilliseconds resourcecost1 resourcecost2 .. resourcecostx trainableUnits.count trainableUnits description"
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
            builder.Append(buildTime + " ");
            builder.Append(prerequisite + " ");
            builder.Append(providesResource + " ");
            builder.Append(resourceType + " ");
            builder.Append(gatherSpeed + " ");
            builder.Append(canBeBuiltOnResource + " ");
            builder.Append(builtOnResourceType + " ");
            builder.Append(aggressive + " ");
            builder.Append(imageName + " ");
            builder.Append(iconName + " ");
            builder.Append(range + " ");
            builder.Append(attackSpeedMilliseconds + " ");
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                builder.Append(resourceCosts[i] + " ");
            builder.Append(trainableUnits.Length + " ");
            foreach(string s in trainableUnits)
                builder.Append(s + " ");
            builder.Append(description);

            return builder.ToString();
        }

        /// <summary>
        /// Returns a new BuildingType that is completely identical to this one.
        /// </summary>
        public object Clone()
        {
            return new BuildingType(name, hitpoints, damage, damageType, lineOfSight, size, resistances, buildTimeMillis, prerequisite, providesResource, resourceType,
                gatherSpeed, canBeBuiltOnResource, builtOnResourceType, aggressive, imageName, iconName, range, attackSpeedMilliseconds, resourceCosts, trainableUnits, description);
        }
    }
}
