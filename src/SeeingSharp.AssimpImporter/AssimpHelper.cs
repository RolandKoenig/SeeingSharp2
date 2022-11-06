using System.Numerics;
using Assimp;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using Matrix4x4 = System.Numerics.Matrix4x4;

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

        public static Vector3 Vector3FromAssimp(Vector3D assimpVector)
        {
            return new Vector3(assimpVector.X, assimpVector.Y, assimpVector.Z);
        }

        public static Vector2 Vector2FromAssimp(Vector2D assimpVector)
        {
            return new Vector2(assimpVector.X, assimpVector.Y);
        }

        public static Vector2 Vector2FromAssimp(Vector3D assimpVector)
        {
            return new Vector2(assimpVector.X, assimpVector.Y);
        }

        public static Vector2 TextureCoord2FromAssimp(Vector3D assimpVector)
        {
            return new Vector2(assimpVector.X, -assimpVector.Y);
        }

        public static Color4 Color4FromAssimp(Color4D assimpColor)
        {
            return new Color4(assimpColor.R, assimpColor.G, assimpColor.B, assimpColor.A);
        }

        public static SeeingSharpTextureAddressMode GetTextureAddressMode(TextureWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case TextureWrapMode.Clamp:
                    return SeeingSharpTextureAddressMode.Clamp;

                case TextureWrapMode.Decal:
                    return SeeingSharpTextureAddressMode.Border;

                case TextureWrapMode.Mirror:
                    return SeeingSharpTextureAddressMode.Mirror;

                case TextureWrapMode.Wrap:
                default:
                    return SeeingSharpTextureAddressMode.Wrap;
            }
        }
    }
}
