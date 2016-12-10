using Devious_Retention_SP.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Represents a human player on the local machine.
    /// </summary>
    public class HumanPlayer : Player, HumanPlayerListener
    {
        private HumanPlayerWindow window;
        private Timer drawClock;

        private List<Entity> selectedEntities;

        public HumanPlayer(World world, Color color, GameConfiguration config)
            : base(world, color, config)
        {
            selectedEntities = new List<Entity>();
        }

        public void Run()
        {
            window = new HumanPlayerWindow(this, this, this.world);

            drawClock = new Timer();
            drawClock.Interval = 16;
            drawClock.Tick += new EventHandler(Tick);
            drawClock.Start();

            Application.Run(window);
        }

        public void Tick(object o, EventArgs e)
        {
            window.Refresh();
        }

        public void DoGameAreaClick(PointF worldCoordinate, MouseButtons buttons)
        {
            Console.WriteLine("Mouse pressed at " + worldCoordinate.X + ", " + worldCoordinate.Y + ".");
            
            foreach (Entity e in selectedEntities)
                if (e.Player == this)
                    doClick(worldCoordinate, buttons, e);
        }

        public void DoKeyPress(Keys keys)
        {
            Console.WriteLine("Key " + keys.ToString() + " pressed.");

            foreach (Entity e in selectedEntities)
                if (e.Player == this)
                    doKeys(keys, e);
        }

        public void DoGameAreaDrag(RectangleF bounds)
        {
            Console.WriteLine("Dragging around area: " + bounds);

            selectedEntities = world.GetEntitiesInArea(bounds);
        }

        public Entity[] GetSelectedEntities()
        {
            return selectedEntities.ToArray();
        }

        /// <summary>
        /// Performs the action which a specific entity should perform on the press of
        /// the given set of mouse buttons.
        /// </summary>
        private void doClick(PointF worldCoordinate, MouseButtons buttons, Entity entity)
        {
            List<Entity> overlappingEntities = world.GetEntitiesAtPoint(worldCoordinate);

            if (buttons == MouseButtons.Left){
                // Select an entity if we left click on it
                if (overlappingEntities.Count > 0){
                    selectedEntities.Clear();
                    selectedEntities.Add(overlappingEntities[0]);
                }
            }
            else if (buttons == MouseButtons.Right)
            {
                // The highest priority is to attack 
                if (entity is Attacker)
                {
                    foreach (Entity e in overlappingEntities)
                    {
                        if (e is Attackable && e.Player != this)
                        {
                            new AttackCommand((Attacker)entity, (Attackable)e, world).Execute();
                            return;
                        }
                    }
                }

                // Then to gather
                if(entity is Gatherer)
                {
                    foreach(Entity e in overlappingEntities)
                    {
                        if(e is Gatherable && ((Gatherable)e).CurrentResourceCount() > 0)
                        {
                            new GatherCommand((Gatherer)entity, (Gatherable)e, world).Execute();
                            return;
                        }
                    }
                }

                // Then to move
                if(entity is Unit)
                {
                    new MoveCommand((Unit)entity, worldCoordinate, world).Execute();
                    return;
                }
            }
        }

        /// <summary>
        /// Performs the action which a specific entity should perform on the press of
        /// the given set of keys.
        /// </summary>
        private void doKeys(Keys keys, Entity entity)
        {

        }
    }
}
