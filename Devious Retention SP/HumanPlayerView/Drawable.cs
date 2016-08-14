using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public interface Drawable
    {
        // TODO Make a class to allow the drawable object to check whether a point is out of bounds
        // to reduce redundant drawing (i.e. we don't want to waste time drawing things off-screen)
        // This could be done with a simple rectangle but it would involve duplicated, un-extendable code
        void Draw(Graphics g, PositionTransformation p);
    }
}
