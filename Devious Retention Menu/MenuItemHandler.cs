using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_Menu
{
    /// <summary>
    /// A menu item handler represents an open window launched from the menu,
    /// which can be closed.
    /// </summary>
    interface MenuItemHandler
    {
        void Close();
    }
}
