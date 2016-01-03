using System;
using System.Text;

namespace Devious_Retention
{
    /// <summary>
    /// A UnitType defines attributes for a specific type of unit.
    /// These attributes can be changed through technologies, and so
    /// each client has a set of UnitTypes specific to it.
    /// </summary>
    public class UnitType
    {
        // The initial amount of hitpoints for units of this type.
        public int hitpoints { get; private set; }
        // The base amount of damage that this unit will do to other units per attack.
        public int damage { get; private set; }
        // So that unit counters can effectively be made, there are multiple different types of damage.
        // Although every attack must do at least one damage, a resistance against a type of damage reduces it by a percentage.
        public int damageType { get; private set; }
        public int[] resistances { get; private set; }
        // Milliseconds (rounds to the nearest tick)
        public int trainingTime { get; private set; }
        // How many tiles units of this type can move per second
        public double speed { get; private set; }
        // Every UnitType can only have up to one prerequisite technology
        // Before this technology is researched, no units of this type can be created
        // Not every UnitType has to have a prerequisite, however
        public String prerequisite { get; private set; }

        // Most units are unable to build.
        // When tasked on a building, these units will simply
        // walk to the click point. However, if a unit can, it will start
        // working on the building.
        public bool canBuild { get; private set; }
        // 1 is the baseline; a unit with 1 build speed will provide 1 "work second" per second
        public double buildSpeed{ get; private set; } // Only applicable if canBuild

        // An aggressive unit will attempt to attack nearby enemy units
        public bool aggressive { get; private set; }

        public int[] resourceCosts { get; private set; }

        /// <summary>
        /// Creating a UnitType requires providing a string with all of the attributes formatted like:
        /// "hitpoints damage damageType resistance1 resistance2 .. resistanceX trainingTime speed
        ///     prerequisiteName canBuild buildSpeed aggressive resourcecost1 resourcecost2 ... resourcecostx"
        /// This format will also be outputted by that UnitType's toString.
        /// </summary>
        public UnitType(String s)
        {
            String[] attributes = s.Split(new char[] { ' ' });

            hitpoints = int.Parse(attributes[0]);
            damage = int.Parse(attributes[1]);
            damageType = int.Parse(attributes[2]);
            resistances = new int[GameInfo.DAMAGE_TYPES];
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                resistances[i] = int.Parse(attributes[3 + i]);
            trainingTime = int.Parse(attributes[3 + GameInfo.DAMAGE_TYPES]);
            speed = double.Parse(attributes[4 + GameInfo.DAMAGE_TYPES]);
            prerequisite = attributes[5 + GameInfo.DAMAGE_TYPES];
            canBuild = bool.Parse(attributes[6 + GameInfo.DAMAGE_TYPES]);
            buildSpeed = double.Parse(attributes[7 + GameInfo.DAMAGE_TYPES]);
            aggressive = bool.Parse(attributes[8 + GameInfo.DAMAGE_TYPES]);
            for(int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                resourceCosts[i] = int.Parse(attributes[9 + GameInfo.DAMAGE_TYPES + i]);
        }

        /// <summary>
        /// See the constructor for information on the string returned.
        /// </summary>
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(hitpoints + " ");
            builder.Append(damage + " ");
            builder.Append(damageType + " ");
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                builder.Append(resistances[i] + " ");
            builder.Append(trainingTime + " ");
            builder.Append(speed + " ");
            builder.Append(prerequisite + " ");
            builder.Append(canBuild + " ");
            builder.Append(buildSpeed + " ");
            builder.Append(aggressive + " ");
            for(int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                builder.Append(resourceCosts[i] + " ");

            return builder.ToString();
        }
    }
}
