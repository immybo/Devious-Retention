using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Presents the current view of the game to a human player through a form,
    /// and pushes information about the human player's actions back to the attached
    /// HumanPlayerListener.
    /// </summary>
    public partial class HumanPlayerWindow : Form
    {
        private HumanPlayerListener listener;

        public HumanPlayerWindow(HumanPlayerListener listener)
        {
            this.listener = listener;

            resourceBar.Paint += DrawResourceBar;
            topRightPanel.Paint += DrawTopRightPanel;
            bottomRightPanel.Paint += DrawBottomRightPanel;
            gameArea.Paint += DrawGameArea;
        }

        public void DrawResourceBar(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = resourceBar.Bounds;
        }

        public void DrawTopRightPanel(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = topRightPanel.Bounds;
        }

        public void DrawBottomRightPanel(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = bottomRightPanel.Bounds;
        }

        public void DrawGameArea(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = gameArea.Bounds;
        }
    }
}
