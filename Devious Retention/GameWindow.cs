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
        // TODO Better graphics/sprites.
        private World world;
        private LocalPlayer player;

        private GameClient client; // TODO remove client from gamewindow

        // 1 = entire screen width/height, 0 = nothing
        private const double GAME_AREA_WIDTH = 0.75;
        private const double GAME_AREA_HEIGHT = 0.90;
        private const double MINIMAP_WIDTH = 0.15;
        private const double TOP_RIGHT_HEIGHT = 0.5;

        private const int HORIZONTAL_TILES = 10;

        // How much the screen moves every tick of holding the button down, in tiles
        private const double SCREEN_X_CHANGE = 1;
        private const double SCREEN_Y_CHANGE = 1;

        // How large most icons are, and the gaps between them (pixels)
        private const int ICON_SIZE = 50;
        private const int ICON_GAP = 20;

        // How wide each resource is in the resource display area, out of 1
        private const double RESOURCE_WIDTH = 0.2;

        // How large tooltips are (pixels)
        private const int TOOLTIP_WIDTH = 300;
        private const int TOOLTIP_HEIGHT = 500;

        // How light greyed out things are (0=completely gray,1=normal)
        private const double OVERLAY_STRENGTH = 0.5;

        // relative to the width of the selection panel
        private const double DAMAGE_ICON_SIZE = 0.06;
        private const double TRAINING_QUEUE_ICON_SIZE = 0.19;
        // pixels
        private const int MINIMAP_BORDER_SIZE = 20;
        private const int RIGHT_AREA_BORDER_WIDTH = 20;
        private const int RESOURCE_AREA_BORDER_WIDTH = 20;

        // Where the top-left of the screen is, in map co-ordinates.
        public double worldX { get; private set; } = 0;
        public double worldY { get; private set; } = 0;
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

        // Whether or not the shift key is currently down
        private bool shiftKeyDown = false;

        // Whether or not the mouse left button is currently down
        private bool mouseDown = false;
        // Whether or not the mouse left button was previous pressed down on top of the game panel
        private bool mouseDownOnGameArea = false;
        // " top right panel
        private bool mouseDownOnTopRightArea = false;

        // How far vertically the top right panel has been shifted
        private int topRightShift = 0;

        // The building (if there is one) that is currently on the mouse cursor to be placed
        private BuildingType placingBuilding = null;

        // Whether the building panel or the technology panel is open
        private bool buildingPanelOpen = true;
        // Areas of icons in the currently open top right panel
        private Dictionary<Rectangle, string> iconBounds;
        // Areas of icons for training units in the selected entity panel
        // (irrelevant if client.selected[0] isn't a building)
        private Dictionary<Rectangle, UnitType> unitTrainingIconBounds;

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

        public GameWindow(World world, LocalPlayer player, GameClient client)
        {
            this.world = world;
            this.client = client;
            this.player = player;

            InitializeComponent();
            LoadLOS();

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

            // Set all the events
            Paint += Render;
            KeyDown += new KeyEventHandler(KeyEvent);
            KeyUp += new KeyEventHandler(KeyUpEvent);
            MouseClick += new MouseEventHandler(MouseClickEvent);
            MouseDown += new MouseEventHandler(MouseDownEvent);
            MouseMove += new MouseEventHandler(MouseMoveEvent);
        }

        public void SetMap(Map newMap)
        {
            world.SetMap(newMap);
            LoadLOS();
        }

        /// <summary>
        /// Selects the appropriate entities within the given area.
        /// This can only select one type of entity for one player. For example,
        /// if there is a unit belonging to player 1 and a building belonging to 
        /// player 1, it will select only the unit.
        /// 
        /// Priority (lower priorities are only done if there are no entities
        /// fitting higher priorities):
        /// 1. Select all of the player's units within the area
        /// 2. Select all of the player's buildings within the area
        /// 3. Select all other players' units within the area
        /// 4. Select all other players' buildings within the area
        /// 5. Select all resources within the area
        /// </summary>
        private void SelectEntitiesInArea(double x1, double y1, double width, double height)
        {
            // TODO Possibly use a quad tree for optimising selecting entities
            Entity[] entities = world.GetEntitiesIn(x1, y1, width, height);
            if (entities.Length == 0) return;

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
                    if (player.Owns(u)) entitiesToAdd.Add(u);
            // 2. Select all of the player's buildings within the area
            if (entitiesToAdd.Count == 0 && buildings.Count != 0)
                foreach (Building b in buildings)
                    if (player.Owns(b)) entitiesToAdd.Add(b);
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
        /// Renders everything; calls lesser rendering methods.
        /// </summary>
        private void Render(object sender, PaintEventArgs e)
        {
            // TODO Renderable interface, so render can be called on a large list of things
            Graphics g = e.Graphics;

            if (world.OutOfBounds(0, 0)) return; // nothing to render

            ResizeToFit();
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
        /// Updates the player's line of sight given that the
        /// given entity was just created.
        /// Assumes that this entity belongs to the player.
        /// Does nothing if this entity is a resource.
        /// </summary>
        public void UpdateLOSAdd(Entity e)
        {
            // TODO FIX LOS....
            // TODO Optimise LOS calculations
            // TODO move LOS to world

            // Resources don't have LOS
            if (e is Resource) return;

            int entityLOS = e.Type.lineOfSight;
            // Just round it down for simplicity
            int entityX = (int)(e.X + e.Type.size / 2);
            int entityY = (int)(e.Y + e.Type.size / 2);

            // Simple way of figuring out a circle
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    // Are we even on the map?
                    if (world.OutOfBounds(x, y)) continue;

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
        /// </summary>
        public void UpdateLOSMove(Unit unit, double dX, double dY)
        {
            if (!player.Owns(unit)) return;

            // The new LOS of the unit
            List<Coordinate> newTiles = new List<Coordinate>();
            // The old LOS of the unit
            List<Coordinate> oldTiles = new List<Coordinate>();

            // Figure out the old circle
            int oldUnitX = (int)(unit.X + unit.unitType.size / 2 - dX);
            int oldUnitY = (int)(unit.Y + unit.unitType.size / 2 - dY);

            for (int x = oldUnitX - unit.unitType.lineOfSight; x <= oldUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = oldUnitY - unit.unitType.lineOfSight; y <= oldUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
                    int distance = (int)(Math.Sqrt(Math.Pow(oldUnitX - x, 2) + Math.Pow(oldUnitY - y, 2)));
                    if (distance > unit.unitType.lineOfSight) continue;

                    // This is one of the tiles that the unit used to be able to see
                    oldTiles.Add(new Coordinate(x, y));
                }
            }

            // Figure out the new circle
            int newUnitX = (int)(unit.X + unit.unitType.size / 2);
            int newUnitY = (int)(unit.Y + unit.unitType.size / 2);
            for (int x = newUnitX - unit.unitType.lineOfSight; x <= newUnitX + unit.unitType.lineOfSight; x++)
            {
                for (int y = newUnitY - unit.unitType.lineOfSight; y <= newUnitY + unit.unitType.lineOfSight; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
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
                if (world.OutOfBounds(c)) continue;
                LOS[c.x, c.y] = true;
            }

            // And check if we can still see the old tiles
            foreach(Coordinate c in nowInvisibleTiles)
                LOS[c.x,c.y] = HasLOSTo(c);
        }

        /// <summary>
        /// Updates the player's line of sight given that the
        /// given entity was just deleted.
        /// </summary>
        public void UpdateLOSDelete(Entity entity)
        {
            if (entity is Resource) return;
            if (!player.Owns(entity)) return;

            int entityLOS = entity.Type.lineOfSight;
            int entityX = (int)(entity.X + entity.Type.size / 2);
            int entityY = (int)(entity.Y + entity.Type.size / 2);
            // Go through all the tiles the entity could see and recheck if we can still see them
            for (int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
            {
                for (int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                {
                    if (world.OutOfBounds(x, y)) continue;
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
            foreach (Unit u in world.GetUnits())
                if (player.Owns(u))
                    entities.Add(u);
                    
            foreach (Building b in world.GetBuildings())
                if (player.Owns(b))
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                // Distance between the entity and the tile
                double distance = Math.Sqrt(Math.Pow(e.X - c.x, 2) + Math.Pow(e.Y - c.y, 2));
                if (distance <= e.Type.lineOfSight) return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to figure out the player's line of sight, and stores it in "LOS"
        /// </summary>
        private void LoadLOS()
        {
            // Clear the current LOS
            LOS = new bool[world.MapSize().x, world.MapSize().y];

            List<Entity> entities = new List<Entity>();
            foreach (Unit u in world.GetUnits())
                if (player.Owns(u))
                    entities.Add(u);
            foreach (Building b in world.GetBuildings())
                if (player.Owns(b))
                    entities.Add(b);

            foreach(Entity e in entities)
            {
                int entityLOS = e.Type.lineOfSight;
                // Just round it down for simplicity
                int entityX = (int)(e.X+e.Type.size/2);
                int entityY = (int)(e.Y+e.Type.size/2);

                // Simple way of figuring out a circle
                for(int x = entityX - entityLOS; x <= entityX + entityLOS; x++)
                {
                    for(int y = entityY - entityLOS; y <= entityY + entityLOS; y++)
                    {
                        // Are we even on the map?
                        if (x < 0 || y < 0) continue;
                        if (x >= world.MapSize().x || y >= world.MapSize().y) continue;

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
            int topTileXOffset = (int)((worldX - (int)worldX) * tileWidth);
            int topTileYOffset = (int)((worldY - (int)worldY) * tileHeight);

            // Figure out how many tiles we can draw on the screen
            maxXTiles = HORIZONTAL_TILES + 1;
            maxYTiles = (double)bounds.Height / tileHeight + 1; // better too many than too few since we draw over the edges anyway

            for (int i = 0; i < maxXTiles; i++)
            {
                if (i + (int)worldX >= world.MapSize().x) continue;
                if (i + (int)worldX < 0) continue;

                for (int j = 0; j < maxYTiles; j++)
                {
                    if (j + (int)worldY >= world.MapSize().y) continue;
                    if (j + (int)worldY < 0) continue;

                    // We allow tiles to go slightly off the side, under the assumption that the GUI will be painted in front of them
                    // We draw tiles from the floor value of the screen position, and then position them off the screen so that the appropriate amount is displayed
                    g.DrawImage(world.GetTile(i + (int)worldX, j + (int)worldY).image, new Rectangle(i * tileWidth - topTileXOffset, j * tileHeight - topTileYOffset, tileWidth, tileHeight));
                    // If this tile is out of line of sight, draw a light grey overlay (grey it out)
                    if (!LOS[i + (int)worldX, j + (int)worldY])
                        g.FillRectangle(new SolidBrush(Color.FromArgb((int)(255*(1-OVERLAY_STRENGTH)), Color.LightGray)), new Rectangle(i * tileWidth - topTileXOffset, j * tileHeight - topTileYOffset, tileWidth, tileHeight));
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
            foreach (Resource r in world.GetResources())
                entities.Add(r);
            foreach (Building b in world.GetBuildings())
                entities.Add(b);
            foreach (Unit u in world.GetUnits())
                entities.Add(u);

            // Render them all
            foreach(Entity e in entities)
            {
                // Render a laser if they're attacking anything and it's their frame to attack
                if (e is Unit && client.attackingUnits.Contains((Unit)e) && ((Unit)e).attackTick == ((Unit)e).unitType.attackTicks)
                {
                    double x = (e.X + e.Type.size / 2 - worldX) * tileWidth;
                    double y = (e.Y + e.Type.size / 2 - worldY) * tileHeight;
                    Entity entityToAttack = ((Unit)e).entityToAttack;
                    double x2 = (entityToAttack.X + entityToAttack.Type.size / 2 - worldX) * tileWidth;
                    double y2 = (entityToAttack.Y + entityToAttack.Type.size / 2 - worldY) * tileHeight;

                    double xDiff = (x2 - x) / tileWidth;
                    double yDiff = (y2 - y) / tileHeight;

                    // Only draw if at least one part is on the screen and if it's within range
                    if (!(x < 0 && x2 < 0) && !(y < 0 && y2 < 0) && !(y > maxYTiles * tileHeight && y2 > maxYTiles * tileHeight) && !(x > maxXTiles * tileWidth && x2 > maxXTiles * tileWidth))
                        if(e.Type.range >= Math.Sqrt(xDiff*xDiff + yDiff* yDiff))
                            g.DrawLine(e.Player.Pen, (int)x, (int)y, (int)x2, (int)y2);
                }

                // First check if they're even on the screen
                if (e.X + e.Type.size < worldX || e.X > worldX + maxXTiles) continue;
                if (e.Y + e.Type.size < worldY || e.Y > worldY + maxYTiles) continue;
                // And check if we have line of sight to them
                if (!LOS[(int)(e.X + e.Type.size / 2), (int)(e.Y + e.Type.size / 2)]) continue;

                // Since they are on the screen, figure out their bounds
                Rectangle entityBounds = new Rectangle();
                entityBounds.X = (int)((e.X - worldX) * tileWidth); // their distance from the left/top of the screen
                entityBounds.Y = (int)((e.Y - worldY) * tileHeight);
                entityBounds.Width = (int)(e.Type.size * tileWidth);
                entityBounds.Height = (int)(e.Type.size * tileHeight);

                e.Render(g, entityBounds);
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

            // Draw the background
            g.DrawImage(resourceDisplayAreaBackgroundImage, bounds);

            Font font = new Font(GameInfo.FONT_NAME, (int)(bounds.Height/1.5), FontStyle.Regular);

            int resourcePadding = 5; // Padding between the icons and text/sides
            int resourceIconWidth = bounds.Height - resourcePadding * 2; // As large as they can be in the area provided while still being square
            int resourceTextWidth = (int)(RESOURCE_WIDTH*bounds.Width - resourceIconWidth); // Take up the rest of the space
            int resourceGapWidth = (int)((1 - GameInfo.RESOURCE_TYPES * RESOURCE_WIDTH) / (GameInfo.RESOURCE_TYPES+1) * bounds.Width); // Gap between adjacent resources

            // Draw the image icons using these dimensions
            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
            {
                Rectangle imageBounds = new Rectangle();
                imageBounds.X = (int)(resourceGapWidth * (i + 1) + (resourceIconWidth+ resourceTextWidth) * i + bounds.X);
                imageBounds.Y = bounds.Y + resourcePadding;
                imageBounds.Width = resourceIconWidth;
                imageBounds.Height = resourceIconWidth;

                Point textPoint = new Point(imageBounds.X + resourceIconWidth + resourcePadding, bounds.Y);

                g.DrawImage(resourceImages[i], imageBounds);
                g.DrawString((int)player.GetResource(i) + "", font, Brushes.Black, textPoint);
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
            double pixelsPerSquareX = (double)bounds.Width / world.MapSize().x;
            double pixelsPerSquareY = (double)bounds.Height / world.MapSize().y;

            // Draw the tiles first
            Bitmap tileImage = new Bitmap(world.MapSize().x, world.MapSize().y);

            // Set the appropriate colours for tiles
            for (int i = 0; i < world.MapSize().x; i++)
                for (int j = 0; j < world.MapSize().y; j++)
                    tileImage.SetPixel(i, j, world.GetTile(i, j).color);

            // Draw the entities next
            List<Entity> entities = new List<Entity>();
            foreach (Unit u in world.GetUnits())
                entities.Add(u);
            foreach (Building b in world.GetBuildings())
                entities.Add(b);

            // Draw on top of the tile image
            foreach (Entity e in entities)
            {
                // Do nothing if we don't have line of sight there
                if (!LOS[(int)(e.X + e.Type.size / 2), (int)(e.Y + e.Type.size / 2)]) continue;
                // Draw at most one tile worth of color, in the middle of the entity (may be important for large entities)
                tileImage.SetPixel((int)(e.X + e.Type.size / 2), (int)(e.Y + e.Type.size / 2), e.Player.Color); // TODO use actual player color not gotten from gameinfo
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
            double x1 = worldX / world.MapSize().x;
            double y1 = worldY / world.MapSize().y;
            double x2 = (worldX + maxXTiles) / world.MapSize().x;
            double y2 = (worldY + maxYTiles) / world.MapSize().y;

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
                    g.DrawImage(building.buildingType.providesResource ? resourceImages[building.buildingType.resourceType] : resourceImages[building.buildingType.builtOnResourceType], new Rectangle(drawPoint.X, drawPoint.Y, (int)(DAMAGE_ICON_SIZE * bounds.Width), (int)(DAMAGE_ICON_SIZE * bounds.Width)));
                    drawPoint.X += (int)(DAMAGE_ICON_SIZE * bounds.Width) + 2;
                    g.DrawString("/s", font, Brushes.Black, drawPoint);

                    if(building.buildingType.canBeBuiltOnResource && building.resource != null)
                    {
                        drawPoint.X = bounds.X + 10;
                        drawPoint.Y += fontSize + 10;
                        g.DrawString(building.resource.amount + " remaining in resource.", font, Brushes.Black, drawPoint);
                    }
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

                        Rectangle rect = new Rectangle(drawPoint.X, drawPoint.Y, (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width), (int)(TRAINING_QUEUE_ICON_SIZE * bounds.Width));
                        g.DrawImage(u.icon, rect);

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
                unitTrainingIconBounds = new Dictionary<Rectangle, UnitType>();
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
                            Rectangle rect = new Rectangle(drawPoint.X, drawPoint.Y, ICON_SIZE, ICON_SIZE);
                            unitTrainingIconBounds.Add(rect, u);
                            g.DrawImage(u.icon, rect);

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
            g.SetClip(bounds);

            int fontSize = bounds.Width / 25;

            // Account for the border size
            bounds.X += RIGHT_AREA_BORDER_WIDTH;
            bounds.Width -= RIGHT_AREA_BORDER_WIDTH;

            bounds.Y += 40;
            bounds.Height -= 40;

            // If the building panel is open, draw that
            if (buildingPanelOpen)
            {
                RenderBuildingPanel(g, bounds);
            }

            // Otherwise draw the technology panel
            else
            {
                RenderTechnologyPanel(g, bounds);
            }

            // Draw the switch button
            g.FillRectangle(Brushes.Azure, bounds.X, bounds.Y-40, bounds.Width, 40);
            g.DrawRectangle(Pens.Black, bounds.X, bounds.Y-40, bounds.Width, 40);
            StringFormat centerFormat = new StringFormat();
            centerFormat.Alignment = StringAlignment.Center;
            string drawString;
            if (buildingPanelOpen)
                drawString = "Open Technology Panel";
            else
                drawString = "Open Building Panel";
            g.DrawString(drawString, new Font(GameInfo.FONT_NAME, fontSize), Brushes.Black, new Point(bounds.X + bounds.Width / 2, bounds.Y -35), centerFormat);

            // If there's a building on the cursor, render that at the mouse position
            if(placingBuilding != null)
            {
                RenderPlacingBuilding(g);
            }
        }

        /// <summary>
        /// Renders the technology panel. Should only be called when the technology panel is open.
        /// </summary>
        private void RenderTechnologyPanel(Graphics g, Rectangle bounds)
        {
            iconBounds = new Dictionary<Rectangle, string>();
            int numIconsPerRow = (int)((bounds.Width - ICON_GAP) / (ICON_SIZE + ICON_GAP));
            int i = 0;

            List<Technology> techList = new List<Technology>();
            List<Technology> grayedTechList = new List<Technology>();
            // Figure out if technologies are greyed out or not
            foreach (Technology t in client.info.technologies.Values)
            {
                if (t.researched) continue; // don't draw it if it's researched
                                            // Don't draw it if a clashing technology has been researched
                foreach (string s in t.clashing)
                {
                    Technology clashingTech = client.info.technologies[s];
                    if (clashingTech.researched)
                    {
                        goto end;
                    }
                }

                foreach (string s in t.prerequisites)
                {
                    if (!client.info.technologies.ContainsKey(s) || !client.info.technologies[s].researched)
                    {
                        grayedTechList.Add(t);
                        goto end;
                    }
                }

                techList.Add(t);
            end:;
            }

            // Then draw the non-greyed ones first
            List<Technology> orderedTechs = new List<Technology>();
            foreach (Technology t in techList)
                orderedTechs.Add(t);
            foreach (Technology t in grayedTechList)
                orderedTechs.Add(t);

            foreach (Technology t in orderedTechs)
            {
                Rectangle iconBounds = new Rectangle(bounds.X + ICON_GAP + (ICON_SIZE + ICON_GAP) * (i % numIconsPerRow),
                    bounds.Y + ICON_GAP + topRightShift + (int)(i / numIconsPerRow) * (ICON_SIZE + ICON_GAP),
                    ICON_SIZE, ICON_SIZE);
                if (iconBounds.Y + ICON_SIZE < 0) continue;
                if (iconBounds.Y > bounds.Y + bounds.Height) continue;

                this.iconBounds.Add(iconBounds, t.name);
                g.DrawImage(t.icon, iconBounds);
                if (grayedTechList.Contains(t))
                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(255 * (1 - OVERLAY_STRENGTH)), Color.LightGray)), iconBounds);
                i++;
            }

            // Also draw a tooltip if the mouse is over a technology
            g.ResetClip();
            if (GetArea(mouseX, mouseY).Equals("top right panel")) // In the right screen area
            {
                // Find if it's actually over an icon
                foreach (KeyValuePair<Rectangle, string> pair in iconBounds)
                {
                    // If it's on the icon
                    if (mouseX >= pair.Key.X && mouseX <= pair.Key.X + pair.Key.Width && mouseY >= pair.Key.Y && mouseY <= pair.Key.Y + pair.Key.Height)
                    {
                        // Draw the appropriate tooltip
                        DrawTechnologyTooltip(g, client.info.technologies[pair.Value], new Rectangle((int)mouseX - TOOLTIP_WIDTH, (int)mouseY, TOOLTIP_WIDTH, TOOLTIP_HEIGHT));
                        break;
                    }
                }
            }

            if (placingBuilding != null)
                RenderPlacingBuilding(g);
        }

        /// <summary>
        /// Renders the building panel. Should only be called when the building panel is open.
        /// </summary>
        private void RenderBuildingPanel(Graphics g, Rectangle bounds)
        {
            iconBounds = new Dictionary<Rectangle, string>();
            int numIconsPerRow = (int)((bounds.Width - ICON_GAP) / (ICON_SIZE + ICON_GAP));
            int i = 0;

            List<BuildingType> buildingList = new List<BuildingType>();
            List<BuildingType> grayedBuildingList = new List<BuildingType>();

            // Figure out if buildings are greyed out or not
            foreach (BuildingType b in client.info.buildingTypes.Values)
            {
                if (client.CanBuild(b))
                    buildingList.Add(b);
                else
                    grayedBuildingList.Add(b);
            }

            // Then draw the non-greyed ones first
            List<BuildingType> orderedBuildings = new List<BuildingType>();
            foreach (BuildingType b in buildingList)
                orderedBuildings.Add(b);
            foreach (BuildingType b in grayedBuildingList)
                orderedBuildings.Add(b);

            foreach (BuildingType b in orderedBuildings)
            {
                Rectangle iconBounds = new Rectangle(bounds.X + ICON_GAP + (ICON_SIZE + ICON_GAP) * (i % numIconsPerRow),
                    bounds.Y + ICON_GAP + topRightShift + (int)(i / numIconsPerRow) * (ICON_SIZE + ICON_GAP),
                    ICON_SIZE, ICON_SIZE);
                if (iconBounds.Y + ICON_SIZE < 0) continue;
                if (iconBounds.Y > bounds.Y + bounds.Height) continue;

                this.iconBounds.Add(iconBounds, b.name);
                g.DrawImage(b.icon, iconBounds);
                if (grayedBuildingList.Contains(b))
                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(255 * (1 - OVERLAY_STRENGTH)), Color.LightGray)), iconBounds);
                i++;
            }

            // Also draw a tooltip if the mouse is over a building
            g.ResetClip();
            if (GetArea(mouseX, mouseY).Equals("top right panel")) // In the right screen area
            {
                // Find if it's actually over an icon
                foreach (KeyValuePair<Rectangle, string> pair in iconBounds)
                {
                    // If it's on the icon
                    if (mouseX >= pair.Key.X && mouseX <= pair.Key.X + pair.Key.Width && mouseY >= pair.Key.Y && mouseY <= pair.Key.Y + pair.Key.Height)
                    {
                        // Draw the appropriate tooltip
                        DrawEntityTooltip(g, client.info.buildingTypes[pair.Value], new Rectangle((int)mouseX - TOOLTIP_WIDTH, (int)mouseY, TOOLTIP_WIDTH, TOOLTIP_HEIGHT));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the icon of the building that is currently being placed on the mouse cursor.
        /// Turns the building red if that building type would not be able to be placed there 
        /// (or does not draw it at all if the cursor is off the game area).
        /// Should not be called if placingBuilding is null.
        /// </summary>
        private void RenderPlacingBuilding(Graphics g)
        {
            // If it's not on the game area, we don't draw it
            if (!GetArea(mouseX, mouseY).Equals("game area")) return;

            double tileX = mouseX / tileWidth - placingBuilding.size / 2 + worldX;
            double tileY = mouseY / tileHeight - placingBuilding.size / 2 + worldY;

            // We want to draw it so that the mouse is in the middle
            Rectangle bounds = new Rectangle((int)(mouseX - placingBuilding.size * tileWidth / 2), (int)(mouseY - placingBuilding.size * tileHeight / 2),
                                             (int)(placingBuilding.size * tileWidth), (int)(placingBuilding.size * tileHeight));
            g.DrawImage(placingBuilding.image, bounds);

            // Overlay it in red if it would be an invalid placement
            if (!world.ValidBuildingPlacement(placingBuilding, tileX, tileY))
                g.FillRectangle(new SolidBrush(Color.FromArgb((int)(255 * (1 - OVERLAY_STRENGTH)), Color.DarkRed)), bounds);
        }

        /// <summary>
        /// Processes any key down events on the game window. If they are recognised as
        /// utilised keys, performs the appropriate action on the client.
        /// </summary>
        public void KeyEvent(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if (key == Keys.Up)
                worldY -= SCREEN_Y_CHANGE;
            else if (key == Keys.Down)
                worldY += SCREEN_Y_CHANGE;
            else if (key == Keys.Right)
                worldX += SCREEN_X_CHANGE;
            else if (key == Keys.Left)
                worldX -= SCREEN_X_CHANGE;
            else if (key == Keys.ShiftKey)
                shiftKeyDown = true;
            else if (key == Keys.Delete)
                client.DeleteSelected();
        }

        /// <summary>
        /// Processes any key up events on the game window.
        /// </summary>
        public void KeyUpEvent(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if (key == Keys.ShiftKey)
                shiftKeyDown = false;
        }

        /// <summary>
        /// Processes any mouse clicking events on the game window.
        /// Mostly figures out the area where the click was and calls the appropriate
        /// method in the client.
        /// </summary>
        private void MouseClickEvent(object sender, MouseEventArgs e)
        {
            string area = GetArea(e.X, e.Y);
            if (e.Button == MouseButtons.Right)
            {
                // If you're currently placing a building, get rid of it
                if (area.Equals("game area") && placingBuilding != null)
                    placingBuilding = null;

                // If it's on the game area and there's a selected unit, tell the client
                // Note that this can occur while you're also placing a building, in which case it
                // prioritises getting rid of the building
                else if (area.Equals("game area") && client.selected.Count > 0)
                {
                    double tileX = worldX + (double)e.X / tileWidth;
                    double tileY = worldY + (double)e.Y / tileHeight;
                    RightClick(tileX, tileY);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                mouseDown = false;

                // Check for placing a building
                if (placingBuilding != null)
                {
                    if (area.Equals("game area"))
                    {
                        double x = (double)e.X / tileWidth - placingBuilding.size / 2 + worldX; // Shift it over as the middle of the building should be on the mouse
                        double y = (double)e.Y / tileWidth - placingBuilding.size / 2 + worldY;
                        if (x < 0) x = 0; if (x + placingBuilding.size > world.MapSize().x) x = world.MapSize().x - placingBuilding.size;
                        if (y < 0) y = 0; if (y + placingBuilding.size > world.MapSize().y) y = world.MapSize().y - placingBuilding.size;
                        client.CreateFoundation(placingBuilding, x, y);
                        if (!shiftKeyDown) placingBuilding = null; // Let the player place multiple buildings with the shift key being down
                    }
                    else
                        placingBuilding = null;
                }

                // A mouse click on the minimap
                else if (area.Equals("minimap") && !mouseDownOnGameArea)
                    ScrollToMinimapPoint(e.X, e.Y);

                // A mouse click on the game area (not the minimap as this has already been checked)
                else if (area.Equals("game area") && mouseDownOnGameArea)
                {
                    // If the mouse was dragged across a sizeable area, treat it as a drag
                    if (Math.Abs(e.X - startX) > 20 || Math.Abs(e.Y - startY) > 20)
                    {
                        double x1 = e.X > startX ? startX / tileWidth : (double)e.X / tileWidth;
                        double y1 = e.Y > startY ? startY / tileHeight : (double)e.Y / tileHeight;
                        double width = Math.Abs(e.X - startX) / tileWidth;
                        double height = Math.Abs(e.Y - startY) / tileHeight;
                        SelectEntitiesInArea(x1+worldX, y1+worldY, width, height);
                    }
                    // Otherwise treat it as a click
                    else
                    {
                        Entity entity = world.GetEntityAt((double)e.X / tileWidth, (double)e.Y / tileHeight);
                        client.selected.Clear();
                        if (entity != null) client.selected.Add(entity);
                    }
                }

                // On the top right panel
                else if (area.Equals("top right panel"))
                {
                    // On the "switch between panels" place
                    if (e.Y <= 40 && e.X > GAME_AREA_WIDTH * Width + RIGHT_AREA_BORDER_WIDTH)
                        buildingPanelOpen = !buildingPanelOpen;

                    // Otherwise check if it's on an icon
                    foreach (KeyValuePair<Rectangle, string> pair in iconBounds)
                    {
                        if (e.X >= pair.Key.X && e.X <= pair.Key.X + pair.Key.Width && e.Y >= pair.Key.Y && e.Y <= pair.Key.Y + pair.Key.Height)
                        {
                            // If so, figure out which building or technology it is and act upon it
                            if (buildingPanelOpen)
                            {
                                BuildingType type = client.info.buildingTypes[pair.Value];
                                if (!client.CanBuild(type)) return;
                                placingBuilding = type;
                            }
                            else
                            {
                                Technology tech = client.info.technologies[pair.Value];
                                if (!client.CanResearch(tech)) return;
                                client.ResearchTechnology(tech);
                            }
                        }
                    }
                }

                // On the selected entity panel while a building is there
                else if (area.Equals("selected entity panel") && client.selected[0] is Building)
                {
                    // Check if the mouse is on a unit
                    foreach (KeyValuePair<Rectangle, UnitType> pair in unitTrainingIconBounds)
                        if (e.X >= pair.Key.X && e.X <= pair.Key.X + pair.Key.Width && e.Y >= pair.Key.Y && e.Y <= pair.Key.Y + pair.Key.Height)
                        {
                            // If it is, try to train that type of unit
                            client.CreateUnit((Building)client.selected[0], pair.Value);
                            Console.WriteLine("creating " + pair.Value);
                        }
                }

            }
        }

        /// <summary>
        /// Processes a right click, given that there is at least one
        /// selected entity, at the given location; attacking or moving
        /// the units.
        /// </summary>
        public void RightClick(double x, double y)
        {
            // If the right click is out of bounds, do nothing
            if (world.OutOfBounds(x, y)) return;

            // If there's an enemy there, attack it
            Entity[] overlappingEntities = world.GetEntitiesIn(x, y, 0, 0);
            Entity[] enemies = player.GetEnemies(overlappingEntities);
            if (enemies.Length > 0)
            {
                // Pick any random entity; no guarantee is made as to the order
                client.AttackEntityWithSelected(enemies[0]);
            }

            // Otherwise, move the units
            else
            {
                client.MoveSelectedUnits(x, y);
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
            mouseDownOnTopRightArea = GetArea(e.X, e.Y).Equals("top right panel");
        }

        /// <summary>
        /// Processes any mouse movement events on the game window.
        /// </summary>
        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            // If we're dragging in the top right panel, scroll it
            if (mouseDown && mouseDownOnTopRightArea)
            {
                topRightShift -= (int)mouseY - e.Y;
                if (topRightShift > 0) topRightShift = 0;
            }


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
            double minimapX = x - Width * (GAME_AREA_WIDTH - MINIMAP_WIDTH) - MINIMAP_BORDER_SIZE;
            double minimapY = y - (Height * GAME_AREA_HEIGHT - Width * MINIMAP_WIDTH) - MINIMAP_BORDER_SIZE;
            double minimapSize = Width * MINIMAP_WIDTH - MINIMAP_BORDER_SIZE;

            // How far across (0-1) the point is
            double proportionX = minimapX / minimapSize;
            double proportionY = minimapY / minimapSize;

            // The tile of the map we should therefore center on is..
            worldX = world.MapSize().x * proportionX - maxXTiles / 2;
            worldY = world.MapSize().y * proportionY - maxYTiles / 2;
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

            // Prerequisite
            g.DrawString("Requires: " + type.prerequisite, miniFont, Brushes.Black, new Rectangle(bounds.X + 10, pos.Y, bounds.Width - 20, bounds.Height - (pos.Y - bounds.Y) - 10));
            pos.Y += (int)(g.MeasureString("Requires: " + type.prerequisite, miniFont, bounds.Width - 20).Height) + 15;

            // Description (wrap it in the box)
            g.DrawString(type.description, miniFont, Brushes.Black, new Rectangle(bounds.X + 10, pos.Y, bounds.Width - 20, bounds.Height - (pos.Y-bounds.Y) - 10));

            g.ResetClip();
        }

        /// <summary>
        /// Draws a tooltip for the specified technology at the specified location.
        /// </summary>
        private void DrawTechnologyTooltip(Graphics g, Technology technology, Rectangle bounds)
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
            g.DrawString(technology.name, titleFont, Brushes.Black, new Point(bounds.X + bounds.Width / 2, pos.Y), centerFormat);
            pos.Y += (int)(titleFont.Size) + 20;

            // Cost
            double iconGapRatio = (double)ICON_SIZE / ICON_GAP;
            int resourceGapSize = (int)(bounds.Width / (1 + (1 + iconGapRatio) * GameInfo.RESOURCE_TYPES));
            int resourceIconSize = (int)(resourceGapSize * iconGapRatio);

            for (int i = 0; i < GameInfo.RESOURCE_TYPES; i++)
            {
                if (i == GameInfo.RESOURCE_TYPES / 2) { pos.Y += ICON_SIZE + 10; pos.X = bounds.X + ICON_GAP; }

                g.DrawImage(resourceImages[i], pos.X, pos.Y, ICON_SIZE, ICON_SIZE);
                pos.X += ICON_SIZE + 10;
                g.DrawString(technology.resourceCosts[i] + "", font, Brushes.Black, new Point(pos.X, pos.Y + ICON_SIZE / 2), vertCenterFormat);
                pos.X += (int)g.MeasureString("9999", font).Width + 10;
            }
            pos.X = bounds.X + ICON_GAP;
            pos.Y += ICON_SIZE + 10;

            // Prerequisites
            StringBuilder prereqBuilder = new StringBuilder();
            foreach(string s in technology.prerequisites)
                prereqBuilder.Append(s + ", ");
            string prereqBuilderString = prereqBuilder.ToString().Length > 0 ? prereqBuilder.ToString() : "Nothing!..";
            string finalString = "Requires: " + prereqBuilderString.Substring(0, prereqBuilderString.Length - 2);
            g.DrawString(finalString, miniFont, Brushes.Black, new Rectangle(bounds.X + 10, pos.Y, bounds.Width - 20, bounds.Height - (pos.Y-bounds.Y) - 10));

            pos.Y += (int)(g.MeasureString(finalString, miniFont, bounds.Width - 20).Height) + 15;

            // Description
            g.DrawString(technology.description, miniFont, Brushes.Black, new Rectangle(bounds.X + 10, pos.Y, bounds.Width - 20, bounds.Height - (pos.Y - bounds.Y) - 10));

            g.ResetClip();
        }
    }
}
