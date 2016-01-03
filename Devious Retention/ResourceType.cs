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
        public int id { get; private set; }
        // Which resource this ResourceType provides
        public int resourceType { get; private set; }
        // How much of the resource this ResourceType initially contains
        public int resourceAmount { get; private set; }

        private String imageFilename;
        public Image image { get; private set; }

        // How fast the resource is gathered from resources of this type -
        // 1 is default, higher is faster
        private double gatherSpeed;

        /// <summary>
        /// Constructing a ResourceType requires providing a string that is
        /// equivalent to a ResourceType's ToString output.
        /// </summary>
        public ResourceType(String s)
        {
            String[] attributes = s.Split(new char[] { ' ' });
            id = int.Parse(attributes[0]);
            resourceType = int.Parse(attributes[1]);
            resourceAmount = int.Parse(attributes[2]);
            imageFilename = attributes[3];
            image = Image.FromFile(imageFilename);
            gatherSpeed = double.Parse(attributes[4]);
        }

        /// <summary>
        /// Returns:
        /// "[id] [resourceType] [resourceAmount] [imageFilename] [gatherSpeed]"
        /// </summary>
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(id + " ");
            builder.Append(resourceType + " ");
            builder.Append(resourceAmount + " ");
            builder.Append(imageFilename + " ");
            builder.Append(gatherSpeed);
        }
    }
}
