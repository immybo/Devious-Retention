using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Devious_Retention_SP.HumanPlayerView
{
    class ResourceBar : Panel
    {
        private const int MARGIN = 10;

        private int iconSize;
        private Image[] resourceIcons;
        private Font font;
        private Brush fontBrush;

        private Player player;

        public ResourceBar(Player player)
        {
            this.player = player;

            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;

            regularFont = new Font("Arial", 14);
            fontBrush = Brushes.Black;

            resourceIcons = new Image[4];
            resourceIcons[0] = Image.FromFile("../../Images/ResourceIcons/energy.png");
            resourceIcons[1] = Image.FromFile("../../Images/ResourceIcons/metal.png");
            resourceIcons[2] = Image.FromFile("../../Images/ResourceIcons/oil.png");
            resourceIcons[3] = Image.FromFile("../../Images/ResourceIcons/science.png");
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            RectangleF bounds = g.ClipBounds;
            g.FillRectangle(new SolidBrush(Color.Blue), bounds);

            iconSize = bounds.Height - 2 * MARGIN;

            float resourceWidth = bounds.Width / resourceIcons.Count - MARGIN * (1 + 1/resourceIcons.Count);
            for(int i = 0; i < resourceIcons.Count; i++){
                int x = MARGIN + resourceWidth * i;
                RectangleF imgRect = new RectangleF(x, MARGIN, iconSize, iconSize);
                g.drawImage(resourceIcons[i], imgRect);
                x += iconSize + MARGIN;

                PointF strPt = new PointF(x, MARGIN);
                // TODO determine height of string to center in y as icon is
                g.drawString(player.resources[i]+"", font, fontBrush, strPt);
            }
        }
    }
}
