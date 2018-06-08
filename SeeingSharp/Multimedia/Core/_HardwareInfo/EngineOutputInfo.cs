#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineOutputInfo 
    {
        private const string TRANSLATABLE_GROUP_COMMON_OUTPUT_INFO = "Common output information";

        private int m_adapterIndex;
        private int m_outputIndex;
        private DXGI.OutputDescription m_outputDescription;
        private EngineOutputModeInfo[] m_outputInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineOutputInfo" /> class.
        /// </summary>
        internal EngineOutputInfo(int adapterIndex, int outputIndex, DXGI.Output output)
        {
            m_adapterIndex = adapterIndex;
            m_outputIndex = outputIndex;
            m_outputDescription = output.Description;

            // Get all supported modes
            DXGI.ModeDescription[] modes = output.GetDisplayModeList(
                GraphicsHelper.DEFAULT_TEXTURE_FORMAT,
                DXGI.DisplayModeEnumerationFlags.Interlaced);

            // Convert and sort them
            EngineOutputModeInfo[] engineModes = new EngineOutputModeInfo[modes.Length];
            for(int loop=0; loop<engineModes.Length; loop++)
            {
                engineModes[loop] = new EngineOutputModeInfo(this, modes[loop]);
            }
            Array.Sort(engineModes, (left, right) =>
            {
                int result = left.PixelCount.CompareTo(right.PixelCount);
                if (result == 0) { result = left.RefreshRateNumerator.CompareTo(right.RefreshRateNumerator); }
                return result;
            });

            // Strip them (we want to have each relevant mode once)
            List<EngineOutputModeInfo> strippedModeList = new List<EngineOutputModeInfo>(engineModes.Length);
            EngineOutputModeInfo lastOutputMode = new EngineOutputModeInfo();
            for (int loop=engineModes.Length - 1; loop > -1; loop--)
            {
                if(!engineModes[loop].Equals(lastOutputMode))
                {
                    lastOutputMode = engineModes[loop];
                    strippedModeList.Add(lastOutputMode);
                }
            }

            // Store mode list
            m_outputInfos = strippedModeList.ToArray();
        }

        /// <summary>
        /// Gets the name of the output device.
        /// </summary>
        public string DeviceName
        {
            get { return m_outputDescription.DeviceName; }
        }

        public int AdapterIndex
        {
            get { return m_adapterIndex; }
        }

        public int OutputIndex
        {
            get { return m_outputIndex; }
        }

        public bool IsAttachedToDesktop
        {
            get { return m_outputDescription.IsAttachedToDesktop; }
        }

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

        public string DesktopResolution
        {
            get
            {
                return 
                    (m_outputDescription.DesktopBounds.Right - m_outputDescription.DesktopBounds.Left) + 
                    "x" +
                    (m_outputDescription.DesktopBounds.Bottom - m_outputDescription.DesktopBounds.Top);
            }
        }

        public string DesktopLocation
        {
            get
            {
                return "X = " + m_outputDescription.DesktopBounds.Left + ", Y = " + m_outputDescription.DesktopBounds.Top;
            }
        }

        public string Rotation
        {
            get { return m_outputDescription.Rotation.ToString(); }
        }

        public EngineOutputModeInfo DefaultMode
        {
            get { return m_outputInfos[0]; }
        }

        public EngineOutputModeInfo[] SupportedModes
        {
            get { return m_outputInfos; }
        }
    }
}
