using System;
using System.Collections.Generic;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public class EngineOutputInfo
    {
        private const string TRANSLATABLE_GROUP_COMMON_OUTPUT_INFO = "Common output information";

        private OutputDescription _outputDescription;

        /// <summary>
        /// Gets the name of the output device.
        /// </summary>
        public string DeviceName => _outputDescription.DeviceName;

        public int AdapterIndex { get; }

        public int OutputIndex { get; }

        public bool IsAttachedToDesktop => _outputDescription.IsAttachedToDesktop;

        /// <summary>
        /// Gets the total count of pixels on X axis.
        /// </summary>
        public int DesktopWidth => _outputDescription.DesktopBounds.Right - _outputDescription.DesktopBounds.Left;

        /// <summary>
        /// Gets the total count of pixels on Y axis.
        /// </summary>
        public int DesktopHeight => _outputDescription.DesktopBounds.Bottom - _outputDescription.DesktopBounds.Top;

        public int DesktopXPos => _outputDescription.DesktopBounds.Left;

        public int DesktopYPos => _outputDescription.DesktopBounds.Top;

        public string DesktopResolution =>
            _outputDescription.DesktopBounds.Right - _outputDescription.DesktopBounds.Left +
            "x" +
            (_outputDescription.DesktopBounds.Bottom - _outputDescription.DesktopBounds.Top);

        public string DesktopLocation => "X = " + _outputDescription.DesktopBounds.Left + ", Y = " + _outputDescription.DesktopBounds.Top;

        public string Rotation => _outputDescription.Rotation.ToString();

        public EngineOutputModeInfo DefaultMode => this.SupportedModes[0];

        public EngineOutputModeInfo[] SupportedModes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineOutputInfo" /> class.
        /// </summary>
        internal EngineOutputInfo(int adapterIndex, int outputIndex, Output output)
        {
            this.AdapterIndex = adapterIndex;
            this.OutputIndex = outputIndex;
            _outputDescription = output.Description;

            // Get all supported modes
            var modes = output.GetDisplayModeList(
                GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT,
                DisplayModeEnumerationFlags.Interlaced);

            // Convert and sort them
            var engineModes = new EngineOutputModeInfo[modes.Length];
            for (var loop = 0; loop < engineModes.Length; loop++)
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

            for (var loop = engineModes.Length - 1; loop > -1; loop--)
            {
                if (!engineModes[loop].Equals(lastOutputMode))
                {
                    lastOutputMode = engineModes[loop];
                    strippedModeList.Add(lastOutputMode);
                }
            }

            // Store mode list
            this.SupportedModes = strippedModeList.ToArray();
        }
    }
}