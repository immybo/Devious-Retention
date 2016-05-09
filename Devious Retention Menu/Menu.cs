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

namespace Devious_Retention_Menu
{
    /// <summary>
    /// The menu is the main starting window for the game,
    /// having buttons to launch into the various submenus
    /// which allow launching into the game proper or changing
    /// options, etc.
    /// </summary>
    public partial class Menu : Form
    {
        private MenuItemHandler openMenu = null;

        public Menu()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Launches into a new single player lobby; i.e. one into which
        /// no other human plays can join.
        /// </summary>
        private void singleplayerButton_Click(object sender, EventArgs e)
        {
            //openMenu = new SingleplayerLobbyHandler();
        }

        /// <summary>
        /// Launches into a new multiplayer host lobby.
        /// This allows other human players to connect to this computer.
        /// </summary>
        private void multiplayerHostButton_Click(object sender, EventArgs e)
        {
            // Create a lobby and then attempt to join it at this IP
            LobbyHost lobbyHost = new LobbyHost();
            if (!JoinLobby(IPAddress.Parse("127.0.0.1")))
            {
                lobbyHost.Close();
            }
        }

        /// <summary>
        /// Attempts to use the previously entered IP address to join
        /// an existing multiplayer game lobby. If the text is not a valid
        /// IP address or if a game can't be joined at that address, displays
        /// an error.
        /// </summary>
        private void multiplayerJoinButton_Click(object sender, EventArgs e)
        {
            // Try to grab the IP
            IPAddress ip;

            // If the IP is invalid, we display an error message
            if (!IPAddress.TryParse(ipTextbox.Text, out ip)){
                DisplayError("Invalid IP address.");
                return;
            }

            // Otherwise, we attempt to join a lobby at that address
            JoinLobby(ip);
        }

        /// <summary>
        /// Attempts to launch into a game lobby at the given IP address.
        /// If there is no lobby at that IP, displays an error.
        /// Returns whether or not joining the lobby succeeded.
        /// </summary>
        private bool JoinLobby(IPAddress ip)
        {
            try {
                MultiplayerLobbyHandler lobby = new MultiplayerLobbyHandler(ip);
                openMenu = lobby;
                return true;
            }
            catch(ApplicationException)
            {
                DisplayError("Could not connect to lobby at " + ip.ToString() + ".");
                return false;
            }
        }

        /// <summary>
        /// Pops up in front of the menu with an error.
        /// </summary>
        /// <param name="errorText">The text of the error to display.</param>
        private void DisplayError(string errorText)
        {
            MessageBox.Show(errorText);
        }
    }
}
