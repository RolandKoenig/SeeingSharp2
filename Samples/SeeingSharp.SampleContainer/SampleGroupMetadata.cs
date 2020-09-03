/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

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