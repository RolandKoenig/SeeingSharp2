using System.Collections.Generic;

namespace SeeingSharp.SampleContainer
{
    public class SampleGroupMetadata
    {
        private static readonly Dictionary<string, char> _sampleGroupIcons = new Dictionary<string, char>();

        public string GroupName
        {
            get;
        }

        public List<SampleMetadata> Samples
        {
            get;
        }

        public string IconFontFamily => "Segoe MDL2 Assets";

        public char IconFontGlyph
        {
            get
            {
                if (_sampleGroupIcons.TryGetValue(this.GroupName, out var glyphCode))
                {
                    return glyphCode;
                }
                return ' ';
            }
        }

        static SampleGroupMetadata()
        {
            _sampleGroupIcons[nameof(Basics2D)] = (char) 0xE80A;        // TiltDown
            _sampleGroupIcons[nameof(Basics3D)] = (char) 0xE809;        // TiltUp
            _sampleGroupIcons[nameof(MassScenes)] = (char) 0xE909;      // World
            _sampleGroupIcons[nameof(Postprocessing)] = (char) 0xE81E;  // Map Layers
            _sampleGroupIcons[nameof(Primitives3D)] = (char) 0xE879;    // RoamingDomestic
        }

        public SampleGroupMetadata(string groupName)
        {
            this.Samples = new List<SampleMetadata>();

            this.GroupName = groupName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.GroupName;
        }
    }
}