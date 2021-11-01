using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// Container for export model data.
    /// </summary>
    public class ExportModelContainer
    {
        private Dictionary<NamedOrGenericKey, ExportGeometryInfo> _dicExportGeometry;
        private Dictionary<NamedOrGenericKey, ExportMaterialInfo> _dicExportMaterial;
        private Dictionary<SceneObject, object> _dicOriginalObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportModelContainer"/> class.
        /// </summary>
        internal ExportModelContainer()
        {
            _dicExportMaterial = new Dictionary<NamedOrGenericKey, ExportMaterialInfo>();
            _dicExportGeometry = new Dictionary<NamedOrGenericKey, ExportGeometryInfo>();
            _dicOriginalObjects = new Dictionary<SceneObject, object>();
        }

        public void AddExportGeometry(ExportGeometryInfo exportGeometry)
        {
            exportGeometry.EnsureNotNull(nameof(exportGeometry));

            _dicExportGeometry[exportGeometry.Key] = exportGeometry;
        }

        public void AddExportMaterial(ExportMaterialInfo exportMaterial)
        {
            exportMaterial.EnsureNotNull(nameof(exportMaterial));

            _dicExportMaterial[exportMaterial.Key] = exportMaterial;
        }

        public bool ContainsExportGeometry(NamedOrGenericKey key)
        {
            return _dicExportGeometry.ContainsKey(key);
        }

        public bool ContainsExportMaterial(NamedOrGenericKey key)
        {
            return _dicExportMaterial.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether the given original is also exported by this container.
        /// Original objects are hold to be able to check whether parents are also exported.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        public bool ContainsOriginalObject(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            return _dicOriginalObjects.ContainsKey(sceneObject);
        }

        /// <summary>
        /// Registers the given original object.
        /// Original objects are hold to be able to check whether parents are also exported.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        internal void RegisterOriginalObject(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            _dicOriginalObjects[sceneObject] = null;
        }
    }
}
