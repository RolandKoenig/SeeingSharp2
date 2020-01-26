﻿/*
    Seeing# and all applications distributed together with it. 
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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// Container for imported model data.
    /// </summary>
    public class ImportedModelContainer
    {
        // Static id counter 
        private static int s_maxContainerID;

        // All model data
        private int m_importID;
        private ImportOptions m_importOptions;
        private List<SceneObject> m_objects;
        private List<ParentChildRelationship> m_parentChildRelationships;
        private List<ImportedResourceInfo> m_importedResources;

        // State
        private bool m_isFinished;
        private bool m_isValid;
        private Exception m_finishException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportedModelContainer" /> class.
        /// </summary>
        public ImportedModelContainer(ImportOptions importOptions)
        {
            m_importOptions = importOptions;
            m_objects = new List<SceneObject>();
            m_parentChildRelationships = new List<ParentChildRelationship>();
            m_importedResources = new List<ImportedResourceInfo>();
            m_importID = Interlocked.Increment(ref s_maxContainerID);
        }

        /// <summary>
        /// Creates and adds the root for all imported scene objects.
        /// </summary>
        public void FinishLoading(BoundingBox boundingBox)
        {
            if (m_isFinished)
            {
                throw new SeeingSharpException("ModelContainer already finished!");
            }

            try
            {
                // Generic checks
                if (this.Objects.Count == 0)
                {
                    throw new SeeingSharpException("No objects imported");
                }
                if (EngineMath.EqualsWithTolerance(boundingBox.Width, 0) ||
                    EngineMath.EqualsWithTolerance(boundingBox.Height, 0) ||
                    EngineMath.EqualsWithTolerance(boundingBox.Depth, 0))
                {
                    throw new SeeingSharpException($"BoundingBox of the loaded model data seems to be empty (Width={boundingBox.Width}, Height={boundingBox.Height}, Depth={boundingBox.Height}");
                }

                // Create root for the imported object graph
                var rootObject = new ScenePivotObject();
                rootObject.TransformationType = SpacialTransformationType.ScalingTranslationEulerAngles;

                // Configure base transformation of the root object
                switch (m_importOptions.ResourceCoordinateSystem)
                {
                    case CoordinateSystem.LeftHanded_UpY:
                        break;

                    case CoordinateSystem.LeftHanded_UpZ:
                        rootObject.Scaling = new Vector3(1f, -1f, 1f);
                        rootObject.RotationEuler = new Vector3(-EngineMath.RAD_90DEG, 0f, 0f);
                        break;

                    case CoordinateSystem.RightHanded_UpY:
                        rootObject.Scaling = new Vector3(1f, 1f, -1f);
                        break;

                    case CoordinateSystem.RightHanded_UpZ:
                        rootObject.Scaling = new Vector3(-1f, 1f, -1f);
                        rootObject.RotationEuler = new Vector3(EngineMath.RAD_90DEG, 0f, 0f);
                        break;
                }

                // Configure position and scaling of the root object
                if (m_importOptions.FitToCube)
                {
                    var scaleFactor = Math.Min(
                        (1f / boundingBox.Width),
                        Math.Min((1f / boundingBox.Height), (1f / boundingBox.Depth)));
                    rootObject.Scaling *= scaleFactor;
                    rootObject.Position = new Vector3(
                        (0f - (boundingBox.Minimum.X + (boundingBox.Maximum.X - boundingBox.Minimum.X) / 2f)) *
                        scaleFactor,
                        (0f - (boundingBox.Minimum.Y + (boundingBox.Maximum.Y - boundingBox.Minimum.Y) / 2f)) *
                        scaleFactor,
                        (0f - (boundingBox.Minimum.Z + (boundingBox.Maximum.Z - boundingBox.Minimum.Z) / 2f)) *
                        scaleFactor);
                }

                // Find current root objects and assign them as child to the new root object
                foreach (var actRootObject in this.FindRootObjects())
                {
                    this.ParentChildRelationships.Add(
                        new ParentChildRelationship(rootObject, actRootObject));
                }

                // AddObject the object finally
                this.Objects.Add(rootObject);

                m_isValid = true;
            }
            catch (Exception ex)
            {
                m_finishException = ex;
                m_isValid = false;
            }
            finally
            {
                m_isFinished = true;
            }
        }

        /// <summary>
        /// Search for root objects (objects with no parents).
        /// </summary>
        private IEnumerable<SceneObject> FindRootObjects()
        {
            foreach(var actObject in m_objects)
            {
                var isRoot = true;
                foreach(var actParentChildRelationship in m_parentChildRelationships)
                {
                    if (actObject == actParentChildRelationship.Child)
                    {
                        isRoot = false;
                        break;
                    }
                }

                if (isRoot)
                {
                    yield return actObject;
                }
            }
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
        public IList<SceneObject> Objects => m_objects;

        /// <summary>
        /// Gets the hierarchy information of the imported objects.
        /// </summary>
        public IList<ParentChildRelationship> ParentChildRelationships => m_parentChildRelationships;

        /// <summary>
        /// Gets a collection containing all imported resources.
        /// </summary>
        public IList<ImportedResourceInfo> ImportedResources => m_importedResources;

        public bool IsFinished => m_isFinished;

        public bool IsValid => m_isValid;

        public Exception FinishException => m_finishException;
    }
}