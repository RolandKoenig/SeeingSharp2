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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class TextFormatResource : Drawing2DResourceBase
    {
        // Fixed resource parameters (passed on constructor)
        private DWrite.TextFormat[] _loadedTextFormats;
        private string _fontFamilyName;
        private float _fontSize;
        private FontWeight _fontWeight;
        private FontStyle _fontStyle;
        private FontStretch _fontStretch;

        // Dynamic runtime parameters (possible to pass on each render call)
        private bool[] _runtimeDataChangedFlags;
        private ParagraphAlignment _paragraphAlignment;
        private TextAlignment _textAlignment;
        private WordWrapping _wordWrapping;
        private ReadingDirection _readingDirection;

        /// <summary>
        /// Gets or sets the alignment of the paragraph.
        /// </summary>
        public ParagraphAlignment ParagraphAlignment
        {
            get => _paragraphAlignment;
            set
            {
                var castedValue = value;

                if (castedValue != _paragraphAlignment)
                {
                    _paragraphAlignment = castedValue;
                    _runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the paragraph.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                var castedValue = value;

                if (castedValue != _textAlignment)
                {
                    _textAlignment = castedValue;
                    _runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the WordWrapping mode.
        /// </summary>
        public WordWrapping WordWrapping
        {
            get => _wordWrapping;
            set
            {
                var castedValue = value;

                if (castedValue != _wordWrapping)
                {
                    _wordWrapping = castedValue;
                    _runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the reading direction.
        /// </summary>
        public ReadingDirection ReadingDirection
        {
            get => _readingDirection;
            set
            {
                var castedValue = value;

                if (castedValue != _readingDirection)
                {
                    _readingDirection = castedValue;
                    _runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFormatResource"/> class.
        /// </summary>
        /// <param name="fontFamilyName">Name of the font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontWeight">The weight of the font.</param>
        /// <param name="fontStretch">The stretch parameter for the font.</param>
        /// <param name="fontStyle">The style parameter for the font.</param>
        public TextFormatResource(
            string fontFamilyName, float fontSize,
            FontWeight fontWeight = FontWeight.Normal,
            FontStyle fontStyle = FontStyle.Normal,
            FontStretch fontStretch = FontStretch.Normal)
        {
            _loadedTextFormats = new DWrite.TextFormat[GraphicsCore.Current.DeviceCount];
            _runtimeDataChangedFlags = new bool[GraphicsCore.Current.DeviceCount];
            _fontFamilyName = fontFamilyName;
            _fontSize = fontSize;
            _fontWeight = fontWeight;
            _fontStyle = fontStyle;
            _fontStretch = fontStretch;

            _paragraphAlignment = ParagraphAlignment.Near; //ParagraphAlignment.Near;
            _textAlignment = TextAlignment.Leading;
            _wordWrapping = WordWrapping.Wrap;
            _readingDirection = ReadingDirection.LeftToRight;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var textFormat = _loadedTextFormats[engineDevice.DeviceIndex];

            if (textFormat != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(textFormat);
                _loadedTextFormats[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the TextFormat object for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal DWrite.TextFormat GetTextFormat(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = _loadedTextFormats[engineDevice.DeviceIndex];
            if (result == null)
            {
                // Load the TextFormat object
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    _fontFamilyName,
                    (DWrite.FontWeight)_fontWeight, (DWrite.FontStyle)_fontStyle, (DWrite.FontStretch)_fontStretch, _fontSize);
                _loadedTextFormats[engineDevice.DeviceIndex] = result;
                engineDevice.RegisterDeviceResource(this);
            }

            // Update runtime values on demand
            if (_runtimeDataChangedFlags[engineDevice.DeviceIndex])
            {
                _runtimeDataChangedFlags[engineDevice.DeviceIndex] = false;

                SeeingSharpUtil.DisposeObject(result);
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    _fontFamilyName,
                    (DWrite.FontWeight)_fontWeight, (DWrite.FontStyle)_fontStyle, (DWrite.FontStretch)_fontStretch, _fontSize);
                result.TextAlignment = (DWrite.TextAlignment)_textAlignment;
                _loadedTextFormats[engineDevice.DeviceIndex] = result;
            }

            return result;
        }
    }
}