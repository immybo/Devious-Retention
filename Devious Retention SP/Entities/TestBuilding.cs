using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP.Entities
{
    public class TestBuilding : Building, Attacker, Trainer
    {
        private const float TRAINING_SPEED = 1f;

        private Queue<Unit> trainingQueue;

        public TestBuilding(Player player, double x, double y, double size, float buildResistance)
            : base(player, x, y, size, "TestBuilding", buildResistance)
        {
            this.MaxHitpoints = 100;
            this.Hitpoints = 1;

            this.trainingQueue = new Queue<Unit>();
        }

        public override void Damage(int amount, int damageType)
        {
            this.Hitpoints -= amount;
        }

        public override void Draw(Graphics g, PositionTransformation p)
        {
            PointF topLeft = p.Transform(this.GetPosition());
            Rectangle rect = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(p.Scale().X * this.Size), (int)(p.Scale().Y * this.Size));
            g.FillRectangle(new SolidBrush(this.Player.Color), rect);
            g.DrawString("Building", new Font("Arial", 20), new SolidBrush(Color.Black), new PointF(topLeft.X + 20, topLeft.Y + 20));
        }

        public int GetAttackTime()
        {
            return 20;
        }

        public int GetDamage()
        {
            return 10;
        }

        public int GetDamageType()
        {
            return 0;
        }

        public float GetRange()
        {
            return 2;
        }

        public string[] GetTrainableUnits()
        {
            // TODO something so we don't specify the literals
            return new string[] { "testUnit" };
        }
        
        public Unit[] GetTrainingQueue()
        {
            return trainingQueue.ToArray();
        }

        public float GetTrainingSpeed(){
            return TRAINING_SPEED;
        }

        public void Train(string entityStr)
        {
            if (!GetTrainableUnits().Contains(entityStr))
                throw new ArgumentException("Attempting to create a " + entityStr + " which can't be created from this Trainer.");
            Entity entity = Entity.FromName(entityStr);
            if (!(entity is Unit))
                throw new InvalidOperationException("Attempting to create a non-unit from a trainer!");
            trainingQueue.Enqueue((Unit)entity);
        }
    }
}
