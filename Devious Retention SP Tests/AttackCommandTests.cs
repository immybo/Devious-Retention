using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Devious_Retention_SP;
using Devious_Retention_SP.Entities;

namespace Devious_Retention_SP_Tests
{
    [TestClass]
    public class AttackCommandTests
    {
        private static WorldTemplate CONFIG_DEFAULT = new WorldTemplate(2);

        [TestMethod]
        public void TestAttackPoint()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Attacker attacker = new TestUnit(world.players[0], 3, 3, 1);
            Attackable defender = new TestUnit(world.players[1], 7, 7, 1);
            AttackCommand command = new AttackCommand(attacker, defender, world.world);
            Utilities.ApplyCommandSynchronous(command, attacker, world.world);

            // Make sure the attacker is within range of the defender
            double actualRange = Math.Sqrt(Math.Pow(Math.Abs(attacker.X - defender.X), 2)
                                        + Math.Pow(Math.Abs(attacker.Y - defender.Y), 2));
            Assert.IsTrue(actualRange < attacker.GetRange() + 0.1);
        }

        [TestMethod]
        public void TestAttackKills()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Attacker attacker = new TestUnit(world.players[0], 3, 3, 1);
            Attackable defender = new TestUnit(world.players[1], 4, 3, 1);
            AttackCommand command = new AttackCommand(attacker, defender, world.world);
            Utilities.ApplyCommandSynchronous(command, attacker, world.world);

            Assert.IsTrue(defender.IsDead());
            world.world.Tick();
            Assert.IsFalse(world.world.GetEntities().Contains(defender));
        }

        [TestMethod]
        public void TestMoveAfterAttack()
        {

            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Attacker attacker = new TestUnit(world.players[0], 3, 3, 1);
            Attackable defender = new TestUnit(world.players[1], 4, 3, 1);
            AttackCommand command = new AttackCommand(attacker, defender, world.world);
            command.Execute();
            attacker.Tick(world.world);

            MoveCommand move = new MoveCommand((Unit)attacker, new PointF(1, 1), world.world);
            Utilities.ApplyCommandSynchronous(move, attacker, world.world); // we know that, if this finishes, it's close enough
        }

        [TestMethod]
        public void TestAttackWhileMoving()
        {

            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Attacker attacker = new TestUnit(world.players[0], 3, 3, 1);
            Attackable defender = new TestUnit(world.players[1], 4, 3, 1);

            MoveCommand move = new MoveCommand((Unit)attacker, new PointF(1, 1), world.world);
            move.Execute();
            attacker.Tick(world.world);

            AttackCommand command = new AttackCommand(attacker, defender, world.world);
            Utilities.ApplyCommandSynchronous(command, attacker, world.world);

            Assert.IsTrue(defender.IsDead());
            world.world.Tick();
            Assert.IsFalse(world.world.GetEntities().Contains(defender));
        }
    }
}
