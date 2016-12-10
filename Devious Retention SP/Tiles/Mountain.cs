using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP.Tiles
{
    class Mountain : Tile
    {
        public override void DrawAtPosition(Graphics g, RectangleF graphicsRect)
        {
            g.FillRectangle(new SolidBrush(Color.Gray), graphicsRect);
        }
    }
}
