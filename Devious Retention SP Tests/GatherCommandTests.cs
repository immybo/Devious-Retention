﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Devious_Retention_SP;
using Devious_Retention_SP.Entities;

namespace Devious_Retention_SP_Tests
{
    [TestClass]
    public class GatherCommandTests
    {
        private static WorldTemplate CONFIG_DEFAULT = new WorldTemplate(2);

        [TestMethod]
        public void TestGatherPoint()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Gatherer gatherer = new TestUnit(world.players[0], 3, 3, 1, 100, 1);
            Resource resource = new TestResource(5, 5, 1, 0, 1000, 1000);
            GatherCommand command = new GatherCommand(gatherer, resource, world.world);
            Utilities.ApplyCommandSynchronous(command, gatherer, world.world);
            
            Assert.IsTrue(Entity.WithinRange(gatherer, resource, 1.2f));
            
            // Make sure that the resource has no resource left
            Assert.IsTrue(resource.CurrentResourceCount() == 0);
        }

        [TestMethod]
        public void TestDoubleGather()
        {
            WorldConfiguration world = Utilities.BuildWorldFromTemplate(CONFIG_DEFAULT);
            Gatherer gatherer = new TestUnit(world.players[0], 3, 3, 1, 100, 1);
            Resource resource = new TestResource(5, 5, 1, 0, 1000, 1000);
            GatherCommand command = new GatherCommand(gatherer, resource, world.world);
            Utilities.ApplyCommandSynchronous(command, gatherer, world.world);
            GatherCommand command2 = new GatherCommand(gatherer, resource, world.world);
            Utilities.ApplyCommandSynchronous(command2, gatherer, world.world);

            // Make sure that the resource has no resource left (non-negative)
            Assert.IsTrue(resource.CurrentResourceCount() == 0);
        }
    }
}
