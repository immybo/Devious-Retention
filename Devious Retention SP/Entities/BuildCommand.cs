using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Devious_Retention_SP.Entities;

namespace Devious_Retention_SP
{
    public class BuildCommand : Command, ICallback
    {
        private Builder builder;
        private Building building;
        private World world;

        private int currentTick;

        public BuildCommand(Builder builder, Building building, World world)
        {
            this.builder = builder;
            this.building = building;
            this.world = world;
        }

        public override void Execute()
        {
            builder.AddPendingCommand(this);
            currentTick = 1;
        }

        public override bool Tick()
        {
            if (!base.Tick()) return false;

            if (builder is Unit)
            {
                Unit uBuilder = (Unit)builder;
                MoveWithinRange(uBuilder);
                
                bool ok = true;
                foreach (Command c in builder.GetPendingCommands())
                {
                    if (c is MoveCommand)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    PerformBuildTick();
                }
            }
            else if(Entity.WithinRange(builder, building, 1))
            {
                PerformBuildTick();
            }
            else
            {
                // We can't build it, because we're unable to move
                return false;
            }

            return !building.IsFullyBuilt;
        }

        // TODO factor this out, since both buildcommand and attackcommand use it
        private void MoveWithinRange(Unit uBuilder)
        {
            // Move to an applicable point if we have to
            if (!Entity.WithinRange(builder, building, 1))
            {
                // Only redo movement every 10 ticks
                if (currentTick == 10)
                {
                    uBuilder.MoveWithinRange(building, 1, this, world);
                    currentTick = 0;
                }

                currentTick++;
            }
        }

        private void PerformBuildTick()
        {
            int amountBuilt = (int)(builder.GetBuildSpeed() / building.BuildResistance);
            amountBuilt = amountBuilt == 0 ? 1 : amountBuilt;
            building.Build(amountBuilt);
        }

        public void Callback()
        {
            // We're within range now
        }
    }
}
