using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Devious_Retention_SP;
using Devious_Retention_SP.Entities;

namespace Devious_Retention_SP_Tests
{
    [TestClass]
    public class BuildCommandTests
    {
        private static WorldTemplate CONFIG_DEFAULT = new WorldTemplate(2);

        [TestMethod]
        public void TestBuildPoint()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Builder builder = new TestUnit(world.players[0], 3, 3, 1);
            Building building = new TestBuilding(world.players[1], 7, 7, 1, 1);
            BuildCommand command = new BuildCommand(builder, building, world.world);
            Utilities.ApplyCommandSynchronous(command, builder, world.world);

            // Make sure the builder gets within range of the defender
            Assert.IsTrue(Entity.WithinRange(builder, building, 1.1f));
        }

        [TestMethod]
        public void TestBuilderBuilds()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Builder builder = new TestUnit(world.players[0], 3, 3, 1);
            Building building = new TestBuilding(world.players[1], 7, 7, 1, 1);
            BuildCommand command = new BuildCommand(builder, building, world.world);
            Utilities.ApplyCommandSynchronous(command, builder, world.world);

            Assert.IsTrue(building.IsFullyBuilt);
            Assert.IsTrue(building.Hitpoints == building.MaxHitpoints);
        }

        [TestMethod]
        public void TestMoveAfterBuild()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Builder builder = new TestUnit(world.players[0], 3, 3, 1);
            Building building = new TestBuilding(world.players[1], 7, 7, 1, 1);
            BuildCommand command = new BuildCommand(builder, building, world.world);
            command.Execute();
            builder.Tick(world.world);

            MoveCommand move = new MoveCommand((Unit)builder, new PointF(1, 1), world.world);
            Utilities.ApplyCommandSynchronous(move, builder, world.world);

            Assert.IsFalse(building.IsFullyBuilt);
            Assert.AreEqual(builder.GetBuildSpeed(), building.Hitpoints);
        }
    }
}
