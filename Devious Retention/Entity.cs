using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// An entity is anything that lies on top of the tile map of the game.
    /// This interface is usually used for rendering, as it simplifies things.
    /// Also, entities are treated the same for selection purposes.
    /// </summary>
    public interface Entity
    {
        Image image { get; }
        EntityType type { get; } // unfortunately, this means that we must have two properties for this
        // in each entity, as apparently returning a type that implements EntityType isn't good enough......

        double x { get; set; }
        double y { get; set; }
        
        int playerNumber { get; }
        int id { get; }
    }
}
