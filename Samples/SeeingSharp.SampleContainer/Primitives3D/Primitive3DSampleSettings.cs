using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SeeingSharp.SampleContainer.Primitives3D
{
    public class Primitive3DSampleSettings : SampleSettingsWith3D
    {
        public const string CATEGORY_NAME = "Primitive";

        private bool _textured = false;
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
                    base.RaiseRecreateRequest();
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
                    base.RaiseRecreateRequest();
                }
            }
        }
    }
}
