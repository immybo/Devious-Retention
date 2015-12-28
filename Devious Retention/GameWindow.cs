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
        GameClient client;

        public GameWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Returns the entity, if there is one, which is displayed at (x,y)
        /// on the the screen for the client. This means that, if there are
        /// overlapping entities, the one in front will be returned.
        /// </summary>
        /// <returns>The entity at the position, or void if there was none.</returns>
        public Entity GetEntityAt(double x, double y)
        {

        }

        /// <summary>
        /// Returns the entities, if there are any, which are contained within
        /// the rectangle of the display with corners at (x1,y1),(x1,y2),(x2,y1),
        /// (x2,y2), from the client's perspective.
        /// </summary>
        /// <returns>The set of entities that were within the rectangle, or void if there were none.</returns>
        public Set<Entity> GetEntitiesIn(double x1, double y1, double x2, double y2)
        {

        }

        /// <summary>
        /// Renders the game panel; i.e. the part of the window which contains
        /// the entities, tiles, etc. Also renders the resource counts.
        /// Uses the client's perspective to do so.
        /// </summary>
        public void RenderGamePanel()
        {

        }

        /// <summary>
        /// Renders the selected entity panel; i.e. the part of the window which
        /// displays information about the currently selected entities.
        /// </summary>
        public void RenderSelectedEntityPanel()
        {

        }

        /// <summary>
        /// Renders the top right panel; i.e. the part of the window which allows
        /// the user to either select a building to create or a technology to 
        /// research.
        /// </summary>
        public void RenderTopRightPanel()
        {

        }

        /// <summary>
        /// Processes any key events on the game window. If they are recognised as
        /// utilised keys, passes the event on to the client.
        /// </summary>
        public void KeyEvent(KeyEventArgs e)
        {

        }

        /// <summary>
        /// Processes any mouse events on the game window. Usually passes on the
        /// event to the client.
        /// </summary>
        public void MouseEvent(MouseEventArgs e)
        {

        }
    }
}
