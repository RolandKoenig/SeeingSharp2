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

#region using

using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using System.Runtime.InteropServices;
    using SharpDX;

    #endregion

    /// <summary>
    /// A structure for sending line vertex data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LineVertex
    {
        #region Constants
        public static readonly int Size = Marshal.SizeOf<LineVertex>();
        public static readonly D3D11.InputElement[] InputElements =
        {
            new D3D11.InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0)
        };
        #endregion

        #region Vertex data
        public Vector3 Position;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LineVertex" /> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public LineVertex(Vector3 position)
        {
            Position = position;
        }
    }
}