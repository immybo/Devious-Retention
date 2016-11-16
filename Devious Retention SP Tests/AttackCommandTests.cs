using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Devious_Retention_SP;
using Devious_Retention_SP.Entities;

namespace Devious_Retention_SP_Tests
{
    [TestClass]
    public class AttackCommandTests
    {
        private static WorldTemplate CONFIG_DEFAULT = new WorldTemplate(1);

        [TestMethod]
        public void TestAttackPoint()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Attacker attacker = new TestUnit(world.players[0], 3, 3, 1);
            Attackable defender = new TestUnit(world.players[0], 7, 7, 1);
            AttackCommand command = new AttackCommand(attacker, defender, world.world);
            Utilities.ApplyCommandSynchronous(command, attacker);

            // Make sure the attacker is within range of the defender
            double actualRange = Math.Sqrt(Math.Pow(Math.Abs(attacker.X - defender.X), 2)
                                        + Math.Pow(Math.Abs(attacker.Y - defender.Y), 2));
            Assert.IsTrue(actualRange < attacker.GetRange());
        }
    }
}
