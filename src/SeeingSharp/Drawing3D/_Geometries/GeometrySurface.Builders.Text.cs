using System;
using SeeingSharp.Core;
using DWrite = Vortice.DirectWrite;

namespace SeeingSharp.Drawing3D
{
    public partial class GeometrySurface
    {
        /// <summary>
        /// Builds the text geometry for the given string.
        /// </summary>
        /// <param name="stringToBuild">The string to build within the geometry.</param>
        public void BuildTextGeometry(string stringToBuild)
        {
            this.BuildTextGeometry(stringToBuild, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Builds the text geometry for the given string.
        /// </summary>
        /// <param name="stringToBuild">The string to build within the geometry.</param>
        /// <param name="geometryOptions">Some configuration for geometry creation.</param>
        public void BuildTextGeometry(string stringToBuild, TextGeometryOptions geometryOptions)
        {
            var writeFactory = GraphicsCore.Current.FactoryDWrite;

            // Get font properties
            var fontWeight = geometryOptions.FontWeight;
            var fontStyle = geometryOptions.FontStyle;

            // Create the text layout object
            try
            {
                

                using var textLayout = writeFactory.CreateTextLayout(
                    stringToBuild,
                    writeFactory.CreateTextFormat(
                        geometryOptions.FontFamily, (DWrite.FontWeight)fontWeight, (DWrite.FontStyle)fontStyle, geometryOptions.FontSize),
                        float.MaxValue, float.MaxValue);

                // Render the text using the geometry text renderer
                using var textRenderer = new GeometryTextRenderer(this, geometryOptions);
                textLayout.Draw(textRenderer, 0f, 0f);
            }
            catch (Exception ex)
            {
                // Publish exception info
                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.CreateTextGeometry);
            }
        }
    }
}