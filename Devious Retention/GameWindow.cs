using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        private const double GAME_AREA_HEIGHT = 0.90;
        private const double MINIMAP_WIDTH = 0.15;
        private const double TOP_RIGHT_HEIGHT = 0.5;

        private const int HORIZONTAL_TILES = 10;

        // How much the screen moves every tick of holding the button down
        private const int SCREEN_X_CHANGE = 1;
        private const int SCREEN_Y_CHANGE = 1;

        // How large the building/technology icons are, and the gaps between them (pixels)
        private const int ICON_SIZE = 50;
        private const int ICON_GAP = 20;

        private const double RESOURCE_WIDTH = 0.2;

        // How opaque the overlay to tiles which we don't have LOS to is
        private const int OVERLAY_ALPHA = 100;

        // relative to the width of the selection panel
        private const double DAMAGE_ICON_SIZE = 0.06;
        private const double TRAINING_QUEUE_ICON_SIZE = 0.19;
        // pixels
        private const int MINIMAP_BORDER_SIZE = 20;
        private const int RIGHT_AREA_BORDER_WIDTH = 20;
        private const int RESOURCE_AREA_BORDER_WIDTH = 20;

        public GameClient client;

        // Where the top-left of the screen is, in map co-ordinates.
        public double screenY { get; private set; } = 0;
        public double screenX { get; private set; } = 0;

        // Where the mouse started dragging, for selection purposes
        private double startX = -1;
        private double startY = -1;

        // Whether the building panel or the technology panel is open
        private bool buildingPanelOpen = true;

        // What tiles are within the player's line of sight
        private bool[,] LOS;

        // Images for the resource display area and tooltips
        private Image[] resourceImages;
        // Images for different damage types in the selected entity area
        private Image[] damageTypeIcons;

        private Image resourceDisplayAreaBackgroundImage;
        private Image selectedEntityAreaBackgroundImage;
        private Image minimapBackgroundImage;

        private Image minimapBorderLeft;
        private Image minimapBorderTop;
        private Image resourceAreaBorderTop;
        private Image rightAreaBorderLeft;

        public GameWindow()
        {
            InitializeComponent();

            // Load all the images
            resourceImages = new Image[GameInfo.RESOURCE_TYPES];
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
                resourceImages[i] = Image.FromFile(GameInfo.RESOURCE_ICON_IMAGE_BASE + GameInfo.RESOURCE_ICON_NAMES[i]);
            damageTypeIcons = new Image[GameInfo.DAMAGE_TYPES];
            for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                damageTypeIcons[i] = Image.FromFile(GameInfo.DAMAGE_TYPE_ICON_IMAGE_BASE + GameInfo.DAMAGE_TYPE_ICON_NAMES[i]);

            resourceDisplayAreaBackgroundImage = Image.FromFile(GameInfo.BACKGROUND_IMAGE_BASE + GameInfo.BACKGROUND_IMAGE_RESOURCE_DISPLAY_AREA);
            selectedEntityAreaBackgroundImage = Image.FromFile(GameInfo.BACKGROUND_IMAGE_BASE + GameInfo.BACKGROUND_IMAGE_SELECTED_ENTITY_AREA);
            minimapBackgroundImage = Image.FromFile(GameInfo.BACKGROUND_IMAGE_BASE + GameInfo.BACKGROUND_IMAGE_MINIMAP);

            minimapBorderLeft = Image.FromFile(Path.Combine(GameInfo.BORDER_IMAGE_BASE,GameInfo.BORDER_MINIMAP_LEFT));
            minimapBorderTop = Image.FromFile(Path.Combine(GameInfo.BORDER_IMAGE_BASE,GameInfo.BORDER_MINIMAP_TOP));
            resourceAreaBorderTop = Image.FromFile(Path.Combine(GameInfo.BORDER_IMAGE_BASE, GameInfo.BORDER_RESOURCE_AREA_TOP));
            rightAreaBorderLeft = Image.FromFile(Path.Combine(GameInfo.BORDER_IMAGE_BASE, GameInfo.BORDER_RIGHT_AREA_LEFT));

            Paint += Render;
            KeyDown += new KeyEventHandler(KeyEvent);
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

            ResizeToFit();
            //LoadLOS();
            RenderTiles(g,
                new Rectangle(0,0,(int)(Width*GAME_AREA_WIDTH),(int)(Height* GAME_AREA_HEIGHT)));
            RenderEntities(g,
                new Rectangle(0, 0, (int)(Width * GAME_AREA_WIDTH), (int)(Height * GAME_AREA_HEIGHT)));
            RenderResourceDisplayArea(g,
                new Rectangle(0, (int)(Height * GAME_AREA_HEIGHT), (int)(GAME_AREA_WIDTH * Width), (int)((1 - GAME_AREA_HEIGHT) * Height)));
            RenderSelectedEntityPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH*Width),(int)(TOP_RIGHT_HEIGHT* Height),(int)((1-GAME_AREA_WIDTH) * Width), (int)((1-TOP_RIGHT_HEIGHT)* Height)));
            RenderTopRightPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH * Width), 0, (int)((1 - GAME_AREA_WIDTH) * Width), (int)(TOP_RIGHT_HEIGHT * Height)));
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
        /// Assuming that the client has been set, initialises the line of sight.
        /// </summary>
        public void InitLOS()
        {
            LOS = new bool[client.map.width, client.map.height];
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given entity was just created.
        /// Assumes that this entity belongs to the player.
        /// Does nothing if this entity is a resource.
        /// </summary>
        public void UpdateLOSAdd(Entity e)
        {
            // Resources don't have LOS
            if (e is Resource) return;

            int entityLOS = e.GetLOS();
            // Just round it down for simplicity
            int entityX = (int)(e.GetX() + e.GetSize() / 2);
            int entityY = (int)(e.GetY() + e.GetSize() / 2);

            // Simple way of figuring out a circle
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    // Are we even on the map?
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;

                    // Find the distance from the entity (pythagoras)
                    int distance = (int)(Math.Sqrt(Math.Pow(entityX - x, 2) + Math.Pow(entityY - y, 2)));
                    // Do nothing if it's too far away
                    if (distance > entityLOS) continue;

                    // Otherwise add this square to LOS
                    LOS[x, y] = true;
                }
            }
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given unit has just moved by (dX,dY).
        /// Assumes that the unit belongs to the player.
        /// </summary>
        public void UpdateLOSMove(Unit unit, double dX, double dY)
        {
            // The new LOS of the unit
            List<Coordinate> newTiles = new List<Coordinate>();
            // The old LOS of the unit
            List<Coordinate> oldTiles = new List<Coordinate>();

            // Figure out the old circle
            int oldUnitX = (int)(unit.GetX() + unit.GetSize() / 2 - dX);
            int oldUnitY = (int)(unit.GetY() + unit.GetSize() / 2 - dY);
            for (int x = oldUnitX - unit.GetLOS(); x <= oldUnitX + unit.GetLOS(); x++)
            {
                for (int y = oldUnitY - unit.GetLOS(); y <= oldUnitY + unit.GetLOS(); y++)
                {
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(oldUnitX - x, 2) + Math.Pow(oldUnitY - y, 2)));
                    if (distance > unit.GetLOS()) continue;

                    // This is one of the tiles that the unit used to be able to see
                    oldTiles.Add(new Coordinate(x, y));
                }
            }

            // Figure out the new circle
            int newUnitX = (int)(unit.GetX() + unit.GetSize() / 2);
            int newUnitY = (int)(unit.GetY() + unit.GetSize() / 2);
            for (int x = newUnitX - unit.GetLOS(); x <= newUnitX + unit.GetLOS(); x++)
            {
                for (int y = newUnitY - unit.GetLOS(); y <= newUnitY + unit.GetLOS(); y++)
                {
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(newUnitX - x, 2) + Math.Pow(newUnitY - y, 2)));
                    if (distance > unit.GetLOS()) continue;

                    // This is one of the tiles that the unit can now see
                    newTiles.Add(new Coordinate(x, y));
                }
            }

            // The tiles that it can't see any more
            List<Coordinate> nowInvisibleTiles = new List<Coordinate>();
            // The tiles that it couldn't see before but can now
            List<Coordinate> nowVisibleTiles = new List<Coordinate>();
            
            // Add tiles to the list of tiles that we can't see any more... only if we can't see them any more
            foreach (Coordinate oldTile in oldTiles)
                if (!newTiles.Contains(oldTile))
                    nowInvisibleTiles.Add(oldTile);
            // Add tiles to the list of tiles that we can see now, only if we couldn't see them before
            foreach (Coordinate newTile in newTiles)
                if (!oldTiles.Contains(newTile))
                    nowVisibleTiles.Add(newTile);
            
            // Set all the newly visible tiles to be within LOS
            foreach(Coordinate c in nowVisibleTiles)
            {
                if (c.x >= client.map.width || c.y >= client.map.height) continue;
                if (c.x < 0 || c.y < 0) continue;
                LOS[c.x, c.y] = true;
            }

            // And check if we can still see the old tiles
            foreach(Coordinate c in nowInvisibleTiles)
                LOS[c.x,c.y] = HasLOSTo(c);
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given entity was just deleted.
        /// Assumes that the entity belonged to the player.
        /// </summary>
        public void UpdateLOSDelete(Entity entity)
        {
            if (entity is Resource) return;

            int entityLOS = entity.GetLOS();
            int entityX = (int)(entity.GetX() + entity.GetSize() / 2);
            int entityY = (int)(entity.GetY() + entity.GetSize() / 2);
            // Go through all the tiles the entity could see and recheck if we can still see them
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(entityX - x, 2) + Math.Pow(entityY - y, 2)));
                    if (distance > entityLOS) continue;

                    // Check whether or not we can still see this tile
                    LOS[x, y] = HasLOSTo(new Coordinate(x, y));
                }
            }
        }

        /// <summary>
        /// Returns whether or not the player has line of sight to
        /// the given coordinate.
        /// </summary>
        private bool HasLOSTo(Coordinate c)
        {
            // Scroll through units and buildings that belong to the player, and figure out which are within range
            // Stop if we find one that is
            HashSet<Entity> entities = new HashSet<Entity>();
            foreach (Unit u in client.units)
                if (u.player == client.playerNumber)
                    entities.Add(u);
                    
            foreach (Building b in client.buildings)
                if (b.player == client.playerNumber)
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                // Distance between the entity and the tile
                int distance = (int)(Math.Sqrt(Math.Pow(e.GetX() - c.x, 2) + Math.Pow(e.GetY() - c.y, 2)));
                if (distance <= e.GetLOS()) return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to figure out the player's line of sight, and stores it in "LOS"
        /// </summary>
        private void LoadLOS()
        {
            // Clear the current LOS
            LOS = new bool[client.map.width, client.map.height];

            List<Entity> entities = new List<Entity>();
            foreach (Unit u in client.units)
                if (u.player == client.playerNumber)
                    entities.Add(u);
            foreach (Building b in client.buildings)
                if (b.player == client.playerNumber)
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                int entityLOS = e.GetLOS();
                // Just round it down for simplicity
                int entityX = (int)(e.GetX()+e.GetSize()/2);
                int entityY = (int)(e.GetY()+e.GetSize()/2);

                // Simple way of figuring out a circle
                for(int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
                {
                    for(int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                    {
                        // Are we even on the map?
                        if (x < 0 || y < 0) continue;
                        if (x >= client.map.width || y >= client.map.height) continue;

                        // Find the distance from the entity (pythagoras)
                        int distance = (int)(Math.Sqrt(Math.Pow(entityX-x,2) + Math.Pow(entityY-y,2)));
                        // Do nothing if it's too far away
                        if (distance > entityLOS) continue;
                        
                        // Otherwise add this square to LOS
                        LOS[x,y] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Renders all tiles on the map
        /// </summary>
        private void RenderTiles(Graphics g, Rectangle bounds)
        {
            // Clip the output to a specific region; the game area minus the minimap
            Rectangle clipRect1 = new Rectangle(0,0,(int)((GAME_AREA_WIDTH-MINIMAP_WIDTH)*Width), (int)(GAME_AREA_HEIGHT* Height));
            Rectangle clipRect2 = new Rectangle(0, 0, (int)(GAME_AREA_WIDTH * Width), (int)(GAME_AREA_HEIGHT * Height - MINIMAP_WIDTH * Width));
            g.SetClip(clipRect1);
            g.SetClip(clipRect2, CombineMode.Union);

            // Figure out how large tiles are; they must always be square
            int tileWidth = (int)(bounds.Width / HORIZONTAL_TILES);
            int tileHeight = tileWidth;

            // Figure out how much of the top and left tiles must be cut off the screen, due
            // to the camera position being potentially not an integer value
            int topTileYOffset = (int)((screenY - (int)screenY) * tileHeight);
            int topTileXOffset = (int)((screenX - (int)screenX) * tileWidth);

            // Figure out how many tiles we can draw on the screen
            int maxXTiles = HORIZONTAL_TILES;
            int maxYTiles = (int)(Math.Ceiling((double)bounds.Height / tileHeight)); // better too many than too few since we draw over the edges anyway

            for (int i = 0; i + screenX < client.map.width && i < maxXTiles; i++)
            {
                if (i + (int)screenX < 0) continue;
                for (int j = 0; j + screenY < client.map.height && j < maxYTiles; j++)
                {
                    if (j + (int)screenY < 0) continue;
                    // We allow tiles to go slightly off the side, under the assumption that the GUI will be painted in front of them
                    // We draw tiles from the floor value of the screen position, and then position them off the screen so that the appropriate amount is displayed
                    g.DrawImage(client.map.GetTile(i + (int)screenX, j + (int)screenY).image, new Rectangle(i * tileWidth - topTileXOffset, j * tileHeight - topTileYOffset, tileWidth, tileHeight));
                    // If this tile is out of line of sight, draw a light grey overlay (grey it out)
                    if (!LOS[i + (int)screenX, j + (int)screenY])
                        g.FillRectangle(new SolidBrush(Color.FromArgb(OVERLAY_ALPHA, Color.LightGray)), new Rectangle(i * tileWidth - topTileXOffset, j * tileHeight - topTileYOffset, tileWidth, tileHeight));
                }
            }

            g.SetClip(new Rectangle(0, 0, Width, Height));
        }

        /// <summary>
        /// Renders all entities on the map - 
        /// gets them from the client.
        /// </summary>
        private void RenderEntities(Graphics g, Rectangle bounds)
        {
            // Clip the output to a specific region; the game area minus the minimap
            Rectangle clipRect1 = new Rectangle(0, 0, (int)((GAME_AREA_WIDTH - MINIMAP_WIDTH) * Width), (int)(GAME_AREA_HEIGHT * Height));
            Rectangle clipRect2 = new Rectangle(0, 0, (int)(GAME_AREA_WIDTH * Width), (int)(GAME_AREA_HEIGHT * Height - MINIMAP_WIDTH * Width));
            g.SetClip(clipRect1);
            g.SetClip(clipRect2, CombineMode.Union);

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
                if (e.GetX() + e.GetSize() < screenX || e.GetX() > screenX + maxXTiles) continue;
                if (e.GetY() + e.GetSize() < screenY || e.GetY() > screenY + maxYTiles) continue;
                // And check if we have line of sight to them
                if (!LOS[(int)(e.GetX() + e.GetSize() / 2), (int)(e.GetY() + e.GetSize() / 2)]) continue;

                // Since they are on the screen, figure out their bounds
                Rectangle entityBounds = new Rectangle();
                entityBounds.X = (int)((e.GetX() - screenX) * tileWidth); // their distance from the left/top of the screen
                entityBounds.Y = (int)((e.GetY() - screenY) * tileHeight);
                entityBounds.Width = (int)(e.GetSize() * tileWidth);
                entityBounds.Height = (int)(e.GetSize() * tileHeight);

                // And finally, draw them
                g.DrawImage(e.GetImage(), entityBounds);
            }

            g.SetClip(new Rectangle(0, 0, Width, Height));
        }

        /// <summary>
        /// Renders the resource display area at the bottom of the screen.
        /// </summary>
        private void RenderResourceDisplayArea(Graphics g, Rectangle bounds)
        {
            // Account for the border
            bounds.Y += RESOURCE_AREA_BORDER_WIDTH;
            bounds.Height -= RESOURCE_AREA_BORDER_WIDTH;

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

                Point textPoint = new Point(imageBounds.X + resourceIconWidth + resourcePadding, bounds.Y);

                g.DrawImage(resourceImages[i], imageBounds);
                g.DrawString(client.currentResources[i] + "", font, Brushes.Black, textPoint);
        }

            // Draw the border
            g.DrawImage(resourceAreaBorderTop, new Rectangle(bounds.X, bounds.Y - RESOURCE_AREA_BORDER_WIDTH, bounds.Width, RESOURCE_AREA_BORDER_WIDTH));
        }

        /// <summary>
        /// Renders the minimap.
        /// </summary>
        private void RenderMinimap(Graphics g, Rectangle bounds)
        {
            // Figure out how many pixels each square should occupy 
            // (not an integer for accuracy of figuring out coordinates)
            double pixelsPerSquareX = (double)bounds.Width / client.map.width;
            double pixelsPerSquareY = (double)bounds.Height / client.map.height;

            // Draw the tiles first
            Bitmap tileImage = new Bitmap(client.map.width, client.map.height);

            // Set the appropriate colours for tiles
            for (int i = 0; i < client.map.width; i++)
                for (int j = 0; j < client.map.height; j++)
                    tileImage.SetPixel(i, j, client.map.GetTile(i, j).color);

            // Draw the entities next
            List<Entity> entities = new List<Entity>();
            foreach (Unit u in client.units)
                entities.Add(u);
            foreach (Building b in client.buildings)
                entities.Add(b);

            // Draw on top of the tile image
            foreach (Entity e in entities)
            {
                // Do nothing if we don't have line of sight there
                if (!LOS[(int)(e.GetX() + e.GetSize() / 2), (int)(e.GetY() + e.GetSize() / 2)]) continue;
                // Draw at most one tile worth of color, in the middle of the entity (may be important for large entities)
                tileImage.SetPixel((int)(e.GetX() + e.GetSize() / 2), (int)(e.GetY() + e.GetSize() / 2), GameInfo.PLAYER_COLORS[e.GetPlayerNumber()]);
        }

            g.InterpolationMode = InterpolationMode.NearestNeighbor; // Remove blur from scaling the image up, we want it to be sharp
            g.PixelOffsetMode = PixelOffsetMode.Half; // So that we don't cut off half of the top and left images

            g.DrawImage(tileImage,
                new Rectangle(bounds.X + MINIMAP_BORDER_SIZE, bounds.Y + MINIMAP_BORDER_SIZE,
                              bounds.Width - MINIMAP_BORDER_SIZE, bounds.Height - MINIMAP_BORDER_SIZE));

            g.DrawImage(minimapBorderLeft, new Rectangle(bounds.X, bounds.Y, MINIMAP_BORDER_SIZE, bounds.Height));
            g.DrawImage(minimapBorderTop, new Rectangle(bounds.X, bounds.Y, bounds.Width, MINIMAP_BORDER_SIZE));

            g.InterpolationMode = InterpolationMode.Default;
            g.PixelOffsetMode = PixelOffsetMode.Default;
        }

        /// <summary>
        /// Renders the selected entity panel; i.e. the part of the window which
        /// allows the player to perform actions from the selected entity.
        /// </summary>
        private void RenderSelectedEntityPanel(Graphics g, Rectangle bounds)
        {
            // Account for the border
            bounds.X += RIGHT_AREA_BORDER_WIDTH;
            bounds.Width -= RIGHT_AREA_BORDER_WIDTH;

            int fontSize = bounds.Width / 25;
            Font titleFont = new Font("Arial", (int)(fontSize*1.5), FontStyle.Regular);
            Font font = new Font("Arial", fontSize, FontStyle.Regular);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            // Draw background image
            g.DrawImage(selectedEntityAreaBackgroundImage, bounds);

            // Find out if we've got a unit, building or resource (or nothing)
            if (client.selected.Count == 0) return; // draw nothing if nothing is selected
            
            Entity entity = client.selected[0];
            // We draw the panel based off the first entity out of the list of selected entities;
            // generally the player should only be selecting one entity if they intend to view its attributes
            // or create units out of it
            
            if (entity is Building)
            {
                Building building = (Building)entity;
                // Middle x, top y
                Point drawPoint = new Point(bounds.X + bounds.Width / 2, bounds.Y + 10);
                // Draw the title in the middle
                g.DrawString(building.type.name, titleFont, Brushes.Black, drawPoint, format);

                // Now draw everything else on a left alignment
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += (int)(fontSize*1.5) + 20;

                // HITPOINTS
                g.DrawString("HP: " + building.hitpoints + "/" + building.type.hitpoints, font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // DAMAGE
                // Only draw the damage if this building can attack
                if (building.type.aggressive)
                {
                    g.DrawString("Damage:", font, Brushes.Black, drawPoint);
                    // icon for the damage type
                    g.DrawImage(damageTypeIcons[building.type.damageType],
                        new Rectangle(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 10,
                                      drawPoint.Y, (int)(DAMAGE_ICON_SIZE*bounds.Width),(int)(DAMAGE_ICON_SIZE*bounds.Width)));
                    g.DrawString(building.type.damage + "", font, Brushes.Black,
                        new Point(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 20 + (int)(DAMAGE_ICON_SIZE * bounds.Width),drawPoint.Y));
                    drawPoint.Y += fontSize + 10;

                    // RANGE
                    g.DrawString("Range: " + building.type.range, font, Brushes.Black, drawPoint);
                    drawPoint.Y += fontSize + 10;
                }

                // RESISTANCES
                g.DrawString("Resist:", font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString("Resist:", font).Width) + 10;
                for(int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                {
                    g.DrawImage(damageTypeIcons[i],
                        new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE*bounds.Width), (int)(DAMAGE_ICON_SIZE*bounds.Width)));
                    drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 10;
                    g.DrawString(building.type.resistances[i] + "%", font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString(building.type.resistances[i] + "%", font).Width) + 10;
                }

                // RESOURCE GATHER RATE
                // Only draw the resource gather rate if this building is built on a resource it can gather, or passively grants resources
                if (building.type.providesResource || (building.type.canBeBuiltOnResource && building.resource != null))
                {
                    double gatherRate;
                    if (building.type.providesResource) // passively provides resource
                        gatherRate = building.type.gatherSpeed;
                    else // actively provides resource
                        gatherRate = building.resource.type.gatherSpeed;

                    drawPoint.X = bounds.X + 10;
                    drawPoint.Y += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 20;
                    g.DrawString("Gathering " + gatherRate, font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString("Gathering " + gatherRate, font).Width) + 4;
                    g.DrawImage(resourceImages[building.type.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 2;
                    g.DrawString("/s", font, Brushes.Black, drawPoint);
                }

                // TRAINING QUEUE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 20;

                g.DrawString("Training Queue", font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // max visible unit types
                int maxUnits = (int)(1 / TRAINING_QUEUE_ICON_SIZE);

                List<UnitType> queueUnits = new List<UnitType>();
                List<int> queueUnitCounts = new List<int>();
                foreach(UnitType u in building.trainingQueue)
                {
                    if (queueUnits.Count >= maxUnits) break; // only display a certain amount of unittypes

                    // If this is the first unit, we can draw the progress towards finishing it
                    bool drawProgress = false;
                    if (queueUnits.Count == 0)
                        drawProgress = true;

                    // If it was already in queueUnits, add a number to it
                    if (queueUnits.Count != 0 && queueUnits[queueUnits.Count-1].Equals(u))
                        queueUnitCounts[queueUnits.Count-1]++;
                    // Otherwise actually draw the icon
                    else
                    {
                        queueUnits.Add(u);
                        queueUnitCounts.Add(1);

                        g.DrawImage(u.icon,
                            new Rectangle(drawPoint.X, drawPoint.Y, (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width), (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width)));

                        if (drawProgress)
                        {
                            int progress;
                            if (u.trainingTime == 0) progress = 100; // doesn't really matter but to avoid /0
                            else progress = (int)((u.trainingTime - building.trainingQueueTime) / u.trainingTime);
                            g.DrawString(progress + "%", font, Brushes.Black, drawPoint);
                        }

                        drawPoint.X += (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width) + 10;
                    }
                }
                // Draw the number to train
                drawPoint.X = bounds.X + 10 + (int)(TRAINING_QUEUE_ICON_SIZE*bounds.Width);
                drawPoint.Y += (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width);
                format.Alignment = StringAlignment.Far; // come from bottom right of icon
                format.LineAlignment = StringAlignment.Far;
                for(int i = 0; i < queueUnits.Count; i++)
                {
                    g.DrawString("x"+queueUnitCounts[i], font, Brushes.Black, drawPoint, format);
                    drawPoint.X += (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width) + 10;
                }

                // TRAINABLE UNITS
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += 20;

                g.DrawString("Trainable Units", font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                int currentOnLine = 0; // how many icons are on the current line
                foreach(string s in building.type.trainableUnits)
                {
                    if (client.info.unitTypes.ContainsKey(s))
                    {
                        UnitType u = client.info.unitTypes[s];
                        g.DrawImage(u.icon,
                            new Rectangle(drawPoint.X, drawPoint.Y, (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width), (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width)));

                        drawPoint.X += (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width) + 10;
                        currentOnLine++;
                        if (currentOnLine >= maxUnits)
                        { // Go down to next line
                            drawPoint.Y += (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width) + 10;
                            drawPoint.X = bounds.X + 10;
                        }
                    }
                    // Do nothing if the unit type doesn't exist or can't be found
                }
            }
            else if (entity is Unit)
            {
                Unit unit = (Unit)entity;
                Point drawPoint = new Point(bounds.X + 10, bounds.Y + 20 + (int)(fontSize * 1.5));
                g.DrawString(unit.type.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width / 2, bounds.Y + 10), format);
                
                // HITPOINTS
                g.DrawString("HP: " + unit.hitpoints + "/" + unit.type.hitpoints, font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // DAMAGE
                g.DrawString("Damage:", font, Brushes.Black, drawPoint);
                // icon for the damage type
                g.DrawImage(damageTypeIcons[unit.type.damageType],
                    new Rectangle(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 10,
                        drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    g.DrawString(unit.type.damage + "", font, Brushes.Black,
                        new Point(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 20 + (int)(DAMAGE_ICON_SIZE * bounds.Width), drawPoint.Y));
                drawPoint.Y += fontSize + 10;

                // RESISTANCES
                g.DrawString("Resist:", font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString("Resist:", font).Width) + 10;
                for (int i = 0; i < GameInfo.DAMAGE_TYPES; i++)
                {
                    g.DrawImage(damageTypeIcons[i],
                        new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 10;
                    g.DrawString(unit.type.resistances[i] + "%", font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString(unit.type.resistances[i] + "%", font).Width) + 10;
                }

                // RANGE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 10;
                g.DrawString("Range: " + unit.type.range, font, Brushes.Black, drawPoint);

                // SPEED
                drawPoint.Y += fontSize + 10;
                g.DrawString("Speed: " + unit.type.speed, font, Brushes.Black, drawPoint);
            }
            else if (entity is Resource)
            {
                Resource resource = (Resource)entity;
                Point drawPoint = new Point(bounds.X + 10, bounds.Y + 20 + (int)(fontSize * 1.5));
                g.DrawString(resource.type.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width / 2, bounds.Y + 10), format);

                // REMAINING RESOURCE
                g.DrawString(resource.amount+"/"+resource.type.resourceAmount, font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString(resource.amount+"/"+resource.type.resourceAmount, font).Width) + 4;
                g.DrawImage(resourceImages[resource.type.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 4;
                g.DrawString("remaining.", font, Brushes.Black, drawPoint);

                // GATHER RATE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 10;
                g.DrawString("Gather rate: " + resource.type.gatherSpeed, font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString("Gather rate: " + resource.type.gatherSpeed, font).Width) + 2;
                g.DrawImage(resourceImages[resource.type.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 2;
                g.DrawString("/s", font, Brushes.Black, drawPoint);
            }

            // Border
            g.DrawImage(rightAreaBorderLeft, new Rectangle(bounds.X-RIGHT_AREA_BORDER_WIDTH, 0, RIGHT_AREA_BORDER_WIDTH, Height));
        }

        /// <summary>
        /// Renders the top right panel; i.e. the part of the window which allows
        /// the user to either select a building to create or a technology to 
        /// research.
        /// </summary>
        private void RenderTopRightPanel(Graphics g, Rectangle bounds)
        {
            // Account for the border size
            bounds.X += RIGHT_AREA_BORDER_WIDTH;
            bounds.Width -= RIGHT_AREA_BORDER_WIDTH;

            // If the building panel is open, draw that
            if (buildingPanelOpen)
            {
                // Find out how many icons we can fit
                int iconWidth = (int)((bounds.Width - ICON_GAP) / (ICON_SIZE + ICON_GAP));

                int i = 0;
                foreach(BuildingType b in client.info.buildingTypes.Values)
                {
                    // If the building can't be built yet, it will be greyed out
                    bool grayed = !(client.info.technologies.ContainsKey(b.prerequisite) && client.info.technologies[b.prerequisite].researched);

                    Rectangle iconBounds = new Rectangle(bounds.X + ICON_GAP + (ICON_SIZE + ICON_GAP) * (i % iconWidth),
                        bounds.Y + ICON_GAP + (int)(i / iconWidth) * (ICON_SIZE + ICON_GAP),
                        ICON_SIZE, ICON_SIZE);

                    g.DrawImage(b.image, iconBounds);
                    
                    if (grayed)
                        g.FillRectangle(new SolidBrush(Color.FromArgb(OVERLAY_ALPHA, Color.LightGray)), iconBounds);

                    i++;
                }
                // Also draw a tooltip if the mouse is over a building
            }

            // Otherwise draw the technology panel
            else
            {

                // Also draw a tooltip if the mouse is over a technology
            }
        }

        /// <summary>
        /// Processes any key events on the game window. If they are recognised as
        /// utilised keys, performs the appropriate action on the client.
        /// </summary>
        public void KeyEvent(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if (key == Keys.Up)
                this.screenY -= SCREEN_Y_CHANGE;
            else if (key == Keys.Down)
                this.screenY += SCREEN_Y_CHANGE;
            else if (key == Keys.Right)
                this.screenX += SCREEN_X_CHANGE;
            else if (key == Keys.Left)
                this.screenX -= SCREEN_X_CHANGE;

            Refresh();
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
