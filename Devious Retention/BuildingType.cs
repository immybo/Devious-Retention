using System;
using System.Collections.Generic;
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
        public int[] resistances;
        // Milliseconds (rounds to the nearest tick)
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
        public bool builtOnResource { get; private set; }
        public int builtOnResourceType { get; private set; }

        // This is different from units:
        // Buildings that are not aggressive will never attack and can't be commanded to.
        // Buildings that are act the same as aggressive units (except can't move towards the enemy).
        public bool aggressive { get; private set; }

        public int[] resourceCosts;

        public List<Building> buildings;

        /// <summary>
        /// Creating a BuildingType requires providing a string with all of the attributes formatted like:
        /// "name hitpoints damage damageType resistance1 resistance2 .. resistanceX buildTime
        ///     prerequisiteName providesResource resourceType gatherSpeed builtOnResource builtOnResourceType aggressive resourcecost1 resourcecost2 .. resourcecostx"
        /// This format will also be outputted by that BuildingType's toString.
        /// </summary>
        public BuildingType(String s)
        {
            String[] attributes = s.Split(new char[] { ' ' });

            name = attributes[0];
            hitpoints = int.Parse(attributes[1]);
            damage = int.Parse(attributes[2]);
            damageType = int.Parse(attributes[3]);
            resistances = new int[GameInfo.DAMAGE_TYPES];
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                resistances[i] = int.Parse(attributes[4 + i]);
            buildTime = int.Parse(attributes[4 + GameInfo.DAMAGE_TYPES]);
            prerequisite = attributes[5 + GameInfo.DAMAGE_TYPES];
            providesResource = bool.Parse(attributes[6 + GameInfo.DAMAGE_TYPES]);
            resourceType = int.Parse(attributes[7 + GameInfo.DAMAGE_TYPES]);
            gatherSpeed = double.Parse(attributes[8 + GameInfo.DAMAGE_TYPES]);
            builtOnResource = bool.Parse(attributes[9 + GameInfo.DAMAGE_TYPES]);
            builtOnResourceType = int.Parse(attributes[10 + GameInfo.DAMAGE_TYPES]);
            aggressive = bool.Parse(attributes[11 + GameInfo.DAMAGE_TYPES]);
            resourceCosts = new int[GameInfo.RESOURCE_TYPES];
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                resourceCosts[i] = int.Parse(attributes[12 + GameInfo.DAMAGE_TYPES + i]);
        }

        /// <summary>
        /// See the constructor for information on the string returned.
        /// </summary>
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            builder.Append(hitpoints + " ");
            builder.Append(damage + " ");
            builder.Append(damageType + " ");
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                builder.Append(resistances[i] + " ");
            builder.Append(buildTime + " ");
            builder.Append(prerequisite + " ");
            builder.Append(providesResource + " ");
            builder.Append(resourceType + " ");
            builder.Append(gatherSpeed + " ");
            builder.Append(builtOnResource + " ");
            builder.Append(builtOnResourceType + " ");
            builder.Append(aggressive + " ");
            for(int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                builder.Append(resourceCosts[i] + " ");

            return builder.ToString();
        }
    }
}
