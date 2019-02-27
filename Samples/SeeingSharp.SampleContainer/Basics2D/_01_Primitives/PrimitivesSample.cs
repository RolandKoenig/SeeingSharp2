#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.SampleContainer.Basics2D._01_Primitives
{
    #region using

    using System.Threading.Tasks;
    using Checking;
    using Multimedia.Core;
    using Multimedia.Drawing2D;
    using SharpDX;

    #endregion

    [SampleDescription(
        "Primitives", 1, nameof(SeeingSharp.SampleContainer.Basics2D),
        sampleImageFileName:"PreviewImage.png",
        sourceCodeUrl: "https://github.com/RolandKoenig/SeeingSharp2/tree/master/_Samples/SeeingSharp.SampleContainer/Basics2D/_01_Primitives")]
    public class PrimitivesSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            var solidBrush = new SolidBrushResource(Color4Ex.Gray);
            await targetRenderLoop.Register2DDrawingLayerAsync((graphics) =>
            {
                // Full width and height for all primitive objects
                float allPrimitivesWidth = 180f;
                float allPrimitivesHeight = 100f;

                // Update current transform so that primitive objects are in the middle of the screen
                var screenBounds = graphics.ScreenBounds;
                graphics.TransformStack.Push(Matrix3x2.Translation(new Vector2(
                    (graphics.ScreenWidth / 2f) - (allPrimitivesWidth / 2f),
                    (graphics.ScreenHeight / 2f) - (allPrimitivesHeight / 2f))));
                graphics.ApplyTransformStack();

                try
                {
                    // Draw all primitives
                    graphics.FillRectangle(
                        new RectangleF(0f, 0f, 50f, 100f),
                        solidBrush);
                    graphics.FillRoundedRectangle(
                        new RectangleF(60f, 0f, 50f, 100f),
                        15f, 15f,
                        solidBrush);
                    graphics.DrawLine(
                        new Vector2(120f, 0f),
                        new Vector2(120f, 100f),
                        solidBrush,
                        2f);
                    graphics.FillEllipse(
                        new RectangleF(130f, 0f, 50f, 100f),
                        solidBrush);
                }
                finally
                {
                    // Reset transform
                    graphics.TransformStack.Pop();
                    graphics.ApplyTransformStack();
                }
            });
        }
    }
}
