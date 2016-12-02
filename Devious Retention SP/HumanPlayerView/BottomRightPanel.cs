using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Devious_Retention_SP.HumanPlayerView
{
    class BottomRightPanel : Panel
    {
        private const int ICON_SIZE = 48;
        private const int MARGIN = 20;

        private Image[] damageIcons;
        private Image healthIcon;
        private Image movementSpeedIcon;
        private Image[] resourceIcons;

        private Font titleFont;
        private Font regularFont;

        private Brush fontBrush;

        // The bottom right panel needs to keep reference to the player
        // so it knows what the player has selected.
        private HumanPlayer player;

        public BottomRightPanel(HumanPlayer player)
        {
            this.Paint += new PaintEventHandler(Render);
            this.DoubleBuffered = true;
            this.player = player;

            titleFont = new Font("Arial", 28);
            regularFont = new Font("Arial", 14);
            fontBrush = Brushes.Black;
            
            damageIcons = new Image[3];
            damageIcons[0] = Image.FromFile("../../Images/DamageTypeIcons/melee.png");
            damageIcons[1] = Image.FromFile("../../Images/DamageTypeIcons/ranged.png");
            damageIcons[2] = Image.FromFile("../../Images/DamageTypeIcons/explosion.png");

            resourceIcons = new Image[4];
            resourceIcons[0] = Image.FromFile("../../Images/ResourceIcons/energy.png");
            resourceIcons[1] = Image.FromFile("../../Images/ResourceIcons/metal.png");
            resourceIcons[2] = Image.FromFile("../../Images/ResourceIcons/oil.png");
            resourceIcons[3] = Image.FromFile("../../Images/ResourceIcons/science.png");

            healthIcon = Image.FromFile("../../Images/MiscIcons/health.png");
            movementSpeedIcon = Image.FromFile("../../Images/MiscIcons/speed.png");
        }

        public void Render(object sender, PaintEventArgs e)
        {
            // Draw the selected entity if they have one
            if (player.GetSelectedEntities().Length == 1)
            {
                RenderEntityDisplay(e.Graphics, player.GetSelectedEntities()[0]);
            }
            // If they have multiple selected, show their icons
            else if(player.GetSelectedEntities().Length > 1)
            {
                RenderEntityIcons(e.Graphics, player.GetSelectedEntities());
            }
            // If they have none selected, we don't need to draw anything
        }

        /// <summary>
        /// Draws descriptive information for a singular entity on the
        /// specified graphics pane.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        /// <param name="entity">The entity to display information of.</param>
        private void RenderEntityDisplay(Graphics g, Entity entity)
        {
            if (entity is Unit)
                RenderUnitDisplay(g, (Unit)entity);
            else if (entity is Building)
                RenderBuildingDisplay(g, (Building)entity);
            else if (entity is Resource)
                RenderResourceDisplay(g, (Resource)entity);
            else
                throw new ArgumentException("Attempting to render display of an entity that isn't a known type.");
        }

        /// <summary>
        /// Draws descriptive information for a singular unit on the
        /// specified graphics pane.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        /// <param name="unit">The unit to display information of.</param>
        private void RenderUnitDisplay(Graphics g, Unit unit)
        {
            PointF currentPoint = new PointF(MARGIN, MARGIN);

            g.DrawString(unit.Name, titleFont, fontBrush, currentPoint);
            currentPoint.Y += titleFont.Size + MARGIN * 2;

            g.DrawImage(healthIcon, new RectangleF(currentPoint.X, currentPoint.Y, ICON_SIZE, ICON_SIZE));
            currentPoint.X += ICON_SIZE + MARGIN;
            currentPoint.Y += 10;
            g.DrawString(unit.Hitpoints + " / " + unit.MaxHitpoints, regularFont, fontBrush, currentPoint);
            currentPoint.X = MARGIN;
            currentPoint.Y += ICON_SIZE + MARGIN - 10;
        }

        /// <summary>
        /// Draws descriptive information for a singular building on the
        /// specified graphics pane. This includes possibly buttons for 
        /// the training of units or researching of technologies.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        /// <param name="building">The building to display information of.</param>
        private void RenderBuildingDisplay(Graphics g, Building building)
        {
            PointF currentPoint = new PointF(MARGIN, MARGIN);

            g.DrawString(building.Name, titleFont, fontBrush, currentPoint);
            currentPoint.Y += titleFont.Size + MARGIN * 2;

            g.DrawImage(healthIcon, new RectangleF(currentPoint.X, currentPoint.Y, ICON_SIZE, ICON_SIZE));
            currentPoint.X += ICON_SIZE + MARGIN;
            currentPoint.Y += 10;
            g.DrawString(building.Hitpoints + " / " + building.MaxHitpoints, regularFont, fontBrush, currentPoint);
            currentPoint.X = MARGIN;
            currentPoint.Y += ICON_SIZE + MARGIN - 10;

            if (!building.IsFullyBuilt)
            {
                g.DrawString("Not built yet!", regularFont, fontBrush, currentPoint);
            }
        }

        /// <summary>
        /// Draws descriptive information for a singular resource on the
        /// specified graphics pane.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        /// <param name="resource">The resource to display information of.</param>
        private void RenderResourceDisplay(Graphics g, Resource resource)
        {
            PointF currentPoint = new PointF(MARGIN, MARGIN);

            g.DrawString(resource.Name, titleFont, fontBrush, currentPoint);
            currentPoint.Y += titleFont.Size + MARGIN * 2;

            g.DrawImage(resourceIcons[resource.ResourceType], new RectangleF(currentPoint.X, currentPoint.Y, ICON_SIZE, ICON_SIZE));
            currentPoint.X += ICON_SIZE + MARGIN;
            currentPoint.Y += 10;
            g.DrawString(resource.CurrentResourceCount() + " / " + resource.MaxResourceCount(), regularFont, fontBrush, currentPoint);
            currentPoint.X = MARGIN;
            currentPoint.Y += ICON_SIZE + MARGIN - 10;
        }

        /// <summary>
        /// Draws a set of icons corresponding to the given entities on
        /// the given graphics pane.
        /// </summary>
        /// <param name="g">The graphics to draw on.</param>
        /// <param name="entities">The entities to display icons of.</param>
        private void RenderEntityIcons(Graphics g, Entity[] entities)
        {

        }
    }
}
