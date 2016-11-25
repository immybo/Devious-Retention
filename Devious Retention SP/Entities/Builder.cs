using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention_SP.Entities
{
    public interface Builder : IEntity
    {
        float GetBuildSpeed();
    }
}
