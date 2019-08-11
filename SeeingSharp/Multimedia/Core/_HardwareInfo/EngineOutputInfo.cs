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
using System;
using System.Collections.Generic;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineOutputInfo
    {
        private const string TRANSLATABLE_GROUP_COMMON_OUTPUT_INFO = "Common output information";

        private OutputDescription m_outputDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineOutputInfo" /> class.
        /// </summary>
        internal EngineOutputInfo(int adapterIndex, int outputIndex, Output output)
        {
            this.AdapterIndex = adapterIndex;
            this.OutputIndex = outputIndex;
            m_outputDescription = output.Description;

            // Get all supported modes
            var modes = output.GetDisplayModeList(
                GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                DisplayModeEnumerationFlags.Interlaced);

            // Convert and sort them
            var engineModes = new EngineOutputModeInfo[modes.Length];
            for(var loop=0; loop<engineModes.Length; loop++)
            {
                engineModes[loop] = new EngineOutputModeInfo(this, modes[loop]);
            }
            Array.Sort(engineModes, (left, right) =>
            {
                var result = left.PixelCount.CompareTo(right.PixelCount);
                if (result == 0) { result = left.RefreshRateNumerator.CompareTo(right.RefreshRateNumerator); }
                return result;
            });

            // Strip them (we want to have each relevant mode once)
            var strippedModeList = new List<EngineOutputModeInfo>(engineModes.Length);
            var lastOutputMode = new EngineOutputModeInfo();

            for (var loop =engineModes.Length - 1; loop > -1; loop--)
            {
                if(!engineModes[loop].Equals(lastOutputMode))
                {
                    lastOutputMode = engineModes[loop];
                    strippedModeList.Add(lastOutputMode);
                }
            }

            // Store mode list
            this.SupportedModes = strippedModeList.ToArray();
        }

        /// <summary>
        /// Gets the name of the output device.
        /// </summary>
        public string DeviceName => m_outputDescription.DeviceName;

        public int AdapterIndex { get; }

        public int OutputIndex { get; }

        public bool IsAttachedToDesktop => m_outputDescription.IsAttachedToDesktop;

        /// <summary>
        /// Gets the total count of pixels on X axis.
        /// </summary>
        public int DesktopWidth => m_outputDescription.DesktopBounds.Right - m_outputDescription.DesktopBounds.Left;

        /// <summary>
        /// Gets the total count of pixels on Y axis.
        /// </summary>
        public int DesktopHeight => m_outputDescription.DesktopBounds.Bottom - m_outputDescription.DesktopBounds.Top;

        public int DesktopXPos => m_outputDescription.DesktopBounds.Left;

        public int DesktopYPos => m_outputDescription.DesktopBounds.Top;

        public string DesktopResolution =>
            m_outputDescription.DesktopBounds.Right - m_outputDescription.DesktopBounds.Left +
            "x" +
            (m_outputDescription.DesktopBounds.Bottom - m_outputDescription.DesktopBounds.Top);

        public string DesktopLocation => "X = " + m_outputDescription.DesktopBounds.Left + ", Y = " + m_outputDescription.DesktopBounds.Top;

        public string Rotation => m_outputDescription.Rotation.ToString();

        public EngineOutputModeInfo DefaultMode => this.SupportedModes[0];

        public EngineOutputModeInfo[] SupportedModes { get; }
    }
}