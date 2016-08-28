using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Denotes a class which can listen to the actions of a human player.
    /// </summary>
    public interface HumanPlayerListener
    {
        void DoKeyPress(PointF worldCoordinate, Keys keys);
        void DoGameAreaClick(PointF worldCoordinate, MouseButtons buttons);
    }
}
