using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A UnitType, BuildingType or ResourceType
    /// </summary>
    public interface EntityType
    {
        // Of course, not all of these are supported for all 3 classes.
        string name { get; }
        string description { get; }
        int hitpoints { get; set; }
        int damage { get; set;  }
        int damageType { get; }
        int[] resistances { get; set; }
        double speed { get; set; }
        double size { get; }
        string prerequisite { get; }
        bool aggressive { get; }
        int range { get; set; }
        int lineOfSight { get; set; }
        int[] resourceCosts { get; }
    }
}
