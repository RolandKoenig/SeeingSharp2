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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneObjectInfo
    {
        private SceneObject m_sceneObject;
        private SceneObjectInfoType m_sceneObjectType;
        private List<SceneObjectInfo> m_childs;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObjectInfo"/> class.
        /// </summary>
        /// <param name="obj">The object for which to build this info object.</param>
        /// <param name="buildFullChildTree">True to build a full child tree automatically.</param>
        internal SceneObjectInfo(SceneObject obj, bool buildFullChildTree = true)
        {
            obj.EnsureNotNull(nameof(obj));
            obj.Scene.EnsureNotNull($"{nameof(obj)}.{nameof(obj.Scene)}");

            m_sceneObject = obj;

            // Build child list
            m_childs = new List<SceneObjectInfo>(obj.CountChildren);
            if(buildFullChildTree)
            {
                foreach(SceneObject actChildObject in obj.GetAllChildrenInternal())
                {
                    m_childs.Add(new SceneObjectInfo(actChildObject));
                }
            }

            // Set the type of this object
            m_sceneObjectType = SceneObjectInfoType.Other;
            Type clrType = obj.GetType();
            if(clrType == typeof(GenericObject)) { m_sceneObjectType = SceneObjectInfoType.GenericObject; }
            else if(clrType == typeof(ScenePivotObject)) { m_sceneObjectType = SceneObjectInfoType.Pivot; }
        }

        public override string ToString()
        {
            return $"Type:{this.Type}, #Childs:{this.Childs.Count}";
        }

        public SceneObject OriginalObject
        {
            get { return m_sceneObject; }
        }

        public SceneObjectInfoType Type
        {
            get { return m_sceneObjectType; }
        }

        public IReadOnlyList<SceneObjectInfo> Childs
        {
            get { return m_childs; }
        }
    }
}
