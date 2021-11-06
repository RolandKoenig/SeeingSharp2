using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public class ImportedResourceInfo
    {
        public NamedOrGenericKey ResourceKey
        {
            get;
        }

        public Func<EngineDevice, Resource> ResourceFactory
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedResourceInfo"/> class.
        /// </summary>
        /// <param name="resourceKey">The key which is used by added the resource to a scene.</param>
        /// <param name="resourceFactory">The resource factory.</param>
        public ImportedResourceInfo(NamedOrGenericKey resourceKey, Func<EngineDevice, Resource> resourceFactory)
        {
            this.ResourceKey = resourceKey;
            this.ResourceFactory = resourceFactory;
        }

        public override string ToString()
        {
            return this.ResourceKey.ToString();
        }
    }
}
