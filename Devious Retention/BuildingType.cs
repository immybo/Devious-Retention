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
    public class BuildingType
    {
        public String name { get; private set; }
        // The initial amount of hitpoints for buildings of this type.
        public int hitpoints;
        // The base amount of damage that this building will do to other units per attack.
        // Note that some buildings don't attack; this will be 0 in that case.
        public int damage;
        // So that unit counters can effectively be made, there are multiple different types of damage.
        // Although every attack must do at least one damage, a resistance against a type of damage reduces it by a percentage.
        public int damageType { get; private set; }
        // How many tiles this building can "see"
        public int lineOfSight;
        // How many tiles this building takes up along each axis
        public int size { get; private set; }
        public int[] resistances;
        // Ticks
        public int buildTime;
        // Every BuildingType can only have up to one prerequisite technology
        // Before this technology is researched, no buildings of this type can be created
        // Not every BuildingType has to have a prerequisite, however
        public String prerequisite { get; private set; }

        // Most buildings don't provide any resources.
        // Those that do, however, passively provide an infinite amount of that resource, usually at a slow rate.
        public bool providesResource;
        public int resourceType;
        public double gatherSpeed;

        // Some buildings can be built on top of resource sites, and they will extract that resource.
        // If they do, the gather rate is set by that specific resource type.
        public bool canBeBuiltOnResource { get; private set; }
        public int builtOnResourceType { get; private set; }

        // This is different from units:
        // Buildings that are not aggressive will never attack and can't be commanded to.
        // Buildings that are act the same as aggressive units (except can't move towards the enemy).
        public bool aggressive { get; private set; }

        public int[] resourceCosts;

        // What types of unit can be trained from this type of building
        public string[] trainableUnits { get; private set; }

        private string imageName;
        public Image image { get; private set; }
        private string iconName;
        public Image icon { get; private set; }

        // How many tiles away this building can attack from (only relevant if aggressive)
        public int range { get; private set; }
        // How many ticks it takes this building to attack (only relevant if aggressive)
        public int attackTicks { get; private set; }
        private int attackSpeedMilliseconds;

        public List<Building> buildings;

        /// <summary>
        /// Anything attempting to create a BuildingType from a file must first
        /// parse the string into these attributes.
        /// </summary>
        public BuildingType(string name, int hitpoints, int damage, int damageType, int lineOfSight, int size, int[] resistances, int buildTimeMillis, string prerequisite, bool providesResource, int resourceType,
            double gatherSpeed, bool builtOnResource, int builtOnResourceType, bool aggressive, string imageName, string iconName, int range, int attackSpeedMilliseconds, int[] resourceCosts, string[] trainableUnits)
        {
            this.name = name;
            this.hitpoints = hitpoints;
            this.damageType = damageType;
            this.lineOfSight = lineOfSight;
            this.size = size;
            this.resistances = resistances;
            this.buildTime = (int)(buildTimeMillis / GameInfo.TICK_TIME);
            this.prerequisite = prerequisite;
            this.providesResource = providesResource;
            this.resourceType = resourceType;
            this.gatherSpeed = gatherSpeed;
            this.canBeBuiltOnResource = builtOnResource;
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
            catch(IOException e)
            {
                image = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
                icon = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
            }
        }

        /// <summary>
        /// Returns:
        /// "name hitpoints damage damageType lineOfSight size resistance1 resistance2 .. resistanceX buildTime
        ///     prerequisiteName providesResource resourceType gatherSpeed builtOnResource builtOnResourceType aggressive imageName iconName range attackSpeedMilliseconds resourcecost1 resourcecost2 .. resourcecostx trainableUnits"
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
            foreach(string s in trainableUnits)
                builder.Append(s + " ");

            return builder.ToString();
        }
    }
}
