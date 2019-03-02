#region License information
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
#endregion
namespace SeeingSharp.Multimedia.Core
{
    public class PickingInformation
    {
        private SceneObject m_pickedObject;
        private float m_distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickingInformation" /> class.
        /// </summary>
        public PickingInformation()
        {
            m_pickedObject = null;
            m_distance = float.NaN;
        }

        /// <summary>
        /// Notifies a pick for the given object with the given distance.
        /// </summary>
        /// <param name="pickedObject">The object that was picked.</param>
        /// <param name="distance">The distance from the origin to the picked point.</param>
        public void NotifyPick(SceneObject pickedObject, float distance)
        {
            if ((float.IsNaN(m_distance)) ||
                (distance < m_distance))
            {
                m_distance = distance;
                m_pickedObject = pickedObject;
            }
        }

        /// <summary>
        /// The picked object.
        /// </summary>
        public SceneObject PickedObject
        {
            get { return m_pickedObject; }
        }


        /// <summary>
        /// Gets the distance to the picked object.
        /// </summary>
        public float Distance
        {
            get { return m_distance; }
        }
    }
}
