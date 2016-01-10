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

        private const double RESOURCE_WIDTH = 0.2;

        // relative to the width of the selection panel
        private const double DAMAGE_ICON_SIZE = 0.06;
        private const double TRAINING_QUEUE_ICON_SIZE = 0.19;
        // pixels
        private const int MINIMAP_BORDER_SIZE = 20;
        private const int RIGHT_AREA_BORDER_WIDTH = 20;
        private const int RESOURCE_AREA_BORDER_WIDTH = 20;

        public GameClient client;

        // Where the mouse started dragging, for selection purposes
        private double startX = -1;
        private double startY = -1;

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
