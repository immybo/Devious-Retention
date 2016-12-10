using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Devious_Retention_SP.HumanPlayerView
{
    public class MapDraw
    {
        /// Draws the specified tile over the given rectangle
        /// on the given graphics pane.
        public static void DrawTile(Tile t, Graphics g, Rectangle r){
            // TODO : delegate this functionality into here
            t.DrawAtPosition(g, r);
        }
    }
}