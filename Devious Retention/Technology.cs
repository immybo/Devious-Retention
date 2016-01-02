using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A technology can apply certain effects to a player.
    /// </summary>
    public class Technology
    {
        // The unique ID of this technology
        private int id { get; }

        private String name { get; }

        // A technology can have one or more prerequisite technologies
        // that must be researched before it can be.
        private HashSet<Technology> prerequisites;

        // Each string has a few components, seperated by spaces:
        // - an identifier for whether it affects a unit, a building or a technology
        // - an identifier for which unit, building or technology it affects
        // - an identifier for the statistic of that unit, building or technology that it affects
        // - a modifier for that statistic 
        private List<String> effects;

        /// <summary>
        /// As no technology types exist, all of a technology's attributes must be given
        /// to create one.
        /// </summary>
        public Technology(int id, String name, HashSet<Technology> prerequisites, List<String> effects)
        {

        }

        /// <summary>
        /// Applies this technology's effects to the given types.
        /// </summary>
        public void ApplyEffects(GameInfo types)
        {

        }
    }
}
