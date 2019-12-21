using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Objects
{
    internal class ACMaterialInfo
    {
        public CommonMaterialProperties CreateMaterialProperties(int materialIndex)
        {
            var result = new CommonMaterialProperties
            {
                DiffuseColor = Diffuse,
                AmbientColor = Ambient,
                EmissiveColor = Emissive,
                SpecularColor = Specular,
                Shininess = Shininess,
                Name = $"{materialIndex}-{this.Name}"
            };

            return result;
        }

        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Emissive;
        public string Name;
        public float Shininess;
        public Color4 Specular;
    }
}
