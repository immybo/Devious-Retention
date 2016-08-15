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
        private Timer windowRefreshTimer;

        public HumanPlayerWindow(HumanPlayerListener listener)
        {
            InitializeComponent();
            this.listener = listener;

            windowRefreshTimer = new Timer();
            windowRefreshTimer.Interval = 16;
            windowRefreshTimer.Tick += new EventHandler(RefreshWindow);
            windowRefreshTimer.Start();
        }

        public void RefreshWindow(object sender, EventArgs e)
        {
            this.Refresh();
        }
    }
}
