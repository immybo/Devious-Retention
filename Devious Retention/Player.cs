using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// Each player represents a game client running on a machine.
    /// </summary>
    public class Player
    {
        // TODO players owning entities

        // Silly sounding names but they make sense...
        public int Number { get; private set; }
        public Color Color { get; private set; }
        public Brush Brush { get; private set; }
        public Pen Pen { get; private set; }

        private Faction faction;
        // Every player must have a separate set of definitions,
        // as they'll be changed by technologies and faction.
        public GameInfo Definitions { get; private set; }
        private Relation[] playerRelations;

        public Player(Relation[] baseRelations, int number, Color color, Faction faction, GameInfo definitions)
        {
            playerRelations = baseRelations;
            Number = number;
            Color = color;
            this.faction = faction;
            this.Definitions = definitions;
            Brush = new SolidBrush(color);
            Pen = new Pen(Brush);
        }

        /// <summary>
        /// Returns whether or not this player is the owner of
        /// the given entity.
        /// </summary>
        public bool Owns(Entity entity)
        {
            return entity.Player == this;
        }

        /// <summary>
        /// Generates and returns an array of entities which are
        /// enemies to this player, and are contained within
        /// the given array.
        /// </summary>
        public Entity[] GetEnemies(Entity[] entities)
        {
            List<Entity> enemies = new List<Entity>();
            foreach (Entity entity in entities)
                if (IsEnemy(entity))
                    enemies.Add(entity);
            return enemies.ToArray();
        }

        /// <summary>
        /// Finds out and returns whether or not the given entity is
        /// an enemy of this player.
        /// </summary>
        private bool IsEnemy(Entity entity)
        {
            return entity.Attackable() && GetRelation(entity.Player.Number) == Relation.ENEMY;
        }

        /// <summary>
        /// Returns The relation which this player has to the player
        /// with the given number.
        /// </summary>
        private Relation GetRelation(int playerNumber)
        {
            if (playerNumber < 0 || playerNumber >= playerRelations.Length)
                throw new ArgumentOutOfRangeException("Attempting to find relation with player #" + playerNumber + ", who does not exist.");
            return playerRelations[playerNumber];
        }

        public static Relation[] DefaultRelations(int playerNumber, int playerCount)
        {
            Relation[] relations = new Relation[playerCount];
            for (int i = 0; i < playerCount; i++)
                relations[i] = Relation.ENEMY;
            relations[playerNumber] = Relation.ALLIED;
            return relations;
        }

        public enum Relation
        {
            ALLIED,
            ENEMY
        }
    }

    /// <summary>
    /// A player which represents the local game client.
    /// We don't really need to know most things about remote players.
    /// </summary>
    public class LocalPlayer : Player
    {
        private bool[][] LOS;
        // How many of each resource the player currently has
        // Resources are handled entirely client-side
        // metal, oil, energy, science
        private double[] currentResources;

        public LocalPlayer(Relation[] baseRelations, int number, Color color, Faction faction, GameInfo definitions)
            : base(baseRelations, number, color, faction, definitions)
        {
            currentResources = new double[GameInfo.RESOURCE_TYPES];
        }

        /// <summary>
        /// Returns whether or not the player can afford all
        /// of the given resource costs.
        /// </summary>
        public bool CanAfford(int[] resourceCosts)
        {
            if (resourceCosts.Length != currentResources.Length)
                throw new ArgumentException("Not the right amount of resources passed to CanAfford. Passed: "
                    + resourceCosts.Length + " Actual: " + currentResources.Length);

            for (int i = 0; i < resourceCosts.Length; i++)
                if (resourceCosts[i] > currentResources[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Deducts the given amounts of resources from this
        /// player's stockpiles, disregarding whether or not
        /// the player can actually afford it.
        /// </summary>
        public void PayResources(int[] resourceCosts)
        {
            if (resourceCosts.Length != currentResources.Length)
                throw new ArgumentException("Not the right amount of resources passed to PayResources. Passed: "
                    + resourceCosts.Length + " Actual: " + currentResources.Length);

            for (int i = 0; i < resourceCosts.Length; i++)
                currentResources[i] -= resourceCosts[i];
        }

        public void AddResource(int resourceIndex, double amount)
        {
            if (resourceIndex >= currentResources.Length || resourceIndex < 0)
                throw new ArgumentOutOfRangeException("Invalid resource index passed to add resource. Index: " + resourceIndex);
            currentResources[resourceIndex] += amount;
        }

        public double GetResource(int resourceIndex)
        {
            if (resourceIndex >= currentResources.Length || resourceIndex < 0)
                throw new ArgumentException("Invalid resource index given to GetResource. Index: " +
                                            resourceIndex + " Maximum: " + (currentResources.Length - 1));
            return currentResources[resourceIndex];
        }
    }
}
