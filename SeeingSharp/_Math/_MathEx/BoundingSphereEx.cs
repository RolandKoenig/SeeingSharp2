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

namespace SeeingSharp
{
    #region using

    using SharpDX;

    #endregion

    public static class BoundingSphereEx
    {
        public static void Transform(this BoundingSphere boundingSphere, Matrix matrix)
        {
            Vector3 center = boundingSphere.Center;
            Vector3 otherPoint = center + new Vector3(boundingSphere.Radius, 0f, 0f);

            boundingSphere.Center = Vector3.Transform(center, matrix).ToXYZ();
            boundingSphere.Radius = (Vector3.Transform(otherPoint, matrix).ToXYZ() - boundingSphere.Center).Length();
        }
    }
}