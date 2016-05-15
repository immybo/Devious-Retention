using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private LobbyHost openLobbyHost = null;
        private bool tryingToConnect = false;

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
            if (tryingToConnect) return;

            tryingToConnect = true;
            CloseAllWindows();
            // Create a lobby and then attempt to join it at this IP
            openLobbyHost = new LobbyHost(8);

            if (!JoinLobby(IPAddress.Parse("127.0.0.1"), true))
            {
                CloseAllWindows();
            }
            else
            {
                ((MultiplayerLobbyHandler)openMenu).BeginGUI(true, openLobbyHost);
            }
            tryingToConnect = false;
        }

        /// <summary>
        /// Attempts to use the previously entered IP address to join
        /// an existing multiplayer game lobby. If the text is not a valid
        /// IP address or if a game can't be joined at that address, displays
        /// an error.
        /// </summary>
        private void multiplayerJoinButton_Click(object sender, EventArgs e)
        {
            if (tryingToConnect) return;

            tryingToConnect = true;

            // Try to grab the IP
            IPAddress ip;

            // If the IP is invalid, we display an error message
            if (!IPAddress.TryParse(ipTextbox.Text, out ip)){
                DisplayError("Invalid IP address.");
                return;
            }

            // Otherwise, we attempt to join a lobby at that address
            if (JoinLobby(ip, false))
            {
                ((MultiplayerLobbyHandler)openMenu).BeginGUI(false, null);
            }

            tryingToConnect = false;
        }

        /// <summary>
        /// Attempts to launch into a game lobby at the given IP address.
        /// If there is no lobby at that IP, displays an error.
        /// Returns whether or not joining the lobby succeeded.
        /// </summary>
        private bool JoinLobby(IPAddress ip, bool joiningSelf)
        {
            try {
                // Close the currently open lobby and make a new one
                if (joiningSelf) // If we're joining this computer, we don't want
                    CloseOpenMenu(); // to close the host at this computer.
                else
                    CloseAllWindows();

                MultiplayerLobbyHandler lobby = new MultiplayerLobbyHandler(ip);
                openMenu = lobby;
                return true;
            }
            catch(InvalidOperationException) // Couldn't find lobby at the IP
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

        private void CloseAllWindows()
        {
            CloseOpenMenu();
            if (openLobbyHost != null)
            {
                openLobbyHost.Close();
                openLobbyHost = null;
            }
        }
        private void CloseOpenMenu()
        {
            if (openMenu != null)
            {
                openMenu.Close();
                openMenu = null;
            }
        }
    }
}
