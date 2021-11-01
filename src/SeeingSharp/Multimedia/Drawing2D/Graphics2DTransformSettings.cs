using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public struct Graphics2DTransformSettings
    {
        public static readonly Graphics2DTransformSettings Default = new Graphics2DTransformSettings
        {
            TransformMode = Graphics2DTransformMode.Custom,
            VirtualScreenSize = new Size2F(),
            CustomTransform = Matrix3x2.Identity
        };

        public Graphics2DTransformMode TransformMode;
        public Size2F VirtualScreenSize;
        public Matrix3x2 CustomTransform;
    }
}
