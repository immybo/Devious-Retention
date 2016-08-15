using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Represents a transformation from world coordinates
    /// to actual coordinates on a graphics pane.
    /// </summary>
    public class PositionTransformation
    {
        private int xOffset;
        private int yOffset;
        // Multipliers are applied before offset
        private float xMultiplier;
        private float yMultiplier;

        /// <summary>
        /// Creates a position transformation that can take a world coordinate
        /// and convert it to a point on the graphics pane by:
        /// - Multiplying it by the x and y multipliers, then
        /// - Adding the x and y offsets
        /// </summary>
        public PositionTransformation(int xOffset, int yOffset, float xMultiplier, float yMultiplier)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.xMultiplier = xMultiplier;
            this.yMultiplier = yMultiplier;
        }

        /// <summary>
        /// Takes a point as a world coordinate and returns the corresponding
        /// point on the graphics pane according to this transformation.
        /// </summary>
        public Point Transform(PointF worldCoordinate)
        {
            return new Point(
                (int)(worldCoordinate.X * xMultiplier + xOffset),
                (int)(worldCoordinate.Y * yMultiplier + yOffset)
            );
        }

        /// <summary>
        /// Takes a point on the graphics pane and returns the corresponding
        /// world coordinate according to the reverse of this transformation.
        /// </summary>
        public PointF TransformReverse(Point graphicsPoint)
        {
            return new PointF(
                ((float)graphicsPoint.X - xOffset) / xMultiplier,
                ((float)graphicsPoint.Y - yOffset) / yMultiplier
            );
        }

        /// <summary>
        /// Returns the multiplicative scale of this position transformation.
        /// </summary>
        public PointF Scale()
        {
            return new PointF(xMultiplier, yMultiplier);
        }
    }
}
