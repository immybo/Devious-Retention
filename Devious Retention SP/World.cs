﻿using System;
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
        /// Returns an entity which overlaps the given point.
        /// No guarantee is made as to which type of entity it will be,
        /// or what types of entities priority will be given to.
        /// Returns null if no entity is present at the given location.
        /// </summary>
        public Entity GetEntityAtPoint(PointF worldPoint)
        {
            foreach(Entity e in entities)
            {
                if (e.X + e.Size > worldPoint.X &&
                   e.Y + e.Size > worldPoint.Y &&
                   e.X < worldPoint.X &&
                   e.Y < worldPoint.Y)
                {
                    return e; // just return the first one found
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a list of entities, containing all entities
        /// which are at least partially contained within the given
        /// area.
        /// </summary>
        public List<Entity> GetEntitiesInArea(RectangleF worldArea)
        {
            List<Entity> ret = new List<Entity>();
            foreach(Entity e in entities)
            {
                if(e.X + e.Size > worldArea.X &&
                   e.Y + e.Size > worldArea.Y &&
                   e.X < worldArea.Right &&
                   e.Y < worldArea.Bottom)
                {
                    ret.Add(e);
                }
            }
            return ret;
        }
    }
}
