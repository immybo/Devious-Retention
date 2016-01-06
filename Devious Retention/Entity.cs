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
        /// <summary>
        /// GetImage should always return the image for the entity's current frame of animation,
        /// direction, etc.
        /// </summary>
        Image GetImage();
        /// <summary>
        /// Every entity has a size, and for the purposes of rendering it may as well be a double.
        /// </summary>
        Double GetSize();

        Double GetX();
        Double GetY();
    }
}
