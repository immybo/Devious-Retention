using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devious_Retention_SP
{
    /// <summary>
    /// Is responsible for launching and ending games, as well
    /// as the tick cycle.
    /// </summary>
    public class Game
    {
        private GameConfiguration config;
        private Player[] players;
        private World world;

        private Timer tickClock;

        public Game(Player[] players, World world, GameConfiguration config)
        {
            this.players = players;
            this.world = world;
            this.config = config;

            tickClock = new Timer();
            tickClock.Tick += Tick;
        }

        public void SetTickSpeed(int percentage)
        {
            tickClock.Interval = percentage * config.DEFAULT_TICK_TIME;
        }

        /// <summary>
        /// Begins the game.
        /// </summary>
        public void RunGame()
        {
            tickClock.Interval = config.DEFAULT_TICK_TIME;
            tickClock.Start();
        }

        /// <summary>
        /// If the game was unpaused, pauses it.
        /// If the game was paused, unpauses it.
        /// </summary>
        public void PauseGame()
        {
            if (tickClock.Enabled)
                tickClock.Stop();
            else
                tickClock.Start();
        }

        /// <summary>
        /// Halts the game.
        /// </summary>
        public void StopGame()
        {
            tickClock.Stop();
        }

        public void Tick(object o, EventArgs e)
        {
            world.Tick();
        }
    }
}
