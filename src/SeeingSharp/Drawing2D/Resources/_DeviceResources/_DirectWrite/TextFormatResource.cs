using System;
using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;
using DWrite = Vortice.DirectWrite;

namespace SeeingSharp.Drawing2D.Resources
{
    public class TextFormatResource : Drawing2DResourceBase
    {
        // Fixed resource parameters (passed on constructor)
        private DWrite.IDWriteTextFormat[] _loadedTextFormats;
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
            _loadedTextFormats = new DWrite.IDWriteTextFormat[GraphicsCore.Current.DeviceCount];
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
        internal DWrite.IDWriteTextFormat GetTextFormat(EngineDevice engineDevice)
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
                result = GraphicsCore.Current.FactoryDWrite.CreateTextFormat(
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
                result = GraphicsCore.Current.FactoryDWrite.CreateTextFormat(
                    _fontFamilyName,
                    (DWrite.FontWeight)_fontWeight, (DWrite.FontStyle)_fontStyle, (DWrite.FontStretch)_fontStretch, _fontSize);
                result.TextAlignment = (DWrite.TextAlignment)_textAlignment;
                _loadedTextFormats[engineDevice.DeviceIndex] = result;
            }

            return result;
        }
    }
}