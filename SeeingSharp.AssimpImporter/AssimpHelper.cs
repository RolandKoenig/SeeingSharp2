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

namespace SeeingSharp.AssimpImporter
{
    internal static class AssimpHelper
    {
        public static Matrix4x4 MatrixFromAssimp(Assimp.Matrix4x4 assimpMatrix)
        {
            return new Matrix4x4(
                assimpMatrix.A1, assimpMatrix.A2, assimpMatrix.A3, assimpMatrix.A4,
                assimpMatrix.B1, assimpMatrix.B2, assimpMatrix.B3, assimpMatrix.B4,
                assimpMatrix.C1, assimpMatrix.C2, assimpMatrix.C3, assimpMatrix.C4,
                assimpMatrix.D1, assimpMatrix.D2, assimpMatrix.D3, assimpMatrix.D4);
        }

        public static Vector3 Vector3FromAssimp(Assimp.Vector3D assimpVector)
        {
            return new Vector3(assimpVector.X, assimpVector.Y, assimpVector.Z);
        }

        public static Vector2 Vector2FromAssimp(Assimp.Vector2D assimpVector)
        {
            return new Vector2(assimpVector.X, assimpVector.Y);
        }

        public static Vector2 Vector2FromAssimp(Assimp.Vector3D assimpVector)
        {
            return new Vector2(assimpVector.X, assimpVector.Y);
        }

        public static Color4 Color4FromAssimp(Assimp.Color4D assimpColor)
        {
            return new Color4(assimpColor.R, assimpColor.G, assimpColor.B, assimpColor.A);
        }
    }
}
