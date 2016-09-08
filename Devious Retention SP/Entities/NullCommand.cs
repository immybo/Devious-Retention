using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP
{
    public class NullCommand : Command
    {
        public void Execute()
        {
        }

        public bool Tick()
        {
            return false;
        }
    }
}
