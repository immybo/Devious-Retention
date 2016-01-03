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
        private List<String> effects;

        /// <summary>
        /// A faction must have its name and all its effects given to be created.
        /// </summary>
        public Faction(String name, List<String> effects)
        {

        }

        /// <summary>
        /// Applies all of this faction's effects to the given sets of units, buildings and technologies.
        /// </summary>
        public void ApplyEffects(GameInfo types)
        {

        }
    }
}
