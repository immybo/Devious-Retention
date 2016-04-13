using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention
{
    /// <summary>
    /// The initial menu that the user sees when they run the game.
    /// Displays both buttons to host or join a game lobby, and the game lobby itself.
    /// </summary>
    public partial class GameMenu : Form
    {
        // How far the various elements are along the x axis (percent)
        private const float PLAYER_NUMBER_PERCENT = 5;
        private const float PLAYER_NAME_PERCENT = 10;

        public static Brush PLAYER_BRUSH { get; private set; } = Brushes.Black;
        public static Font MENU_FONT { get; private set; } = new Font(GameInfo.FONT_NAME, 20);

        private const int PLAYER_Y_OFFSET = 300; // top of the lobby offset
        private const int PLAYER_X_OFFSET = 50; // left/right of the lobby offset
        private const int PLAYER_Y_BOTTOM_BOUNDARY = 200; // how many pixels the bottom of the lobby is from the bottom of the window

        private Font font;
        
        private GameLobby lobby;

        private IPAddress localIP;

        // The name belonging to this player; will appear in the lobby
        private string name = "Bob";

        public GameMenu()
        {
            InitializeComponent();
            SetLocalIP();
            
            font = new Font(GameInfo.FONT_NAME, 20);
            hostGameButton.Font = font;
            joinGameButton.Font = font;
            ipBox.Font = new Font(GameInfo.FONT_NAME, 10);

            Paint += Render;

            // Start a rendering loop
            Timer timer = new Timer();
            timer.Interval = 200; // 5FPS is good enough for the menu
            timer.Tick += RedrawEvent;
            timer.Start();
        }

        public void RedrawEvent(object sender, EventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// Figures out what the local machine's IP is and sets localIP to it.
        /// </summary>
        private void SetLocalIP()
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in entry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip;
                }
            }
        }

        /// <summary>
        /// Upon clicking the host game button, display the game lobby.
        /// Terminate the current one and display the new one if there already was one.
        /// </summary>
        private void hostGameButton_Click(object sender, EventArgs e)
        {
            if (lobby != null) { lobby.Terminate(); lobby = null; }
            lobby = new HostGameLobby(localIP, localIP);
            lobby.Connect();
            lobby.ChangeName(name);
        }

        /// <summary>
        /// Upon clicking the join game button, attempt to parse the given IP
        /// address. If parsed correctly, attempt to join that IP.
        /// </summary>
        private void joinGameButton_Click(object sender, EventArgs e)
        {
            IPAddress ip;
            IPAddress.TryParse(ipBox.Text, out ip);
            if(ip == null) // If it couldn't be parsed as an IP address, prompt the user to enter another one
            {
                ipBox.Text = "Please enter a valid IP address.";
            }
            else // Otherwise, attempt to join that IP
            {
                if (lobby != null) { lobby.Terminate(); lobby = null; }
                lobby = new GameLobby(localIP, ip);
                lobby.ChangeName(name);
                // If we didn't manage to connect to that IP, make sure to reset the lobby
                bool connected = lobby.Connect();
                if (!connected)
                {
                    lobby.Terminate();
                    lobby = null;
                }
            }
        }

        /// <summary>
        /// Renders the window; specifically, the lobby window if it's active.
        /// </summary>
        public void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (lobby == null) return;
            if (lobby.terminated) { lobby = null; return; }

            // Render the player information up the top
            int lobbyWidth = this.Width - PLAYER_X_OFFSET * 2;
            int lobbyHeight = this.Height - PLAYER_Y_OFFSET - PLAYER_Y_BOTTOM_BOUNDARY;
            Rectangle lobbyBounds = new Rectangle(PLAYER_X_OFFSET, PLAYER_Y_OFFSET, 500, 300);
            lobby.Render(g, lobbyBounds);

            // Then, render player choices at the bottom

            // And finally the start game button if we're the host
        }

        /// <summary>
        /// When the name text box is changed, change the player name in the lobby
        /// </summary>
        private void playerNameBox_TextChanged(object sender, EventArgs e)
        {
            name = playerNameBox.Text;
            if(lobby != null)
            {
                lobby.ChangeName(name);
            }
        }
    }
}
