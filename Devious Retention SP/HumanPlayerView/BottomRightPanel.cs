using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Devious_Retention_SP.HumanPlayerView
{
    class BottomRightPanel : Panel
    {
        public BottomRightPanel()
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = g.ClipBounds;
            g.FillRectangle(new SolidBrush(Color.Red), bounds);
        }
    }
}
