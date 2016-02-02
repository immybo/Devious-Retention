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
        // How many tiles fit on the screen
        private double maxXTiles = 0;
        private double maxYTiles = 0;
        // How large tiles are in pixels
        private int tileWidth = 0;
        private int tileHeight = 0;

        // Where the mouse started dragging, for selection purposes
        private double startX = -1;
        private double startY = -1;

        // Where the mouse currently is
        private double mouseX = -1;
        private double mouseY = -1;

        // Whether or not the mouse left button is currently down
        private bool mouseDown = false;
        // Whether or not the mouse left button was previous pressed down on top of the game panel
        private bool mouseDownOnGameArea = false;

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
            MouseClick += new MouseEventHandler(MouseClickEvent);
            MouseDown += new MouseEventHandler(MouseDownEvent);
            MouseMove += new MouseEventHandler(MouseMoveEvent);
        }

        /// <summary>
        /// Returns the entity, if there is one, which is displayed at (x,y)
        /// on the the screen for the client. This means that, if there are
        /// overlapping entities, the one in front will be returned.
        /// </summary>
        /// <returns>The entity at the position, or null if there was none.</returns>
        public Entity GetEntityAt(double x, double y)
        {
            // Units > buildings > resources
            foreach (Entity e in client.units.Values)
                if (e.x + e.type.size > x && e.x < x && e.y + e.type.size > y && e.y < y)
                    return e;
            foreach (Entity e in client.buildings.Values)
                if (e.x + e.type.size > x && e.x < x && e.y + e.type.size > y && e.y < y)
                    return e;
            foreach (Entity e in client.resources.Values)
                if (e.x + e.type.size > x && e.x < x && e.y + e.type.size > y && e.y < y)
                    return e;

            return null;
        }

        /// <summary>
        /// Returns the entities, if there are any, which are contained within
        /// the given rectangle.
        /// </summary>
        public HashSet<Entity> GetEntitiesIn(Rectangle bounds)
        {
            HashSet<Entity> entities = new HashSet<Entity>();

            foreach (Entity e in client.units.Values)
                entities.Add(e);
            foreach (Entity e in client.buildings.Values)
                entities.Add(e);
            foreach (Entity e in client.resources.Values)
                entities.Add(e);
            
            HashSet<Entity> enclosedEntities = new HashSet<Entity>();
            foreach (Entity e in entities)
            {
                if(e.x+e.type.size > bounds.X
                 &&e.y+e.type.size > bounds.Y
                 &&e.x < bounds.X + bounds.Width
                 &&e.y < bounds.Y + bounds.Height)
                {
                    enclosedEntities.Add(e);
                }
            }
            return enclosedEntities;
        }

        /// <summary>
        /// Selects the appropriate entities within the given area.
        /// 
        /// Priority (lower priorities are only done if there are no entities
        /// fitting higher priorities):
        /// 1. Select all of the player's units within the area
        /// 2. Select all of the player's buildings within the area
        /// 3. Select all other players' units within the area
        /// 4. Select all other players' buildings within the area
        /// 5. Select all resources within the area
        /// </summary>
        private void SelectEntitiesInArea(Rectangle rect)
        {
            HashSet<Entity> entities = GetEntitiesIn(rect);
            if (entities.Count == 0) return;

            List<Unit> units = new List<Unit>();
            List<Building> buildings = new List<Building>();
            List<Resource> resources = new List<Resource>();
            foreach(Entity e in entities)
            {
                if (e is Unit) units.Add((Unit)e);
                else if(e is Building) buildings.Add((Building)e);
                else if (e is Resource) resources.Add((Resource)e);
            }

            List<Entity> entitiesToAdd = new List<Entity>();
            // 1. Select all of the player's units within the area
            if(units.Count != 0)
                foreach (Unit u in units)
                    if (u.playerNumber == client.playerNumber) entitiesToAdd.Add(u);
            // 2. Select all of the player's buildings within the area
            if (entitiesToAdd.Count == 0 && buildings.Count != 0)
                foreach (Building b in buildings)
                    if (b.playerNumber == client.playerNumber) entitiesToAdd.Add(b);
            // 3. Select all other players' units within the area
            if (entitiesToAdd.Count == 0 && units.Count != 0)
                foreach (Unit u in units)
                    entitiesToAdd.Add(u);
            // 4. Select all other players' buildings within the area
            if (entitiesToAdd.Count == 0 && buildings.Count != 0)
                foreach (Building b in buildings)
                    entitiesToAdd.Add(b);
            // 5. Select all resources within the area
            if (entitiesToAdd.Count == 0 && resources.Count != 0)
                foreach (Resource r in resources)
                    entitiesToAdd.Add(r);
            
            client.selected = entitiesToAdd;
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
            RenderMinimap(g,
                new Rectangle((int)((GAME_AREA_WIDTH - MINIMAP_WIDTH) * Width), (int)(GAME_AREA_HEIGHT * Height - MINIMAP_WIDTH * Width), (int)(MINIMAP_WIDTH * Width), (int)(MINIMAP_WIDTH * Width)));
            RenderResourceDisplayArea(g,
                new Rectangle(0, (int)(Height * GAME_AREA_HEIGHT), (int)(GAME_AREA_WIDTH * Width), (int)((1 - GAME_AREA_HEIGHT) * Height)));
            RenderSelectedEntityPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH*Width),(int)(TOP_RIGHT_HEIGHT* Height),(int)((1-GAME_AREA_WIDTH) * Width), (int)((1-TOP_RIGHT_HEIGHT)* Height)));
            RenderTopRightPanel(g,
                new Rectangle((int)(GAME_AREA_WIDTH * Width), 0, (int)((1 - GAME_AREA_WIDTH) * Width), (int)(TOP_RIGHT_HEIGHT * Height)));
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

            int entityLOS = e.type.lineOfSight;
            // Just round it down for simplicity
            int entityX = (int)(e.x + e.type.size / 2);
            int entityY = (int)(e.y + e.type.size / 2);

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
            int oldUnitX = (int)(unit.x + unit.unitType.size / 2 - dX);
            int oldUnitY = (int)(unit.y + unit.unitType.size / 2 - dY);

            for (int x = oldUnitX - unit.unitType.lineOfSight; x <= oldUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = oldUnitY - unit.unitType.lineOfSight; y <= oldUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(oldUnitX - x, 2) + Math.Pow(oldUnitY - y, 2)));
                    if (distance > unit.unitType.lineOfSight) continue;

                    // This is one of the tiles that the unit used to be able to see
                    oldTiles.Add(new Coordinate(x, y));
                }
            }

            // Figure out the new circle
            int newUnitX = (int)(unit.x + unit.unitType.size / 2);
            int newUnitY = (int)(unit.y + unit.unitType.size / 2);
            for (int x = newUnitX - unit.unitType.lineOfSight; x <= newUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = newUnitY - unit.unitType.lineOfSight; y <= newUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (x < 0 || y < 0) continue;
                    if (x >= client.map.width || y >= client.map.height) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(newUnitX - x, 2) + Math.Pow(newUnitY - y, 2)));
                    if (distance > unit.unitType.lineOfSight) continue;

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

            int entityLOS = entity.type.lineOfSight;
            int entityX = (int)(entity.x + entity.type.size / 2);
            int entityY = (int)(entity.y + entity.type.size / 2);
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
            foreach (Unit u in client.units.Values)
                if (u.playerNumber == client.playerNumber)
                    entities.Add(u);
                    
            foreach (Building b in client.buildings.Values)
                if (b.playerNumber == client.playerNumber)
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                // Distance between the entity and the tile
                double distance = Math.Sqrt(Math.Pow(e.x - c.x, 2) + Math.Pow(e.y - c.y, 2));
                if (distance <= e.type.lineOfSight) return true;
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
            foreach (Unit u in client.units.Values)
                if (u.playerNumber == client.playerNumber)
                    entities.Add(u);
            foreach (Building b in client.buildings.Values)
                if (b.playerNumber == client.playerNumber)
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                int entityLOS = e.type.lineOfSight;
                // Just round it down for simplicity
                int entityX = (int)(e.x+e.type.size/2);
                int entityY = (int)(e.y+e.type.size/2);

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
            tileWidth = (int)(bounds.Width / HORIZONTAL_TILES);
            tileHeight = tileWidth;

            // Figure out how much of the top and left tiles must be cut off the screen, due
            // to the camera position being potentially not an integer value
            int topTileXOffset = (int)((screenX - (int)screenX) * tileWidth);
            int topTileYOffset = (int)((screenY - (int)screenY) * tileHeight);

            // Figure out how many tiles we can draw on the screen
            maxXTiles = HORIZONTAL_TILES + 1;
            maxYTiles = (double)bounds.Height / tileHeight + 1; // better too many than too few since we draw over the edges anyway

            for (int i = 0; i < maxXTiles; i++)
            {
                if (i + (int)screenX >= client.map.width) continue;
                if (i + (int)screenX < 0) continue;

                for (int j = 0; j < maxYTiles; j++)
                {
                    if (j + (int)screenY >= client.map.height) continue;
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
            foreach (Resource r in client.resources.Values)
                entities.Add(r);
            foreach (Building b in client.buildings.Values)
                entities.Add(b);
            foreach (Unit u in client.units.Values)
                entities.Add(u);

            // Render them all
            foreach(Entity e in entities)
            {
                // First check if they're even on the screen
                if (e.x + e.type.size < screenX || e.x > screenX + maxXTiles) continue;
                if (e.y + e.type.size < screenY || e.y > screenY + maxYTiles) continue;
                // And check if we have line of sight to them
                if (!LOS[(int)(e.x + e.type.size / 2), (int)(e.y + e.type.size / 2)]) continue;

                // Since they are on the screen, figure out their bounds
                Rectangle entityBounds = new Rectangle();
                entityBounds.X = (int)((e.x - screenX) * tileWidth); // their distance from the left/top of the screen
                entityBounds.Y = (int)((e.y - screenY) * tileHeight);
                entityBounds.Width = (int)(e.type.size * tileWidth);
                entityBounds.Height = (int)(e.type.size * tileHeight);

                // And finally, draw them
                g.DrawImage(e.image, entityBounds);
            }

            // If the mouse has been dragged across an area and started on the game area, draw a rectangle around that area
            if (mouseDown && mouseDownOnGameArea)
            {
                int width = (int)Math.Abs(mouseX - startX);
                int height = (int)Math.Abs(mouseY - startY);
                int x1 = (int)(mouseX > startX ? startX : mouseX);
                int y1 = (int)(mouseY > startY ? startY : mouseY);
                g.DrawRectangle(Pens.Black, x1, y1, width, height);
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

            Font font = new Font(GameInfo.FONT_NAME, (int)(bounds.Height/1.5), FontStyle.Regular);

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
            foreach (Unit u in client.units.Values)
                entities.Add(u);
            foreach (Building b in client.buildings.Values)
                entities.Add(b);

            // Draw on top of the tile image
            foreach (Entity e in entities)
            {
                // Do nothing if we don't have line of sight there
                if (!LOS[(int)(e.x + e.type.size / 2), (int)(e.y + e.type.size / 2)]) continue;
                // Draw at most one tile worth of color, in the middle of the entity (may be important for large entities)
                tileImage.SetPixel((int)(e.x + e.type.size / 2), (int)(e.y + e.type.size / 2), GameInfo.PLAYER_COLORS[e.playerNumber]);
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

            // Draw a box around where the camera is
            double x1 = screenX / client.map.width;
            double y1 = screenY / client.map.height;
            double x2 = (screenX + maxXTiles) / client.map.width;
            double y2 = (screenY + maxYTiles) / client.map.height;

            // If one of the sides would be off the side, just set it to the side
            if (x1 < 0) x1 = 0;
            if (y1 < 0) y1 = 0;
            if (x2 > 1) x2 = 1;
            if (y2 > 1) y2 = 1;

            g.DrawRectangle(Pens.Black, (int)(x1*(bounds.Width - MINIMAP_BORDER_SIZE) + bounds.X+MINIMAP_BORDER_SIZE),
                                        (int)(y1*(bounds.Height - MINIMAP_BORDER_SIZE) + bounds.Y+MINIMAP_BORDER_SIZE),
                                        (int)((x2 - x1)*(bounds.Width - MINIMAP_BORDER_SIZE)),
                                        (int)((y2 - y1)*(bounds.Height - MINIMAP_BORDER_SIZE)));
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
            Font titleFont = new Font(GameInfo.TITLE_FONT_NAME, (int)(fontSize*1.5), FontStyle.Regular);
            Font font = new Font(GameInfo.FONT_NAME, fontSize, FontStyle.Regular);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            // Draw background image
            g.DrawImage(selectedEntityAreaBackgroundImage, bounds);

            // Border
            g.DrawImage(rightAreaBorderLeft, new Rectangle(bounds.X - RIGHT_AREA_BORDER_WIDTH, 0, RIGHT_AREA_BORDER_WIDTH, Height));

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
                g.DrawString(building.buildingType.name, titleFont, Brushes.Black, drawPoint, format);

                // Now draw everything else on a left alignment
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += (int)(fontSize*1.5) + 20;

                // HITPOINTS
                g.DrawString("HP: " + building.hitpoints + "/" + building.buildingType.hitpoints, font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // DAMAGE
                // Only draw the damage if this building can attack
                if (building.buildingType.aggressive)
                {
                    g.DrawString("Damage:", font, Brushes.Black, drawPoint);
                    // icon for the damage type
                    g.DrawImage(damageTypeIcons[building.buildingType.damageType],
                        new Rectangle(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 10,
                                      drawPoint.Y, (int)(DAMAGE_ICON_SIZE*bounds.Width),(int)(DAMAGE_ICON_SIZE*bounds.Width)));
                    g.DrawString(building.buildingType.damage + "", font, Brushes.Black,
                        new Point(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 20 + (int)(DAMAGE_ICON_SIZE * bounds.Width),drawPoint.Y));
                    drawPoint.Y += fontSize + 10;

                    // RANGE
                    g.DrawString("Range: " + building.buildingType.range, font, Brushes.Black, drawPoint);
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
                    g.DrawString(building.buildingType.resistances[i] + "%", font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString(building.buildingType.resistances[i] + "%", font).Width) + 10;
                }

                // RESOURCE GATHER RATE
                // Only draw the resource gather rate if this building is built on a resource it can gather, or passively grants resources
                if (building.buildingType.providesResource || (building.buildingType.canBeBuiltOnResource && building.resource != null))
                {
                    double gatherRate;
                    if (building.buildingType.providesResource) // passively provides resource
                        gatherRate = building.buildingType.gatherSpeed;
                    else // actively provides resource
                        gatherRate = building.resource.resourceType.gatherSpeed;

                    drawPoint.X = bounds.X + 10;
                    drawPoint.Y += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 20;
                    g.DrawString("Gathering " + gatherRate, font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString("Gathering " + gatherRate, font).Width) + 4;
                    g.DrawImage(resourceImages[building.buildingType.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 2;
                    g.DrawString("/s", font, Brushes.Black, drawPoint);
                }

                // TRAINING QUEUE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 20;

                g.DrawString("Training Queue", font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // max visible unit types
                int maxUnits = (int)((bounds.Width- ICON_GAP) / (ICON_SIZE+ICON_GAP));

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
                if (building.buildingType.trainableUnits.Length > 0)
                {
                    drawPoint.X = bounds.X + ICON_GAP;
                    drawPoint.Y += 20;

                    g.DrawString("Trainable Units", font, Brushes.Black, drawPoint);
                    drawPoint.Y += fontSize + 20;
                    int startOfTrainableUnitsY = drawPoint.Y - ICON_GAP;

                    int currentOnLine = 0; // how many icons are on the current line
                    List<UnitType> unitTypesList = new List<UnitType>();
                    foreach (string s in building.buildingType.trainableUnits)
                    {
                        if (client.info.unitTypes.ContainsKey(s))
                        {
                            UnitType u = client.info.unitTypes[s];
                            unitTypesList.Add(u);
                            g.DrawImage(u.icon,
                                new Rectangle(drawPoint.X, drawPoint.Y, ICON_SIZE, ICON_SIZE));

                            drawPoint.X += ICON_SIZE + ICON_GAP;
                            currentOnLine++;
                            if (currentOnLine >= maxUnits)
                            { // Go down to next line
                                drawPoint.Y += ICON_SIZE + ICON_GAP;
                                drawPoint.X = bounds.X + ICON_GAP;
                            }
                        }
                        // Do nothing if the unit type doesn't exist or can't be found
                    }

                    drawPoint.X = bounds.X + ICON_GAP;
                    // Also draw a tooltip if the mouse is over a trainable unit
                    if (GetArea(mouseX, mouseY).Equals("selected entity panel") && mouseY > startOfTrainableUnitsY)
                    {
                        // Now find if it's actually over an icon
                        double column = (double)(mouseX - bounds.X) / (ICON_SIZE + ICON_GAP);
                        double row = (double)(mouseY - startOfTrainableUnitsY) / (ICON_SIZE + ICON_GAP);

                        int num = (int)row * maxUnits + (int)column;
                        if (num < unitTypesList.Count)
                        {
                            // Now make sure it's aligned with an icon and not the gaps between them
                            double columnRemainder = column - (int)column;
                            double rowRemainder = row - (int)row;
                            if (columnRemainder > (double)ICON_GAP / (ICON_SIZE + ICON_GAP) && rowRemainder > (double)ICON_GAP / (ICON_SIZE + ICON_GAP))
                            {
                                // Draw it to the top left
                                int tooltipWidth = 300;
                                int tooltipHeight = 500;
                                DrawEntityTooltip(g, unitTypesList[num], new Rectangle((int)mouseX - tooltipWidth, (int)mouseY - tooltipHeight, tooltipWidth, tooltipHeight));
                            }
                        }
                    }
                }
            }
            else if (entity is Unit)
            {
                Unit unit = (Unit)entity;
                Point drawPoint = new Point(bounds.X + 10, bounds.Y + 20 + (int)(fontSize * 1.5));
                g.DrawString(unit.unitType.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width / 2, bounds.Y + 10), format);
                
                // HITPOINTS
                g.DrawString("HP: " + unit.hitpoints + "/" + unit.unitType.hitpoints, font, Brushes.Black, drawPoint);
                drawPoint.Y += fontSize + 10;

                // DAMAGE
                g.DrawString("Damage:", font, Brushes.Black, drawPoint);
                // icon for the damage type
                g.DrawImage(damageTypeIcons[unit.unitType.damageType],
                    new Rectangle(drawPoint.X + (int)g.MeasureString("Damage:", font).Width + 10,
                        drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    g.DrawString(unit.unitType.damage + "", font, Brushes.Black,
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
                    g.DrawString(unit.unitType.resistances[i] + "%", font, Brushes.Black, drawPoint);
                    drawPoint.X += (int)(g.MeasureString(unit.unitType.resistances[i] + "%", font).Width) + 10;
                }

                // RANGE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 10;
                g.DrawString("Range: " + unit.unitType.range, font, Brushes.Black, drawPoint);

                // SPEED
                drawPoint.Y += fontSize + 10;
                g.DrawString("Speed: " + unit.unitType.speed, font, Brushes.Black, drawPoint);
            }
            else if (entity is Resource)
            {
                Resource resource = (Resource)entity;
                Point drawPoint = new Point(bounds.X + 10, bounds.Y + 20 + (int)(fontSize * 1.5));
                g.DrawString(resource.resourceType.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width / 2, bounds.Y + 10), format);

                // REMAINING RESOURCE
                g.DrawString(resource.amount+"/"+resource.resourceType.resourceAmount, font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString(resource.amount+"/"+resource.resourceType.resourceAmount, font).Width) + 4;
                g.DrawImage(resourceImages[resource.resourceType.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 4;
                g.DrawString("remaining.", font, Brushes.Black, drawPoint);

                // GATHER RATE
                drawPoint.X = bounds.X + 10;
                drawPoint.Y += fontSize + 10;
                g.DrawString("Gather rate: " + resource.resourceType.gatherSpeed, font, Brushes.Black, drawPoint);
                drawPoint.X += (int)(g.MeasureString("Gather rate: " + resource.resourceType.gatherSpeed, font).Width) + 2;
                g.DrawImage(resourceImages[resource.resourceType.resourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 2;
                g.DrawString("/s", font, Brushes.Black, drawPoint);
            }
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
                int numIconsPerRow = (int)((bounds.Width - ICON_GAP) / (ICON_SIZE + ICON_GAP));

                int i = 0;

                // We need a list to draw tooltips
                List<BuildingType> buildingTypeList = new List<BuildingType>();
                foreach(BuildingType b in client.info.buildingTypes.Values)
                {
                    // If the building can't be built yet, it will be greyed out
                    bool grayed = !(client.info.technologies.ContainsKey(b.prerequisite) && client.info.technologies[b.prerequisite].researched);

                    Rectangle iconBounds = new Rectangle(bounds.X + ICON_GAP + (ICON_SIZE + ICON_GAP) * (i % numIconsPerRow),
                        bounds.Y + ICON_GAP + (int)(i / numIconsPerRow) * (ICON_SIZE + ICON_GAP),
                        ICON_SIZE, ICON_SIZE);

                    g.DrawImage(b.image, iconBounds);
                    
                    if (grayed)
                        g.FillRectangle(new SolidBrush(Color.FromArgb(OVERLAY_ALPHA, Color.LightGray)), iconBounds);

                    i++;

                    buildingTypeList.Add(b);
                }
                // Also draw a tooltip if the mouse is over a building
                if(GetArea(mouseX, mouseY).Equals("top right panel")) // In the right screen area
                {
                    // Now find if it's actually over an icon
                    double column = (double)(mouseX - bounds.X) / (ICON_SIZE + ICON_GAP);
                    double row = (double)(mouseY - bounds.Y) / (ICON_SIZE + ICON_GAP);

                    int num = (int)row * numIconsPerRow + (int)column;
                    if (num < client.info.buildingTypes.Values.Count)
                    {
                        // Now make sure it's aligned with an icon and not the gaps between them
                        double columnRemainder = column - (int)column;
                        double rowRemainder = row - (int)row;
                        if(columnRemainder > (double)ICON_GAP / (ICON_SIZE+ ICON_GAP) && rowRemainder > (double)ICON_GAP / (ICON_SIZE + ICON_GAP))
                        {
                            // Draw it to the bottom left of the mouse cursor
                            int tooltipWidth = 300;
                            int tooltipHeight = 500;
                            DrawEntityTooltip(g, buildingTypeList[num], new Rectangle((int)mouseX - tooltipWidth, (int)mouseY, tooltipWidth, tooltipHeight));
                        }
                    }
                }
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
        }

        /// <summary>
        /// Processes any mouse clicking events on the game window.
        /// Mostly figures out the area where the click was and calls the appropriate
        /// method in the client.
        /// </summary>
        private void MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // If it's on the game area and there's a selected unit, move that selected unit
                if(GetArea(e.X, e.Y).Equals("game area") && client.selected.Count > 0)
                {
                    double tileX = screenX + (double)e.X / tileWidth;
                    double tileY = screenY + (double)e.Y / tileHeight;
                    client.MoveUnits(tileX, tileY);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                mouseDown = false;
                // A mouse click on the minimap
                if (GetArea(e.X, e.Y).Equals("minimap") && !mouseDownOnGameArea)
                    ScrollToMinimapPoint(e.X, e.Y);

                // A mouse click on the game area (not the minimap as this has already been checked)
                else if (GetArea(e.X, e.Y).Equals("game area") && mouseDownOnGameArea)
                {
                    // If the mouse was dragged across a sizeable area, treat it as a drag
                    if (Math.Abs(e.X - startX) > 20 || Math.Abs(e.Y - startY) > 20)
                    {
                        Rectangle entitySelectBounds = new Rectangle((int)(e.X > startX ? startX : e.X), (int)(e.Y > startY ? startY : e.Y),
                                                                     (int)Math.Abs(e.X - startX), (int)Math.Abs(e.Y - startY));
                        entitySelectBounds.X /= tileWidth; entitySelectBounds.Y /= tileHeight;
                        entitySelectBounds.Width /= tileWidth; entitySelectBounds.Y /= tileHeight;
                        SelectEntitiesInArea(entitySelectBounds);
                    }
                    // Otherwise treat it as a click
                    else
                    {
                        Entity entity = GetEntityAt((double)e.X / tileWidth, (double)e.Y / tileHeight);
                        client.selected.Clear();
                        if (entity != null) client.selected.Add(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Processes any mouse down events on the game window.
        /// This is used to determine the start of an area selection.
        /// </summary>
        private void MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            startX = e.X;
            startY = e.Y;
            mouseDown = true;
            mouseDownOnGameArea = GetArea(e.X, e.Y).Equals("game area");
        }

        /// <summary>
        /// Processes any mouse movement events on the game window.
        /// </summary>
        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            // If we're dragging the mouse in the minimap area, set the view appropriately
            if (mouseDown && !mouseDownOnGameArea && GetArea(e.X, e.Y).Equals("minimap"))
                ScrollToMinimapPoint(e.X, e.Y);
        }

        /// <summary>
        /// Returns the area of the screen the given coordinate is in.
        /// 
        /// Possible returns:
        /// "game area"
        /// "minimap"
        /// "top right panel"
        /// "selected entity panel"
        /// "resource display area"
        /// 
        /// Will give an incorrect return if an invalid coordinate is given.
        /// </summary>
        private string GetArea(double x, double y)
        {
            // On the left of the screen
            if(x < Width*GAME_AREA_WIDTH)
            {
                // In the game area
                if (y < Height * GAME_AREA_HEIGHT)
                {
                    // In the minimap area
                    if (x > Width * (GAME_AREA_WIDTH - MINIMAP_WIDTH) && y > Height * GAME_AREA_HEIGHT - Width * MINIMAP_WIDTH)
                        return "minimap";
                    // Not in the minimap area
                    else
                        return "game area";
                }
                // Otherwise on the left, it has to be in the resource display area
                else
                    return "resource display area";
            }
            // On the right of the screen
            else
            {
                // Top = top right panel
                if (y < Height * TOP_RIGHT_HEIGHT)
                    return "top right panel";
                // Otherwise selected entity panel
                else
                    return "selected entity panel";
            }
        }

        /// <summary>
        /// Assuming that (x,y) lies on the minimap,
        /// moves the camera to the appropriate area.
        /// </summary>
        private void ScrollToMinimapPoint(double x, double y)
        {
            double minimapX = x - Width * (GAME_AREA_WIDTH - MINIMAP_WIDTH);
            double minimapY = y - (Height * GAME_AREA_HEIGHT - Width * MINIMAP_WIDTH);
            double minimapSize = Width * MINIMAP_WIDTH;

            // How far across (0-1) the point is
            double proportionX = minimapX / minimapSize;
            double proportionY = minimapY / minimapSize;

            // The tile of the map we should therefore center on is..
            screenX = client.map.width * proportionX - maxXTiles / 2;
            screenY = client.map.height * proportionY - maxYTiles / 2;
        }

        /// <summary>
        /// Draws a tooltip for a specific entity type at the specified location.
        /// </summary>
        private void DrawEntityTooltip(Graphics g, EntityType type, Rectangle bounds)
        {
            // First draw the background and the border around it
            g.FillRectangle(Brushes.White, bounds);
            g.DrawRectangle(new Pen(Brushes.Black), bounds);
            g.SetClip(bounds);

            Font titleFont = new Font(GameInfo.TITLE_FONT_NAME, 28, FontStyle.Regular);
            Font font = new Font(GameInfo.FONT_NAME, 20, FontStyle.Regular);
            Font miniFont = new Font(GameInfo.FONT_NAME, 14, FontStyle.Regular);
            StringFormat centerFormat = new StringFormat();
            centerFormat.Alignment = StringAlignment.Center;
            StringFormat vertCenterFormat = new StringFormat();
            vertCenterFormat.LineAlignment = StringAlignment.Center;

            Point pos = new Point(bounds.X + ICON_GAP, bounds.Y + ICON_GAP);
            // Name
            g.DrawString(type.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width/2, pos.Y), centerFormat);
            pos.Y += (int)(titleFont.Size) + 20;
            // Resource costs
            double iconGapRatio = (double)ICON_SIZE / ICON_GAP;
            int resourceGapSize = (int)(bounds.Width / (1 + (1 + iconGapRatio) * GameInfo.RESOURCE_TYPES));
            int resourceIconSize = (int)(resourceGapSize * iconGapRatio);

            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
            {
                if (i == GameInfo.RESOURCE_TYPES / 2) { pos.Y += ICON_SIZE + 10; pos.X = bounds.X + ICON_GAP; }

                g.DrawImage(resourceImages[i], pos.X, pos.Y, ICON_SIZE, ICON_SIZE);
                pos.X += ICON_SIZE + 10;
                g.DrawString(type.resourceCosts[i]+"", font, Brushes.Black, new Point(pos.X, pos.Y + ICON_SIZE/2), vertCenterFormat);
                pos.X += (int)g.MeasureString("9999",font).Width + 10;
            }
            pos.X = bounds.X + ICON_GAP;
            pos.Y += ICON_SIZE + 10;

            // Hitpoints and, if applicable, damage
            g.DrawString("Hitpoints: " + type.hitpoints, font, Brushes.Black, pos);
            pos.Y += (int)(font.Size) + 20;
            if (type is UnitType || type.aggressive)
            {
                g.DrawString("Damage: " + type.damage, font, Brushes.Black, new Point(pos.X, pos.Y + ICON_SIZE/2), vertCenterFormat);
                pos.X += (int)g.MeasureString("Damage: " + type.damage, font).Width + 10;
                g.DrawImage(damageTypeIcons[type.damageType], pos.X, pos.Y, ICON_SIZE, ICON_SIZE);
                pos.X = bounds.X + ICON_GAP;
                pos.Y += ICON_SIZE + 10;
            }

            // Description (wrap it in the box)
            g.DrawString(type.description, miniFont, Brushes.Black, new Rectangle(bounds.X + 10, pos.Y, bounds.Width - 20, bounds.Height - (pos.Y-bounds.Y) - 10));

            g.ResetClip();
        }

        /// <summary>
        /// Draws a tooltip for the specified technology at the specified location.
        /// </summary>
        private void DrawTechnologyTooltip(Graphics g, Technology technology, Rectangle bounds)
        {
        }
    }
}
