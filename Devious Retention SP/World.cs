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

        /// <summary>
        /// Returns a list of entities, containing all entities
        /// which are at least partially contained within the given
        /// area.
        /// </summary>
        public List<Entity> GetEntitiesInArea(RectangleF area)
        {
            List<Entity> ret = new List<Entity>();
            foreach(Entity e in entities)
            {
                if(e.X + e.Size > area.X &&
                   e.Y + e.Size > area.Y &&
                   e.X < area.Right &&
                   e.Y < area.Bottom)
                {
                    ret.Add(e);
                }
            }
            return ret;
        }
    }
}
