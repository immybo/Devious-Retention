using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    public partial class GameWindow : Form
    {
        // 1 = entire width/height, 0 = nothing
        private const double GAME_AREA_WIDTH = 0.75;
        private const double GAME_AREA_HEIGHT = 0.95;
        private const double MINIMAP_WIDTH = 0.15;
        private const double TOP_RIGHT_HEIGHT = 0.5;

        public GameClient client;

        // Where the mouse started dragging, for selection purposes
        private double startX = -1;
        private double startY = -1;

        public GameWindow()
        {
            InitializeComponent();

            Paint += Resize;
            Paint += RenderGamePanel;
            Paint += RenderSelectedEntityPanel;
            Paint += RenderTopRightPanel;
            Paint += RenderResourceDisplayArea;
            Paint += RenderMinimap;
        }

        /// <summary>
        /// Returns the entity, if there is one, which is displayed at (x,y)
        /// on the the screen for the client. This means that, if there are
        /// overlapping entities, the one in front will be returned.
        /// </summary>
        /// <returns>The entity at the position, or void if there was none.</returns>
        public Entity GetEntityAt(double x, double y)
        {
            return null;
        }

        /// <summary>
        /// Returns the entities, if there are any, which are contained within
        /// the rectangle of the display with corners at (x1,y1),(x1,y2),(x2,y1),
        /// (x2,y2), from the client's perspective.
        /// </summary>
        /// <returns>The set of entities that were within the rectangle, or void if there were none.</returns>
        public HashSet<Entity> GetEntitiesIn(double x1, double y1, double x2, double y2)
        {
            return null;
        }
    
        /// <summary>
        /// Attempts to resize the window to the dimensions of the screen.
        /// </summary>
        private void Resize(object sender, PaintEventArgs e)
        {
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
        }

        /// <summary>
        /// Renders the game panel; i.e. the part of the window which contains
        /// the entities, tiles, etc. Also renders the resource counts and minimap.
        /// Uses the client's perspective to do so.
        /// </summary>
        private void RenderGamePanel(object sender, PaintEventArgs e)
        {
            int panelWidth = (int)(GAME_AREA_WIDTH * Width);
            int panelHeight = (int)(GAME_AREA_HEIGHT * Height);
            int panelX = 0;
            int panelY = 0;

            Graphics g = e.Graphics;
            
            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 50, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Game area center", font, Brushes.Black, new PointF(panelWidth / 2, panelHeight / 2), format);
        }

        /// <summary>
        /// Renders the resource display area at the bottom of the screen.
        /// </summary>
        private void RenderResourceDisplayArea(object sender, PaintEventArgs e)
        {
            int panelWidth = (int)(GAME_AREA_WIDTH * Width);
            int panelHeight = (int)((1 - GAME_AREA_HEIGHT) * Height);
            int panelX = (int)(0);
            int panelY = (int)(GAME_AREA_HEIGHT * Height);

            Graphics g = e.Graphics;

            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 30, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Resource display area", font, Brushes.Black, new PointF(panelX + panelWidth / 2, panelY + panelHeight / 2), format);
        }

        /// <summary>
        /// Renders the minimap.
        /// </summary>
        private void RenderMinimap(object sender, PaintEventArgs e)
        {
            int panelWidth = (int)(MINIMAP_WIDTH * Width);
            int panelHeight = panelWidth;
            int panelX = (int)((GAME_AREA_WIDTH - MINIMAP_WIDTH) * Width);
            int panelY = (int)(GAME_AREA_HEIGHT * Height) - panelHeight;

            Graphics g = e.Graphics;

            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 30, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Minimap", font, Brushes.Black, new PointF(panelX + panelWidth / 2, panelY + panelHeight / 2), format);
        }

        /// <summary>
        /// Renders the selected entity panel; i.e. the part of the window which
        /// allows the player to perform actions from the selected entity.
        /// </summary>
        private void RenderSelectedEntityPanel(object sender, PaintEventArgs e)
        {
            int panelWidth = (int)((1- GAME_AREA_WIDTH) * Width);
            int panelHeight = (int)((1 - TOP_RIGHT_HEIGHT) * Height);
            int panelX = (int)(GAME_AREA_WIDTH * Width);
            int panelY = (int)(TOP_RIGHT_HEIGHT * Height);

            Graphics g = e.Graphics;

            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 30, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Selected entity panel", font, Brushes.Black, new PointF(panelX + panelWidth / 2, panelY + panelHeight / 2), format);
        }

        /// <summary>
        /// Renders the top right panel; i.e. the part of the window which allows
        /// the user to either select a building to create or a technology to 
        /// research.
        /// </summary>
        private void RenderTopRightPanel(object sender, PaintEventArgs e)
        {
            int panelWidth = (int)((1 - GAME_AREA_WIDTH) * Width);
            int panelHeight = (int)(TOP_RIGHT_HEIGHT * Height);
            int panelX = (int)(GAME_AREA_WIDTH * Width);
            int panelY = 0;

            Graphics g = e.Graphics;

            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 50, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Top right panel", font, Brushes.Black, new PointF(panelX + panelWidth / 2, panelHeight / 2), format);
        }

        /// <summary>
        /// Processes any key events on the game window. If they are recognised as
        /// utilised keys, performs the appropriate action on the client.
        /// </summary>
        public void KeyEvent(KeyEventArgs e)
        {

        }

        /// <summary>
        /// Processes any mouse events on the game window. Usually performs the
        /// appropriate action on the client.
        /// </summary>
        public void MouseEvent(MouseEventArgs e)
        {

        }
    }
}
