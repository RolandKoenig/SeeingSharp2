#region License information
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
#endregion
#region using

// Namespace mappings
using System;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Objects
{
    #region using
    #endregion

    public class WirePainter
    {
        public void DrawLine(Vector3 start, Vector3 destination)
        {
            DrawLine(start, destination, Color4.Black);
        }

        public void DrawLine(Vector3 start, Vector3 destination, Color4 lineColor)
        {
            if (!m_isValid) { throw new SeeingSharpGraphicsException($"This {nameof(WirePainter)} is only valid in the rendering pass that created it!"); }

            // Load and render the given line
            using (var lineBuffer = GraphicsHelper.CreateImmutableVertexBuffer(m_renderState.Device, new[] { start, destination }))
            {
                m_renderResources.RenderLines(
                    m_renderState, m_worldViewPojCreator.Value, lineColor, lineBuffer, 2);
            }
        }

        public void DrawTriangle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            DrawTriangle(point1, point2, point3, Color4.Black);
        }

        public void DrawTriangle(Vector3 point1, Vector3 point2, Vector3 point3, Color4 lineColor)
        {
            if (!m_isValid)
            {
                throw new SeeingSharpGraphicsException($"This {nameof(WirePainter)} is only valid in the rendering pass that created it!");
            }

            Line[] lineData = {
                new Line(point1, point2),
                new Line(point2, point3),
                new Line(point3, point1)
            };

            // Load and render the given lines
            using (var lineBuffer = GraphicsHelper.CreateImmutableVertexBuffer(m_renderState.Device, lineData))
            {
                m_renderResources.RenderLines(
                    m_renderState, m_worldViewPojCreator.Value, lineColor, lineBuffer, lineData.Length * 2);
            }
        }

        internal void SetInvalid()
        {
            m_isValid = false;
        }

        internal WirePainter(RenderState renderState, LineRenderResources renderResources)
        {
            m_isValid = true;
            m_renderResources = renderResources;
            m_renderState = renderState;

            m_worldViewPojCreator = new Lazy<Matrix>(() => Matrix.Transpose(renderState.ViewProj));
        }

        #region All members required for painting
        private bool m_isValid;
        private LineRenderResources m_renderResources;
        private RenderState m_renderState;
        private Lazy<Matrix> m_worldViewPojCreator;
        #endregion
    }
}
