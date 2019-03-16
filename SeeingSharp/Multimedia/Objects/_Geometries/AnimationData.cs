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

using System.Runtime.InteropServices;

namespace SeeingSharp.Multimedia.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationData
    {
        /// <summary>
        /// Gets or sets the first bone index.
        /// </summary>
        public uint BoneIndex01;

        /// <summary>
        /// Gets or sets the second bone index.
        /// </summary>
        public uint BoneIndex02;

        /// <summary>
        /// Gets or sets the third bone index.
        /// </summary>
        public uint BoneIndex03;

        /// <summary>
        /// Gets or sets the fourth bone index.
        /// </summary>
        public uint BoneIndex04;

        /// <summary>
        /// Gets or sets the first bone weight.
        /// </summary>
        public float BoneWeight01;

        /// <summary>
        /// Gets or sets the second bone weight.
        /// </summary>
        public float BoneWeight02;

        /// <summary>
        /// Gets or sets the third bone weight.
        /// </summary>
        public float BoneWeight03;

        /// <summary>
        /// Gets or sets the fourth bone weight.
        /// </summary>
        public float BoneWeight04;
    }
}