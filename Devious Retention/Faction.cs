using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Every player belongs to a given Faction.
    /// Each Faction applies a variety of benefits to the player.
    /// </summary>
    public class Faction
    {
        public String name { get; private set; }

        // Each string has a few components, seperated by spaces:
        // - an identifier for whether it affects a unit, a building or a technology
        // - an identifier for which unit, building or technology it affects
        // - an identifier for the statistic of that unit, building or technology that it affects
        // - a modifier for that statistic 
        // structure same as for technology
        private HashSet<String> effects;

        /// <summary>
        /// A faction must have its name and all its effects given to be created.
        /// </summary>
        public Faction(String name, HashSet<String> effects)
        {

        }

        /// <summary>
        /// Applies all of this faction's effects to the given sets of units, buildings and technologies.
        /// </summary>
        public void ApplyEffects(GameInfo types)
        {
            foreach (String s in effects)
            {
                String[] change = s.Split(new char[] { ' ' });
                // change a unit
                if (int.Parse(change[0]) == 1)
                {
                    // identify the type of unit
                    UnitType type = types.unitTypes[int.Parse(change[1])];
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
                            type.buildSpeed += modifier;
                            if (type.buildSpeed < 0) type.buildSpeed = 0;
                            break;
                        case 5:
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
                        case 6:
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
                    BuildingType type = types.buildingTypes[int.Parse(change[1])];
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
                    Technology tech = types.technologies[int.Parse(change[1])];
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
    }
}
