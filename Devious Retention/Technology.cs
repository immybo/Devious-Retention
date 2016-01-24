using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A technology can apply certain effects to a player.
    /// </summary>
    public class Technology : ICloneable
    {
        public bool researched = false;

        public String name { get; private set; }

        public int[] resourceCosts { get; private set; }

        private string iconName;
        public Image icon { get; private set; }

        // A technology can have one or more prerequisite technologies
        // that must be researched before it can be.
        private HashSet<String> prerequisites;

        // Each string has a few components, seperated by spaces:
        // - an identifier for whether it affects a unit, a building or a technology
        // - an identifier for which unit, building or technology it affects
        // - an identifier for the statistic of that unit, building or technology that it affects
        // - a modifier for that statistic 
        // Structure:
        // "[1=unit/2=building/3=technology] [name] [statistic number] [modifier]"
        // All modifiers are FLAT
        // Unit statistics:
        // 0 = max hitpoints, 1 = damage, 2 = training time, 3 = speed, 4 = build speed, 5 = resource costs (separated by spaces), 6 = resistances (separated by spaces)
        // Building statistics:
        // 0 = max hitpoints, 1 = damage, 2 = building time, [note: the next two are SET modifiers, not FLAT modifiers] 3 = provides resource, 4 = resource type, 5 = gather speed, 6 = resource costs, 7 = resistances
        // Technology statistics:
        // 0 = resource costs
        private HashSet<String> effects;

        /// <summary>
        /// As no technology types exist, all of a technology's attributes must be given
        /// to create one.
        /// </summary>
        public Technology(String name, HashSet<String> prerequisites, HashSet<String> effects, int[] resourceCosts, string iconName)
        {
            this.name = name;
            this.prerequisites = prerequisites;
            this.effects = effects;
            this.resourceCosts = resourceCosts;
            this.iconName = iconName;
            icon = Image.FromFile(GameInfo.TECHNOLOGY_IMAGE_BASE + iconName);
        }

        /// <summary>
        /// Returns a string representing this technology.
        /// "[name] [prerequisites] ~ [effects] ~ [resourceCosts] [iconName]"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            foreach (string p in prerequisites)
                builder.Append(p + " ");
            builder.Append("~ ");
            foreach (string e in effects)
                builder.Append(e + " ");
            builder.Append("~ ");
            foreach (int i in resourceCosts)
                builder.Append(i + " ");
            builder.Append(iconName);
            return builder.ToString();
        }

        /// <summary>
        /// Applies this technology's effects to the given types.
        /// </summary>
        public void ApplyEffects(GameInfo types)
        {
            foreach(String s in effects)
            {
                String[] change = s.Split(new char[] { ' ' });
                // change a unit
                if (int.Parse(change[0]) == 1)
                {
                    // identify the type of unit
                    UnitType type = types.unitTypes[change[1]];
                    double modifier = double.Parse(change[3]);
                    // figure out which statistic to change and change it
                    switch (int.Parse(change[2]))
                    {
                        case 0:
                            // scale the hitpoints for all units of that type
                            foreach (Unit unit in type.units)
                                unit.ChangeMaxHP(unit.hitpoints + (int)modifier);
                            type.hitpoints += (int)(modifier);
                            if (type.hitpoints < 1) type.hitpoints = 1;
                            break;
                        case 1:
                            type.damage += (int)(modifier);
                            if (type.damage < 0) type.damage = 0;
                            break;
                        case 2:
                            type.trainingTime += (int)(modifier);
                            if (type.trainingTime < 1) type.trainingTime = 1;
                            break;
                        case 3:
                            type.speed += modifier;
                            if (type.speed < 0) type.speed = 0;
                            break;
                        case 4:
                            try
                            {
                                for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                                    type.resourceCosts[i] += int.Parse(change[3 + i]);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Effect could not be applied from technology. Too few resource cost modifiers were specified. " + e);
                            }
                            break;
                        case 5:
                            try
                            {
                                for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                                    type.resistances[i] += int.Parse(change[3 + i]);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Effect could not be applied from technology. Too few resistance modifiers were specified. " + e);
                            }
                            break;
                    }
                }
                // change a building
                else if (int.Parse(change[0]) == 2)
                {
                    // identify the type of building
                    BuildingType type = types.buildingTypes[change[1]];
                    double modifier = double.Parse(change[3]);
                    // figure out which statistic to change and change it
                    switch (int.Parse(change[2]))
                    {
                        case 0:
                            foreach (Building building in type.buildings)
                                building.ChangeMaxHP(type.hitpoints + (int)modifier);
                            type.hitpoints += (int)modifier;
                            if (type.hitpoints < 1) type.hitpoints = 1;
                            break;
                        case 1:
                            type.damage += (int)modifier;
                            if (type.damage < 0) type.damage = 0;
                            break;
                        case 2:
                            type.buildTime += (int)modifier;
                            if (type.buildTime < 1) type.buildTime = 1;
                            break;
                        case 3:
                            if ((int)modifier == 1)
                                type.providesResource = true;
                            else
                                type.providesResource = false;
                            break;
                        case 4:
                            type.resourceType = (int)modifier;
                            break;
                        case 5:
                            type.gatherSpeed += modifier;
                            break;
                        case 6:
                            try
                            {
                                for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                                    type.resourceCosts[i] += int.Parse(change[3 + i]);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Effect could not be applied from technology. Too few resource cost modifiers were specified. " + e);
                            }
                            break;
                        case 7:
                            try
                            {
                                for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                                    type.resistances[i] += int.Parse(change[3 + i]);
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                Console.WriteLine("Effect could not be applied from technology. Too few resistance modifiers were specified. " + e);
                            }
                            break;

                    }
                }
                // change a technology
                else if (int.Parse(change[0]) == 3)
                {
                    Technology tech = types.technologies[change[1]];
                    try
                    {
                        for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                            tech.resourceCosts[i] += int.Parse(change[3 + i]);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Console.WriteLine("Effect could not be applied from technology. Too few resource cost modifiers were specified. " + e);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new technology that is identical to this one.
        /// </summary>
        public object Clone()
        {
            return new Technology(name, prerequisites, effects, resourceCosts, iconName);
        }
    }
}
