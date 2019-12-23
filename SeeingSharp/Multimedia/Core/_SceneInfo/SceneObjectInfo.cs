/*
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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Objects;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneObjectInfo
    {
        private List<SceneObjectInfo> m_children;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObjectInfo"/> class.
        /// </summary>
        /// <param name="obj">The object for which to build this info object.</param>
        /// <param name="buildFullChildTree">True to build a full child tree automatically.</param>
        internal SceneObjectInfo(SceneObject obj, bool buildFullChildTree = true)
        {
            obj.EnsureNotNull(nameof(obj));
            obj.Scene.EnsureNotNull($"{nameof(obj)}.{nameof(obj.Scene)}");

            this.OriginalObject = obj;

            // Build child list
            m_children = new List<SceneObjectInfo>(obj.CountChildren);

            if (buildFullChildTree)
            {
                foreach (var actChildObject in obj.GetAllChildrenInternal())
                {
                    m_children.Add(new SceneObjectInfo(actChildObject));
                }
            }

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
        }

        public override string ToString()
        {
            return $"Type:{this.Type}, #Children:{this.Children.Count}";
        }

        public SceneObject OriginalObject { get; }

        public SceneObjectInfoType Type { get; }

        public IReadOnlyList<SceneObjectInfo> Children => m_children;
    }
}
