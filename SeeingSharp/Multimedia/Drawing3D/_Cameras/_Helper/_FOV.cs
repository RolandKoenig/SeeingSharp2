using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class FOV
    {
        //... default value for an aspect of 4:3 and an horizontal fov of 90°
        public const float defaultFOV = (float)Math.PI / 4.0f; //°

        //... see wide high-resolution film gauge https://en.wikipedia.org/wiki/70_mm_film, human eye focal 50 mm, default 35 mm
        public const float defaultFilmGauge = 50; //mm

        /// <summary>
        /// Calculates the Field of View by focal length in respect to the current film gauge ... see http://www.bobatkins.com/photography/technical/field_of_view.html
        /// </summary>
        /// <param name="aspectRatio">width to its height of a image</param>
        public static float CalculateFieldOfView(float aspectRatio)
        {
            float filmHeight = FOV.defaultFilmGauge / (float)Math.Max(aspectRatio, 1);
            float filmGraident = (float)Math.Tan((Math.PI / 180) * 0.5f * FOV.defaultFOV);
            float focalLength = 0.5f * filmHeight / defaultFilmGauge;
            return 2.0f * (float)Math.Atan((filmHeight / 2.0f) / focalLength) * 180.0f / (float)Math.PI;
        }
    }
}
