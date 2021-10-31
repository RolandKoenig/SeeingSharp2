/*
    SeeingSharp and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
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
