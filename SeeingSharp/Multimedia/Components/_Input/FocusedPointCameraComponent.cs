#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SeeingSharp.Multimedia.Components
{
    public class FocusedPointCameraComponent : FocusedCameraComponent
    {
        private Vector3 m_focusedLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedPointCameraComponent"/> class.
        /// </summary>
        public FocusedPointCameraComponent()
        {
            m_focusedLocation = Vector3.Zero;
        }

        protected override Vector3 GetFocusedLocation()
        {
            return m_focusedLocation;
        }

        public float FocusedPointX
        {
            get => m_focusedLocation.X;
            set => m_focusedLocation.X = value;
        }

        public float FocusedPointY
        {
            get => m_focusedLocation.Y;
            set => m_focusedLocation.Y = value;
        }

        public float FocusedPointZ
        {
            get => m_focusedLocation.Z;
            set => m_focusedLocation.Z = value;
        }
    }
}
