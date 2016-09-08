using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    /// <summary>
    /// There is exactly one world running in the process at a time. It holds
    /// responsibility for maintaining entities and the map, and is often changed
    /// by the players as such.
    /// </summary>
    public class World : Drawable
    {
        public Map Map { get; private set; }
        private List<Entity> entities;

        public World()
        {
            this.Map = new Map();
            this.entities = new List<Entity>();
        }

        /// <summary>
        /// Updates everything in the world by one tick.
        /// </summary>
        public void Tick()
        {
            foreach (Entity e in entities)
                e.Tick();
        }

        public void Draw(Graphics g, PositionTransformation p)
        {
            Map.Draw(g, p);
            foreach(Entity e in entities)
            {
                e.Draw(g, p);
            }
        }

        public void AddEntity(Entity e)
        {
            entities.Add(e);
        }
        public List<Entity> GetEntities()
        {
            return entities;
        }
    }
}
