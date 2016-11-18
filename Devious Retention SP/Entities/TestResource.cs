using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    public class TestResource : Resource
    {
        public TestResource(int x, int y, double size, int resourceType, int maxResourceAmount, int resourceAmount)
            : base(x, y, size, "TestResource", resourceType, maxResourceAmount, resourceAmount)
        {
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            PointF topLeft = p.Transform(this.GetPosition());
            Rectangle rect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(p.Scale().X * this.Size), (int)(p.Scale().Y * this.Size));
            g.FillRectangle(new SolidBrush(Color.DarkGray), rect);
            g.DrawString("Resource", new Font("Arial", 14), new SolidBrush(Color.White), new PointF(topLeft.X + 20, topLeft.Y + 20));
        }
    }
}
