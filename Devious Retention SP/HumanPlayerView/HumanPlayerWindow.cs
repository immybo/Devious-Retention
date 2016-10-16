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
        public const bool DEBUG_INPUT = true;

        private World world;
        private HumanPlayerListener listener;
        private HumanPlayer player;
        private Timer windowRefreshTimer;

        public HumanPlayerWindow(HumanPlayerListener listener, HumanPlayer player, World world)
        {
            this.listener = listener;
            this.world = world;
            this.player = player;

            InitializeComponent();

            windowRefreshTimer = new Timer();
            windowRefreshTimer.Interval = 16;
            windowRefreshTimer.Tick += new EventHandler(RefreshWindow);
            windowRefreshTimer.Start();

            this.KeyDown += new KeyEventHandler(DoKeyPress);
        }

        public void RefreshWindow(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void DoKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                gameArea.PlayerView.X += HumanPlayerView.GameArea.SCROLL_SPEED;
            else if (e.KeyCode == Keys.Right)
                gameArea.PlayerView.X -= HumanPlayerView.GameArea.SCROLL_SPEED;
            else if (e.KeyCode == Keys.Up)
                gameArea.PlayerView.Y += HumanPlayerView.GameArea.SCROLL_SPEED;
            else if (e.KeyCode == Keys.Down)
                gameArea.PlayerView.Y -= HumanPlayerView.GameArea.SCROLL_SPEED;

            listener.DoKeyPress(e.KeyCode);
        }

        /// <summary>
        /// Returns the panel which the given position lies on.
        /// Assumes that the given point is within the bounds of the window.
        /// </summary>
        private Panel GetArea(PointF graphicsPosition)
        {
            if (graphicsPosition.X < gameArea.Width)
            {
                if (graphicsPosition.Y < gameArea.Height)
                    return gameArea;
                else
                    return resourceBar;
            }
            else
            {
                if (graphicsPosition.Y < topRightPanel.Height)
                    return topRightPanel;
                else
                    return bottomRightPanel;
            }
        }
    }
}
