using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Technologies are things which the player can research, costing resources,
    /// to provide a variety of benefits (and detriments!) to them, such as changing
    /// unit or building type attributes, or unlocking new technologies or buildings.
    /// </summary>
    public class Technology : ICloneable
    {
        public bool researched = false;

        public string name { get; private set; }
        public string description { get; private set; }

        public int[] resourceCosts { get; private set; }

        private string iconName;
        public Image icon { get; private set; }

        // A technology can have a bunch of prerequisite technologies
        // that must be researched before it can be.
        public List<string> prerequisites { get; private set; }
        // It can also have a bunch of technologies which result in
        // it not being able to be researched if any of them are.
        public List<string> clashing { get; private set; }

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
        private List<string> effects;

        /// <summary>
        /// As no technology types exist, all of a technology's attributes must be given
        /// to create one.
        /// </summary>
        public Technology(string name, List<string> prerequisites, List<string> clashing, List<string> effects, int[] resourceCosts, string iconName, string description)
        {
            this.name = name;
            this.description = description;
            this.prerequisites = prerequisites;
            this.clashing = clashing;
            this.effects = effects;
            this.resourceCosts = resourceCosts;
            this.iconName = iconName;
            icon = Image.FromFile(GameInfo.TECHNOLOGY_IMAGE_BASE + iconName);
        }

        /// <summary>
        /// Returns a string representing this technology.
        /// "[name] [prerequisites] ~ [clashing] ~ [effects] ~ [resourceCosts] [iconName] [description]"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            foreach (string p in prerequisites)
                builder.Append(p + " ");
            builder.Append("~ ");
            foreach (string c in clashing)
                builder.Append(c + " ");
            builder.Append("~ ");
            foreach (string e in effects)
                builder.Append(e + " ");
            builder.Append("~ ");
            foreach (int i in resourceCosts)
                builder.Append(i + " ");
            builder.Append(iconName + " ");
            builder.Append(description);
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
            return new Technology(name, prerequisites, clashing, effects, resourceCosts, iconName, description);
        }
    }
}
