#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// Container for export model data.
    /// </summary>
    public class ExportModelContainer
    {
        private Dictionary<NamedOrGenericKey, ExportMaterialInfo> m_dicExportMaterial;
        private Dictionary<NamedOrGenericKey, ExportGeometryInfo> m_dicExportGeometry;
        private Dictionary<SceneObject, object> m_dicOriginalObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportModelContainer"/> class.
        /// </summary>
        internal ExportModelContainer()
        {
            m_dicExportMaterial = new Dictionary<NamedOrGenericKey, ExportMaterialInfo>();
            m_dicExportGeometry = new Dictionary<NamedOrGenericKey, ExportGeometryInfo>();
            m_dicOriginalObjects = new Dictionary<SceneObject, object>();
        }

        public void AddExportGeometry(ExportGeometryInfo exportGeometry)
        {
            exportGeometry.EnsureNotNull(nameof(exportGeometry));

            m_dicExportGeometry[exportGeometry.Key] = exportGeometry;
        }

        public void AddExportMaterial(ExportMaterialInfo exportMaterial)
        {
            exportMaterial.EnsureNotNull(nameof(exportMaterial));

            m_dicExportMaterial[exportMaterial.Key] = exportMaterial;
        }

        /// <summary>
        /// Registers the given original object.
        /// Original objects are hold to be able to check whether parents are also exported.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        internal void RegisterOriginalObject(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            m_dicOriginalObjects[sceneObject] = null;
        }

        public bool ContainsExportGeometry(NamedOrGenericKey key)
        {
            return m_dicExportGeometry.ContainsKey(key);
        }

        public bool ContainsExportMaterial(NamedOrGenericKey key)
        {
            return m_dicExportMaterial.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether the given original is also exported by this container.
        /// Original objects are hold to be able to check whether parents are also exported.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        public bool ContainsOrignalObject(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            return m_dicOriginalObjects.ContainsKey(sceneObject);
        }
    }
}
