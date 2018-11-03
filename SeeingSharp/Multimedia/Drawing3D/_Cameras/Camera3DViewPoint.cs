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
using SeeingSharp.Util;
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using SharpDX;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// This class represents a specific viewpoint within the 3D world.
    /// </summary>
    public class Camera3DViewPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DViewPoint"/> class.
        /// </summary>
        public Camera3DViewPoint()
        {
            this.OrthographicZoomFactor = 10f;
            this.CameraType = Camera3DType.Perspective;
        }

        ///// <summary>
        ///// Loads a Camera3DViewPoint from the given resource.
        ///// </summary>
        ///// <param name="resourceLink">The resource link.</param>
        //public static Camera3DViewPoint FromResourceLink(ResourceLink resourceLink)
        //{
        //    resourceLink.EnsureNotNull(nameof(resourceLink));

        //    XmlSerializer serializer = SerializerRepository.Current.GetSerializer(typeof(Camera3DViewPoint));
        //    using (Stream inStream = resourceLink.OpenInputStream())
        //    {
        //        return serializer.Deserialize(inStream) as Camera3DViewPoint;
        //    }
        //}

        ///// <summary>
        ///// Loads a Camera3DViewPoint from the given resource.
        ///// </summary>
        ///// <param name="resourceLink">The resource link.</param>
        //public static async Task<Camera3DViewPoint> FromResourceLinkAsync(ResourceLink resourceLink)
        //{
        //    resourceLink.EnsureNotNull(nameof(resourceLink));

        //    XmlSerializer serializer = SerializerRepository.Current.GetSerializer(typeof(Camera3DViewPoint));
        //    using (Stream inStream = await resourceLink.OpenInputStreamAsync())
        //    {
        //        return await Task.Run(() => serializer.Deserialize(inStream) as Camera3DViewPoint);
        //    }
        //}

        ///// <summary>
        ///// Writes this Camera3DViewPoint to the given resource link.
        ///// </summary>
        ///// <param name="resourceLink">The resource link.</param>
        //public void ToResourceLink(ResourceLink resourceLink)
        //{
        //    resourceLink.EnsureNotNull(nameof(resourceLink));

        //    XmlSerializer serializer = SerializerRepository.Current.GetSerializer(typeof(Camera3DViewPoint));
        //    using (Stream outStream = resourceLink.OpenOutputStream())
        //    {
        //        serializer.Serialize(outStream, this);
        //    }
        //}

        public Camera3DType CameraType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the CameraType in string form.
        /// </summary>
        public string CameraTypeString
        {
            get { return this.CameraType.ToString(); }
            set
            {
                Camera3DType valueParsed = Camera3DType.Perspective;
                if (Enum.TryParse(value, out valueParsed))
                {
                    this.CameraType = valueParsed;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of the ViewPoint.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation of the ViewPoint.
        /// </summary>
        public Vector2 Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the zoom factor if we have a orthographic camera.
        /// </summary>
        public float OrthographicZoomFactor
        {
            get;
            set;
        }
    }
}
