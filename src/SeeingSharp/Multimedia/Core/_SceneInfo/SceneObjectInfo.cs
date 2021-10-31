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
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneObjectInfo
    {
        public SceneObject OriginalObject { get; }

        public SceneObjectInfoType Type { get; }

        /// <summary>
        /// Gets or sets the name of this node.
        /// The name ist just meta information and has no relevance for SeeingSharp.
        /// </summary>
        public string Name => this.OriginalObject.Name;

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag1 => this.OriginalObject.Tag1;

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag2 => this.OriginalObject.Tag2;

        /// <summary>
        /// Queries for all children of this object.
        /// </summary>
        public IEnumerable<SceneObjectInfo> Children
        {
            get
            {
                foreach (var actChildObject in this.OriginalObject.GetAllChildren(false))
                {
                    yield return new SceneObjectInfo(actChildObject);
                }
            }
        }

        public bool IsAssociatedToScene => this.OriginalObject.Scene != null;

        public Scene AssociatedScene => this.OriginalObject.Scene;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObjectInfo"/> class.
        /// </summary>
        /// <param name="obj">The object for which to build this info object.</param>
        public SceneObjectInfo(SceneObject obj)
        {
            obj.EnsureNotNull(nameof(obj));

            this.OriginalObject = obj;

            // Set the type of this object
            this.Type = SceneObjectInfoType.Other;
            var clrType = obj.GetType();

            if (clrType == typeof(Mesh))
            {
                this.Type = SceneObjectInfoType.Mesh;
            }
            else if (clrType == typeof(ScenePivotObject))
            {
                this.Type = SceneObjectInfoType.Pivot;
            }
            else if (clrType == typeof(FullscreenTexture))
            {
                this.Type = SceneObjectInfoType.FullscreenTexture;
            }
            else if (clrType == typeof(Skybox))
            {
                this.Type = SceneObjectInfoType.FullscreenTexture;
            }
            else if(clrType == typeof(WireObject))
            {
                this.Type = SceneObjectInfoType.WireObject;
            }
        }

        public override string ToString()
        {
            return $"Type:{this.Type}, #Children:{this.OriginalObject.CountChildren}";
        }
    }
}
