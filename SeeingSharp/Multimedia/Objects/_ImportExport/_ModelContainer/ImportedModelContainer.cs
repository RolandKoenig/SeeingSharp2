#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    /// <summary>
    /// Container for imported model data.
    /// </summary>
    public class ImportedModelContainer
    {
        #region Static id counter 
        private static int s_maxContainerID;
        #endregion

        #region All model data
        private int m_importID = 0;
        private ImportOptions m_importOptions;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedModelContainer" /> class.
        /// </summary>
        public ImportedModelContainer(ImportOptions importOptions)
        {
            m_importOptions = importOptions;
            Objects = new List<SceneObject>();
            ParentChildRelationships = new List<Tuple<SceneObject, SceneObject>>();
            ImportedResources = new List<ImportedResourceInfo>();

            m_importID = Interlocked.Increment(ref s_maxContainerID);
        }

        /// <summary>
        /// Creates and adds the root for all imported scene objects.
        /// </summary>
        public ScenePivotObject CreateAndAddRootObject()
        {
            // Append an object which transform the whole coordinate system
            var rootObject = new ScenePivotObject();

            // Handle base transformation
            switch(m_importOptions.ResourceCoordinateSystem)
            {
                case CoordinateSystem.LeftHanded_UpY:
                    rootObject.TransformationType = SpacialTransformationType.None;
                    break;

                case CoordinateSystem.LeftHanded_UpZ:
                    rootObject.Scaling = new Vector3(1f, -1f, 1f);
                    rootObject.RotationEuler = new Vector3(-EngineMath.RAD_90DEG, 0f, 0f);
                    rootObject.TransformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
                    break;

                case CoordinateSystem.RightHanded_UpY:
                    rootObject.Scaling = new Vector3(1f, 1f, -1f);
                    rootObject.TransformationType = SpacialTransformationType.ScalingTranslation;
                    break;

                case CoordinateSystem.RightHanded_UpZ:
                    rootObject.Scaling = new Vector3(-1f, 1f, -1f);
                    rootObject.RotationEuler = new Vector3(EngineMath.RAD_90DEG, 0f, 0f);
                    rootObject.TransformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
                    break;
            }

            // Add the object finally
            Objects.Add(rootObject);

            return rootObject;
        }

        /// <summary>
        /// Generates a key for a resource contained in an imported object graph.
        /// </summary>
        /// <param name="resourceClass">The type of the resource (defined by importer).</param>
        /// <param name="resourceID">The id of the resource (defined by importer)</param>
        public NamedOrGenericKey GetResourceKey(string resourceClass, string resourceID)
        {
            return new NamedOrGenericKey(
                "Imported." + m_importID + "." + resourceClass + "." + resourceID);
        }

        /// <summary>
        /// Gets a collection containing all imported objects.
        /// </summary>
        public List<SceneObject> Objects { get; }

        /// <summary>
        /// Gets the hierarchy information of the imported objects.
        /// </summary>
        public List<Tuple<SceneObject, SceneObject>> ParentChildRelationships { get; }

        /// <summary>
        /// Gets a collection containing all imported resources.
        /// </summary>
        public List<ImportedResourceInfo> ImportedResources { get; }

        /// <summary>
        /// Should triangle order be changes by the import logic?
        /// (This property is handled by the importer)
        /// </summary>
        public bool ChangeTriangleOrder => m_importOptions.IsChangeTriangleOrderNeeded();

        /// <summary>
        /// The resize factor for imported geometry.
        /// (This property is handled by the importer)
        /// </summary>
        public float ResizeFactor => m_importOptions.ResizeFactor;
    }
}