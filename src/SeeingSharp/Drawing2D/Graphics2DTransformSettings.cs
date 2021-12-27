using System.Drawing;
using System.Numerics;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing2D
{
    public struct Graphics2DTransformSettings
    {
        public static readonly Graphics2DTransformSettings Default = new Graphics2DTransformSettings
        {
            TransformMode = Graphics2DTransformMode.Custom,
            VirtualScreenSize = new SizeF(),
            CustomTransform = Matrix3x2.Identity
        };

        public Graphics2DTransformMode TransformMode;
        public SizeF VirtualScreenSize;
        public Matrix3x2 CustomTransform;
    }
}
