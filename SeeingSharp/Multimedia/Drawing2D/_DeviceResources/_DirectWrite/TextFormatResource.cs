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
using D2D = SharpDX.Direct2D1;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class TextFormatResource : Drawing2DResourceBase
    {
        // Fixed resource parameters (passed on constructor)
        private DWrite.TextFormat[] m_loadedTextFormats;
        private string m_fontFamilyName;
        private float m_fontSize;
        private DWrite.FontWeight m_fontWeight;
        private DWrite.FontStyle m_fontStyle;
        private DWrite.FontStretch m_fontStretch;

        // Dynamic runtime parameters (possible to pass on each render call)
        private bool[] m_runtimeDataChangedFlags;
        private DWrite.ParagraphAlignment m_paragraphAlignment;
        private DWrite.TextAlignment m_textAlignment;
        private DWrite.WordWrapping m_wordWrapping;
        private DWrite.ReadingDirection m_readingDirection;

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var textFormat = m_loadedTextFormats[engineDevice.DeviceIndex];

            if (textFormat != null)
            {
                SeeingSharpTools.DisposeObject(textFormat);
                m_loadedTextFormats[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the TextFormat object for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal DWrite.TextFormat GetTextFormat(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var result = m_loadedTextFormats[engineDevice.DeviceIndex];

            if (result == null)
            {
                // Load the TextFormat object
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    m_fontFamilyName,
                    m_fontWeight, m_fontStyle, m_fontStretch, m_fontSize);
                m_loadedTextFormats[engineDevice.DeviceIndex] = result;
            }

            // Update runtime values on demand
            if(m_runtimeDataChangedFlags[engineDevice.DeviceIndex])
            {
                m_runtimeDataChangedFlags[engineDevice.DeviceIndex] = false;

                SeeingSharpTools.DisposeObject(result);
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    m_fontFamilyName,
                    m_fontWeight, m_fontStyle, m_fontStretch, m_fontSize);
                m_loadedTextFormats[engineDevice.DeviceIndex] = result;
            }

            return result;
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
            m_loadedTextFormats = new DWrite.TextFormat[GraphicsCore.Current.DeviceCount];
            m_runtimeDataChangedFlags = new bool[GraphicsCore.Current.DeviceCount];
            m_fontFamilyName = fontFamilyName;
            m_fontSize = fontSize;
            m_fontWeight = (DWrite.FontWeight)fontWeight;
            m_fontStyle = (DWrite.FontStyle)fontStyle;
            m_fontStretch = (DWrite.FontStretch)fontStretch;

            m_paragraphAlignment = DWrite.ParagraphAlignment.Near;
            m_textAlignment = DWrite.TextAlignment.Leading;
            m_wordWrapping = DWrite.WordWrapping.Wrap;
            m_readingDirection = DWrite.ReadingDirection.LeftToRight;
        }

        /// <summary>
        /// Gets or sets the alignment of the paragraph.
        /// </summary>
        public ParagraphAlignment ParagraphAlignment
        {
            get => (ParagraphAlignment)m_paragraphAlignment;
            set
            {
                var castedValue = (DWrite.ParagraphAlignment)value;

                if(castedValue != m_paragraphAlignment)
                {
                    m_paragraphAlignment = castedValue;
                    m_runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the paragraph.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)m_textAlignment;
            set
            {
                var castedValue = (DWrite.TextAlignment)value;

                if(castedValue != m_textAlignment)
                {
                    m_textAlignment = castedValue;
                    m_runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the WordWrapping mode.
        /// </summary>
        public WordWrapping WordWrapping
        {
            get => (WordWrapping)m_wordWrapping;
            set
            {
                var castedValue = (DWrite.WordWrapping)value;

                if(castedValue != m_wordWrapping)
                {
                    m_wordWrapping = castedValue;
                    m_runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the reading direction.
        /// </summary>
        public ReadingDirection ReadingDirection
        {
            get => (ReadingDirection)m_readingDirection;
            set
            {
                var castedValue = (DWrite.ReadingDirection)value;

                if(castedValue != m_readingDirection)
                {
                    m_readingDirection = castedValue;
                    m_runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }
    }
}