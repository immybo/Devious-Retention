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
    /// <summary>
    /// Displays lots of information to the user, particularly useful for debugging.
    /// This window appearing can be toggled, and should usually be off for regular use.
    /// </summary>
    public partial class DebugWindow : Form
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Writes a new line of text to the debug textbox, in the specified color.
        /// </summary>
        public void WriteLine(String s, Color c)
        {
            debugText.SelectionColor = c;
            debugText.AppendText(s + "\n");
            Refresh();
        }

        /// <summary>
        /// Writes a new line of text to the debug textbox, in the default color.
        /// </summary>
        public void WriteLine(String s)
        {
            debugText.SelectionColor = Color.Black;
            debugText.AppendText(s + "\n");
            Refresh();
        }
    }
}
