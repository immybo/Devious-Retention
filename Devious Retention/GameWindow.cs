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
    public partial class GameWindow : Form
    {
        // 1 = entire width/height, 0 = nothing
        private const double GAME_AREA_WIDTH = 0.75;
        private const double GAME_AREA_HEIGHT = 0.95;
        private const double MINIMAP_WIDTH = 0.15;
        private const double TOP_RIGHT_HEIGHT = 0.5;

        private const int HORIZONTAL_TILES = 10;

        private const double RESOURCE_WIDTH = 0.2;

        public GameClient client;

        // Where the mouse started dragging, for selection purposes
        private double startX = -1;
        private double startY = -1;

        // Images for the resource display area and tooltips
        private Image[] resourceImages;
        private Image resourceDisplayAreaBackgroundImage;

        public GameWindow()
        {
            InitializeComponent();

            resourceImages = new Image[GameInfo.RESOURCE_TYPES];
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                resourceImages[i] = Image.FromFile(GameInfo.RESOURCE_ICON_IMAGE_BASE + GameInfo.RESOURCE_ICON_NAMES[i]);
            resourceDisplayAreaBackgroundImage = Image.FromFile(GameInfo.BACKGROUND_IMAGE_BASE + GameInfo.BACKGROUND_IMAGE_RESOURCE_DISPLAY_AREA);

            Paint += Render;
        }

        /// <summary>
        /// Returns the entity, if there is one, which is displayed at (x,y)
        /// on the the screen for the client. This means that, if there are
        /// overlapping entities, the one in front will be returned.
        /// </summary>
        /// <returns>The entity at the position, or void if there was none.</returns>
        public Entity GetEntityAt(double x, double y)
        {
            return null;
        }

        /// <summary>
        /// Returns the entities, if there are any, which are contained within
        /// the rectangle of the display with corners at (x1,y1),(x1,y2),(x2,y1),
        /// (x2,y2), from the client's perspective.
        /// </summary>
        /// <returns>The set of entities that were within the rectangle, or void if there were none.</returns>
        public HashSet<Entity> GetEntitiesIn(double x1, double y1, double x2, double y2)
        {
            return null;
        }

        /// <summary>
        /// Calls all RenderX methods in appropriate order (preceding them with ResizeToFit)
        /// </summary>
        private void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int panelWidth = (int)((1 - GAME_AREA_WIDTH) * Width);
            int panelHeight = (int)(TOP_RIGHT_HEIGHT * Height);
            int panelX = (int)(GAME_AREA_WIDTH * Width);
            int panelY = 0;
            ResizeToFit();
            RenderTiles(g,
                new Rectangle(0,0,(int)(Width*GAME_AREA_WIDTH),(int)(Height* GAME_AREA_HEIGHT)));
            RenderEntities(g,
                new Rectangle(0, 0, (int)(Width * GAME_AREA_WIDTH), (int)(Height * GAME_AREA_HEIGHT)));
            RenderResourceDisplayArea(g,
                new Rectangle(0, (int)(Height * GAME_AREA_HEIGHT), (int)(GAME_AREA_WIDTH * Width), (int)((1 - GAME_AREA_HEIGHT) * Height)));
            RenderTopRightPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH * Width), 0, (int)((1 - GAME_AREA_WIDTH) * Width), (int)(TOP_RIGHT_HEIGHT * Height)));
            RenderSelectedEntityPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH*Width),(int)(TOP_RIGHT_HEIGHT* Height),(int)((1-GAME_AREA_WIDTH) * Width), (int)((1-TOP_RIGHT_HEIGHT)* Height)));
            RenderMinimap(g,
                new Rectangle((int)((GAME_AREA_WIDTH-MINIMAP_WIDTH) * Width), (int)(GAME_AREA_HEIGHT* Height - MINIMAP_WIDTH* Width), (int)(MINIMAP_WIDTH* Width), (int)(MINIMAP_WIDTH* Width)));
        }
    
        /// <summary>
        /// Attempts to resize the window to the dimensions of the screen.
        /// </summary>
        private void ResizeToFit()
        {
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
        }

        /// <summary>
        /// Renders the game panel; i.e. the part of the window which contains
        /// the entities, tiles, etc. Also renders the resource counts and minimap.
        /// Uses the client's perspective to do so.
        /// </summary>
        /*private void RenderGamePanel(Graphics g)
        {
            int panelWidth = (int)(GAME_AREA_WIDTH * Width);
            int panelHeight = (int)(GAME_AREA_HEIGHT * Height);
            int panelX = 0;
            int panelY = 0;
            
            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(panelX, panelY, panelWidth, panelHeight));

            Font font = new Font("Arial", 50, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Game area center", font, Brushes.Black, new PointF(panelWidth / 2, panelHeight / 2), format);

            // DRAW THE TILES //
            int tileWidth = (int) (panelWidth / HORIZONTAL_TILES);
            int tileHeight = tileWidth;

            // How much of a tile we have to draw above the screen
            int topTileYOffset = (int)((client.screenY - (int)client.screenY) * tileHeight);
            // How much of a tile we have to draw to the left of the screen
            int topTileXOffset = (int)((client.screenX - (int)client.screenX) * tileWidth);

            for (int i = 0; i + client.screenX < client.map.width; i++)
            {
                // If we're already off the edge of the screen, top drawing
                if (i * tileWidth >= panelWidth) break;
                for(int j = 0; j + client.screenY < client.map.height; j++)
                {
                    // We allow tiles to go slightly off the side, under the assumption that the GUI will be painted in front of them
                    // We draw tiles from the floor value of the screen position, and then position them off the screen so that the appropriate amount is displayed
                    g.DrawImage(client.map.GetTile(i+(int)client.screenX, j+(int)client.screenY).image, new Rectangle(i*tileWidth - topTileXOffset,j*tileHeight - topTileYOffset,tileWidth, tileHeight));
                }
            }
        }*/

        /// <summary>
        /// Renders all tiles on the map
        /// </summary>
        private void RenderTiles(Graphics g, Rectangle bounds)
        {
            // Figure out how large tiles are; they must always be square
            int tileWidth = (int)(bounds.Width / HORIZONTAL_TILES);
            int tileHeight = tileWidth;

            // Figure out how much of the top and left tiles must be cut off the screen, due
            // to the camera position being potentially not an integer value
            int topTileYOffset = (int)((client.screenY - (int)client.screenY) * tileHeight);
            int topTileXOffset = (int)((client.screenX - (int)client.screenX) * tileWidth);

            // Figure out how many tiles we can draw on the screen
            int maxXTiles = HORIZONTAL_TILES;
            int maxYTiles = (int)(Math.Ceiling((double)bounds.Height / tileHeight)); // better too many than too few since we draw over the edges anyway

            for (int i = 0; i + client.screenX < client.map.width && i < maxXTiles; i++)
            {
                for (int j = 0; j + client.screenY < client.map.height && j < maxYTiles; j++)
                {
                    // We allow tiles to go slightly off the side, under the assumption that the GUI will be painted in front of them
                    // We draw tiles from the floor value of the screen position, and then position them off the screen so that the appropriate amount is displayed
                    g.DrawImage(client.map.GetTile(i + (int)client.screenX, j + (int)client.screenY).image, new Rectangle(i * tileWidth - topTileXOffset, j * tileHeight - topTileYOffset, tileWidth, tileHeight));
                }
            }
        }

        /// <summary>
        /// Renders all entities on the map - 
        /// gets them from the client.
        /// </summary>
        private void RenderEntities(Graphics g, Rectangle bounds)
        {
            // Figure out how large tiles are; they must always be square
            int tileWidth = (int)(bounds.Width / HORIZONTAL_TILES);
            int tileHeight = tileWidth;

            // Figure out how many tiles we can draw on the screen
            int maxXTiles = HORIZONTAL_TILES;
            int maxYTiles = (int)(Math.Ceiling((double)bounds.Height / tileHeight));

            // Collect all entities into a big list : resources first, then buildings, then units. This means that resources are on the bottom and units on the top
            List<Entity> entities = new List<Entity>();
            foreach (Resource r in client.resources)
                entities.Add(r);
            foreach (Building b in client.buildings)
                entities.Add(b);
            foreach (Unit u in client.units)
                entities.Add(u);

            // Render them all
            foreach(Entity e in entities)
            {
                // First check if they're even on the screen
                if (e.GetX() + e.GetSize() < client.screenX || e.GetX() > client.screenX + maxXTiles) continue;
                if (e.GetY() + e.GetSize() < client.screenY || e.GetY() > client.screenY + maxYTiles) continue;

                // Since they are on the screen, figure out their bounds
                Rectangle entityBounds = new Rectangle();
                entityBounds.X = (int)((e.GetX() - client.screenX) * tileWidth); // their distance from the left/top of the screen
                entityBounds.Y = (int)((e.GetY() - client.screenY) * tileHeight);
                entityBounds.Width = (int)(e.GetSize() * tileWidth);
                entityBounds.Height = (int)(e.GetSize() * tileHeight);

                // And finally, draw them
                g.DrawImage(e.GetImage(), entityBounds);
            }
        }

        /// <summary>
        /// Renders the resource display area at the bottom of the screen.
        /// </summary>
        private void RenderResourceDisplayArea(Graphics g, Rectangle bounds)
        {
            g.DrawImage(resourceDisplayAreaBackgroundImage, bounds);

            Font font = new Font("Arial", (int)(bounds.Height/1.5), FontStyle.Regular);

            int resourcePadding = 5;
            int resourceIconWidth = bounds.Height - resourcePadding * 2;
            int resourceTextWidth = (int)(RESOURCE_WIDTH*bounds.Width - resourceIconWidth);
            int resourceGapWidth = (int)((1 - GameInfo.RESOURCE_TYPES * RESOURCE_WIDTH) / (GameInfo.RESOURCE_TYPES+1) * bounds.Width);

            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++) {
                Rectangle imageBounds = new Rectangle();
                imageBounds.X = (int)(resourceGapWidth * (i + 1) + (resourceIconWidth+ resourceTextWidth) * i + bounds.X);
                imageBounds.Y = bounds.Y + resourcePadding;
                imageBounds.Width = resourceIconWidth;
                imageBounds.Height = resourceIconWidth;

                PointF textPoint = new PointF(imageBounds.X + resourceIconWidth + resourcePadding, bounds.Y);

                g.DrawImage(resourceImages[i], imageBounds);
                g.DrawString(client.currentResources[i] + "", font, Brushes.Black, textPoint);
            }
        }

        /// <summary>
        /// Renders the minimap.
        /// </summary>
        private void RenderMinimap(Graphics g, Rectangle bounds)
        {
            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), bounds);

            Font font = new Font("Arial", 30, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Minimap", font, Brushes.Black, new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2), format);
        }

        /// <summary>
        /// Renders the selected entity panel; i.e. the part of the window which
        /// allows the player to perform actions from the selected entity.
        /// </summary>
        private void RenderSelectedEntityPanel(Graphics g, Rectangle bounds)
        {
            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), bounds);

            Font font = new Font("Arial", 30, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Selected entity panel", font, Brushes.Black, new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2), format);
        }

        /// <summary>
        /// Renders the top right panel; i.e. the part of the window which allows
        /// the user to either select a building to create or a technology to 
        /// research.
        /// </summary>
        private void RenderTopRightPanel(Graphics g, Rectangle bounds)
        {
            g.DrawRectangle(new Pen(new SolidBrush(Color.Black)), bounds);

            Font font = new Font("Arial", 50, FontStyle.Regular);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawString("Top right panel", font, Brushes.Black, new PointF(bounds.X + bounds.Width / 2, bounds.Height / 2), format);
        }

        /// <summary>
        /// Processes any key events on the game window. If they are recognised as
        /// utilised keys, performs the appropriate action on the client.
        /// </summary>
        public void KeyEvent(KeyEventArgs e)
        {

        }

        /// <summary>
        /// Processes any mouse events on the game window. Usually performs the
        /// appropriate action on the client.
        /// </summary>
        public void MouseEvent(MouseEventArgs e)
        {

        }
    }
}
