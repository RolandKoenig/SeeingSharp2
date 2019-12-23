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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class TextFormatResource : Drawing2DResourceBase
    {
        // Fixed resource parameters (passed on constructor)
        private DWrite.TextFormat[] m_loadedTextFormats;
        private string m_fontFamilyName;
        private float m_fontSize;
        private FontWeight m_fontWeight;
        private FontStyle m_fontStyle;
        private FontStretch m_fontStretch;

        // Dynamic runtime parameters (possible to pass on each render call)
        private bool[] m_runtimeDataChangedFlags;
        private ParagraphAlignment m_paragraphAlignment;
        private TextAlignment m_textAlignment;
        private WordWrapping m_wordWrapping;
        private ReadingDirection m_readingDirection;

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
            m_fontWeight = fontWeight;
            m_fontStyle = fontStyle;
            m_fontStretch = fontStretch;

            m_paragraphAlignment = Drawing2D.ParagraphAlignment.Near; //ParagraphAlignment.Near;
            m_textAlignment = Drawing2D.TextAlignment.Leading;
            m_wordWrapping = Drawing2D.WordWrapping.Wrap;
            m_readingDirection = Drawing2D.ReadingDirection.LeftToRight;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var textFormat = m_loadedTextFormats[engineDevice.DeviceIndex];

            if (textFormat != null)
            {
                engineDevice.DeregisterDeviceResource(this);

                SeeingSharpUtil.DisposeObject(textFormat);
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
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = m_loadedTextFormats[engineDevice.DeviceIndex];
            if (result == null)
            {
                // Load the TextFormat object
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    m_fontFamilyName,
                    (DWrite.FontWeight)m_fontWeight, (DWrite.FontStyle)m_fontStyle, (DWrite.FontStretch)m_fontStretch, m_fontSize);
                m_loadedTextFormats[engineDevice.DeviceIndex] = result;
                engineDevice.RegisterDeviceResource(this);
            }

            // Update runtime values on demand
            if (m_runtimeDataChangedFlags[engineDevice.DeviceIndex])
            {
                m_runtimeDataChangedFlags[engineDevice.DeviceIndex] = false;

                SeeingSharpUtil.DisposeObject(result);
                result = new DWrite.TextFormat(
                    GraphicsCore.Current.FactoryDWrite,
                    m_fontFamilyName,
                    (DWrite.FontWeight)m_fontWeight, (DWrite.FontStyle)m_fontStyle, (DWrite.FontStretch)m_fontStretch, m_fontSize);
                result.TextAlignment = (DWrite.TextAlignment)m_textAlignment;
                m_loadedTextFormats[engineDevice.DeviceIndex] = result;
            }

            return result;
        }

        /// <summary>
        /// Gets or sets the alignment of the paragraph.
        /// </summary>
        public ParagraphAlignment ParagraphAlignment
        {
            get => m_paragraphAlignment;
            set
            {
                var castedValue = value;

                if (castedValue != m_paragraphAlignment)
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
            get => m_textAlignment;
            set
            {
                var castedValue = value;

                if (castedValue != m_textAlignment)
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
            get => m_wordWrapping;
            set
            {
                var castedValue = value;

                if (castedValue != m_wordWrapping)
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
            get => m_readingDirection;
            set
            {
                var castedValue = value;

                if (castedValue != m_readingDirection)
                {
                    m_readingDirection = castedValue;
                    m_runtimeDataChangedFlags.SetAllValuesTo(true);
                }
            }
        }
    }
}