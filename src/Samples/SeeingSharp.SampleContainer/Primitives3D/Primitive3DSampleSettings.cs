using System.ComponentModel;

namespace SeeingSharp.SampleContainer.Primitives3D
{
    public class Primitive3DSampleSettings : SampleSettingsWith3D
    {
        public const string CATEGORY_NAME = "Primitive";

        private bool _textured;
        private bool _animated = true;

        [Category(CATEGORY_NAME)]
        public bool Textured
        {
            get => _textured;
            set
            {
                if (_textured != value)
                {
                    _textured = value;
                    this.RaiseRecreateRequest();
                }
            }
        }

        [Category(CATEGORY_NAME)]
        public bool Animated
        {
            get => _animated;
            set
            {
                if (_animated != value)
                {
                    _animated = value;
                    this.RaiseRecreateRequest();
                }
            }
        }
    }
}
