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
using System.Numerics;

namespace SeeingSharp.Multimedia.Components
{
    public class FocusedPointCameraComponent : FocusedCameraComponent
    {
        private Vector3 _focusedLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedPointCameraComponent"/> class.
        /// </summary>
        public FocusedPointCameraComponent()
        {
            _focusedLocation = Vector3.Zero;
        }

        public FocusedPointCameraComponent(Vector3 focusedLocation)
        {
            _focusedLocation = focusedLocation;
        }

        protected override Vector3 GetFocusedLocation()
        {
            return _focusedLocation;
        }

        public float FocusedPointX
        {
            get => _focusedLocation.X;
            set => _focusedLocation.X = value;
        }

        public float FocusedPointY
        {
            get => _focusedLocation.Y;
            set => _focusedLocation.Y = value;
        }

        public float FocusedPointZ
        {
            get => _focusedLocation.Z;
            set => _focusedLocation.Z = value;
        }
    }
}
