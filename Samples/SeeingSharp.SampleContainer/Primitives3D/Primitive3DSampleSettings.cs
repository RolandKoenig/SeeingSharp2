using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SeeingSharp.SampleContainer.Primitives3D
{
    public class Primitive3DSampleSettings : SampleSettingsWith3D
    {
        public const string CATEGORY_NAME = "Primitive";

        private bool m_textured = false;
        private bool m_animated = true;

        [Category(CATEGORY_NAME)]
        public bool Textured
        {
            get => m_textured;
            set
            {
                if (m_textured != value)
                {
                    m_textured = value;
                    base.RaiseRecreateRequest();
                }
            }
        }

        [Category(CATEGORY_NAME)]
        public bool Animated
        {
            get => m_animated;
            set
            {
                if (m_animated != value)
                {
                    m_animated = value;
                    base.RaiseRecreateRequest();
                }
            }
        }
    }
}
