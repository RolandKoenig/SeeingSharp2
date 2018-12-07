#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics2D._01_Primitives
{
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
                // Draw the background
                RectangleF d2dRectangle = new RectangleF(
                    10, 10, 
                    graphics.ScreenWidth - 20f, graphics.ScreenHeight - 20f);

                graphics.Clear(Color4Ex.LightBlue);
                graphics.FillRoundedRectangle(
                    d2dRectangle, 30, 30,
                    solidBrush);
            });
        }
    }
}
