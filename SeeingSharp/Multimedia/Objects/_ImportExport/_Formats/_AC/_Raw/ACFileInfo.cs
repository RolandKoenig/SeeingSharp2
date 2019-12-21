using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Objects
{
    internal class ACFileInfo
    {
        public ACFileInfo()
        {
            Materials = new List<ACMaterialInfo>();
            Objects = new List<ACObjectInfo>();
        }

        /// <summary>
        /// Counts all objects within this file
        /// </summary>
        public int CountAllObjects()
        {
            var result = 0;

            foreach (var actObj in Objects)
            {
                result++;
                result += actObj.CountAllChildObjects();
            }

            return result;
        }

        public List<ACMaterialInfo> Materials;
        public List<ACObjectInfo> Objects;
    }
}
