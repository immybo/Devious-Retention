using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A ResourceType defines attributes for a specific type of resource.
    /// </summary>
    class ResourceType
    {
        private int id;
        // Which resource this ResourceType provides
        private int resourceType;
        // How much of the resource this ResourceType initially contains
        private int resourceAmount;

        private Image image;

        // How fast the resource is gathered from resources of this type -
        // 1 is default, higher is faster
        private double gatherSpeed;

        /// <summary>
        /// Constructing a ResourceType requires providing a string that is
        /// equivalent to a ResourceType's ToString output.
        /// </summary>
        public ResourceType(String s)
        {

        }

        /// <summary>
        /// Returns:
        /// "[id] [resourceType] [resourceAmount] [imageFilename] [gatherSpeed]"
        /// </summary>
        public override String ToString()
        {

        }
    }
}
