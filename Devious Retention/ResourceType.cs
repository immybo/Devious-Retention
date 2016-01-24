﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devious_Retention
{
    /// <summary>
    /// A ResourceType defines attributes for a specific type of resource.
    /// </summary>
    public class ResourceType : ICloneable
    {
        public String name { get; private set; }
        // Which resource this ResourceType provides
        public int resourceType { get; private set; }
        // How much of the resource this ResourceType initially contains
        public int resourceAmount { get; private set; }

        private String imageFilename;
        public Image image { get; private set; }

        // How fast the resource is gathered from resources of this type -
        // 1 is default, higher is faster
        public double gatherSpeed { get; private set; }

        public double size{ get; private set; }

        /// <summary>
        /// Anything attempting to create a ResourceType from a file must first
        /// parse the string into these attributes.
        /// </summary>
        public ResourceType(string name, int resourceType, int resourceAmount, string imageFilename, double gatherSpeed, double size)
        {
            this.name = name;
            this.resourceType = resourceType;
            this.resourceAmount = resourceAmount;
            this.imageFilename = imageFilename;
            this.gatherSpeed = gatherSpeed;
            this.size = size;

            try
            {
                image = Image.FromFile(GameInfo.RESOURCE_IMAGE_BASE + imageFilename);
            }
            // If the image can't be loaded, load a default one instead (which hopefully can!)
            catch (IOException)
            {
                image = Image.FromFile(GameInfo.DEFAULT_IMAGE_NAME);
            }
        }

        /// <summary>
        /// Returns:
        /// "[name] [resourceType] [resourceAmount] [imageFilename] [gatherSpeed] [size]"
        /// </summary>
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + " ");
            builder.Append(resourceType + " ");
            builder.Append(resourceAmount + " ");
            builder.Append(imageFilename + " ");
            builder.Append(gatherSpeed + " ");
            builder.Append(size);
            return builder.ToString();
        }

        /// <summary>
        /// Returns a new ResourceType completely identical to this one.
        /// </summary>
        public object Clone()
        {
            return new ResourceType(name, resourceType, resourceAmount, imageFilename, gatherSpeed, size);
        }
    }
}
