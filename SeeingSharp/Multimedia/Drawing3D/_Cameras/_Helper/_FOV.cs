using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class FOV
    {
        //... default value for an aspect of 4:3 and an horizontal fov of 90°
        public const float defaultFOV = (float)Math.PI / 4.0f; //°

        /// <summary>
        /// Calculates the Field of View by focal length in respect to the current film gauge ... see http://www.bobatkins.com/photography/technical/field_of_view.html
        /// </summary>
        /// <param name="aspectRatio">width to its height of a image</param>
        /// <param name="filmGauge">physical property of photographic or motion picture film stock which defines its width</param>
        public static float CalculateFieldOfView(float aspectRatio, float filmGauge)
        {
            float filmHeight = filmGauge / (float)Math.Max(aspectRatio, 1);
            float focalLength = 0.5f * filmHeight / filmGauge;
            return 2.0f * (float)Math.Atan((filmHeight / 2.0f) / focalLength) * 180.0f / (float)Math.PI;
        }
    }
}
