using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Devious_Retention.GameBuilder;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A game lobby has a bunch of controls which can be manipulated to create
    /// settings for the game which will be launched from it.
    /// </summary>
    public partial class MultiplayerLobby : Form
    {
        #region Relative Size Constants
        private const double TITLE_HEIGHT = 0.12;
        private const double TITLE_FONT_SIZE = 0.06;
        private const double FONT_SIZE = 0.03;
        private const double MARGIN = 0.02;
        private const double PLAYER_AREA_TOP = 0.2;
        private const double TOTAL_PLAYER_HEIGHT = 0.7;

        // Relative to the width of a player (nearly the width of the window)
        private const double PLAYER_NUMBER_WIDTH = 0.1;
        private const double PLAYER_NAME_WIDTH = 0.4;
        #endregion
        private const string FONT_NAME = "Arial";
        private const int MAX_NAME_CHARS = 20;

        private IPlayerChangeListener listener;

        private ICollection<ClientData> players;
        private int playerID;

        private Font titleFont;
        private Font font;

        private Label title;

        private bool isHosting; // Whether or not this player is the host of the lobby
        private LobbyHost host; // Only used if isHosting

        private int playerHeight; // absolute
        private int numPlayers;
        private int margin;

        /// <summary>
        /// Creates a client lobby
        /// </summary>
        public MultiplayerLobby(IPlayerChangeListener listener)
        {
            this.listener = listener;
            isHosting = false;
            InitAll();
        }

        /// <summary>
        /// Creates a host lobby
        /// </summary>
        public MultiplayerLobby(LobbyHost host, IPlayerChangeListener listener)
        {
            this.listener = listener;
            isHosting = true;
            this.host = host;
            InitAll();
        }

        private void InitAll()
        {
            InitializeComponent();
            ManualInit();
            Paint += Render;
        }

        /// <summary>
        /// Initialises some components of the window that aren't there on the
        /// forms designer.
        /// </summary>
        private void ManualInit()
        {
            // Make title
            title = new Label();
            title.Text = "Multiplayer Lobby" + (isHosting ? " Host" : "");
            title.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(title);
        }

        /// <summary>
        /// Sets this lobby renderer's knowledge of the lobby players
        /// to the given players.
        /// </summary>
        public void SetPlayers(ICollection<ClientData> players, int playerID)
        {
            this.players = players;
            numPlayers = players.Count;
            this.playerID = playerID;
        }

        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (!isHosting) startButton.Visible = false;
            AdaptSizes();
            DrawPlayers(g);
        }

        /// <summary>
        /// Sets the sizes andd positions of all appropriate elements,
        /// as well as font sizes, given the current size of the window
        /// and amount of connected players.
        /// </summary>
        private void AdaptSizes()
        {
            font = new Font(FONT_NAME, (int)(FONT_SIZE * Math.Min(Width, Height)));
            titleFont = new Font(FONT_NAME, (int)(TITLE_FONT_SIZE * Math.Min(Width, Height)));
            margin = (int)(MARGIN * Math.Min(Width, Height));
            
            title.SetBounds(0, margin, Width, (int)(TITLE_HEIGHT * Height));
            title.Font = titleFont;

            double playerHeightWithMargin = Height * (TOTAL_PLAYER_HEIGHT / numPlayers);
            playerHeight = (int)(playerHeightWithMargin - Height*MARGIN);
        }

        /// <summary>
        /// Draws the player information to the best knowledge of
        /// the lobby renderer on the given graphics.
        /// </summary>
        private void DrawPlayers(Graphics g)
        {
            int topY = (int)(PLAYER_AREA_TOP * Height);

            foreach(ClientData player in players)
            {
                int currentY = topY + (playerHeight + margin) * (player.playerNumber-1);
                Point currentPoint = new Point(margin, currentY);

                Color playerColor = player.color;
                Brush playerBrush = new SolidBrush(playerColor);

                g.DrawString(player.playerNumber+"", font, playerBrush, currentPoint);
                currentPoint.X += (int)(Width * PLAYER_NUMBER_WIDTH);
                g.DrawString(player.username, font, playerBrush, currentPoint);
                currentPoint.X += (int)(Width * PLAYER_NAME_WIDTH);
                g.DrawString(player.factionName + "", font, playerBrush, currentPoint);
            }
        }

        /// <summary>
        /// Informs the listener of a change to the client's name.
        /// </summary>
        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            if (nameBox.Text.Length > MAX_NAME_CHARS) listener.UpdateClientUsername(nameBox.Text.Substring(0, MAX_NAME_CHARS));
            else listener.UpdateClientUsername(nameBox.Text);
        }
        /// <summary>
        /// Informs the listener of a change to the client's faction name.
        /// </summary>
        private void factionBox_TextChanged(object sender, EventArgs e)
        {
            if (factionBox.Text.Length > MAX_NAME_CHARS) listener.UpdateClientFactionName(factionBox.Text.Substring(0, MAX_NAME_CHARS));
            else listener.UpdateClientFactionName(factionBox.Text);
        }
        /// <summary>
        /// Informs the listener of a change to the client's player color,
        /// if it's a valid color (otherwise does nothing).
        /// </summary>
        private void colorBox_TextChanged(object sender, EventArgs e)
        {
            string colorText = colorBox.Text;
            if (colorText.Length != 7) return;
            Color color;

            try
            {
                color = ColorTranslator.FromHtml(colorText);
            }
            catch(Exception) // It throws an Exception, not any specific one
            {
                return; // Color can't be found
            }

            listener.UpdateClientColor(colorText);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            listener.StartGame();
        }
    }

    /// <summary>
    /// Defines a class's ability to respond to changes in client data
    /// for a specific client, as well as its ability to respond to
    /// the marking of a specific client as ready or not ready to 
    /// begin the game.
    /// The player change listener must have knowledge of which client
    /// has been specified.
    /// </summary>
    public interface IPlayerChangeListener
    {
        void UpdateClientUsername(string newUsername);
        void UpdateClientFactionName(string newFactionName);
        void UpdateClientColor(string newColorHex);
        void StartGame();
    }
}
