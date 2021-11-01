using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal class ACFileInfo
    {
        public List<ACMaterialInfo> Materials;
        public List<ACObjectInfo> Objects;

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
    }
}
