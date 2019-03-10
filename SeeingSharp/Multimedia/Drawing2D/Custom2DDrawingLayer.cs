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

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class Custom2DDrawingLayer
    {
        private Action<Graphics2D> m_draw2DAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Custom2DDrawingLayer"/> class.
        /// </summary>
        public Custom2DDrawingLayer()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Custom2DDrawingLayer"/> class.
        /// </summary>
        /// <param name="draw2DAction">The action which is used for rendering.</param>
        public Custom2DDrawingLayer(Action<Graphics2D> draw2DAction)
        {
            m_draw2DAction = draw2DAction;
        }

        /// <summary>
        /// Performs custom 2D rendering.
        /// Be careful: This method is called from the rendering thread!
        /// </summary>
        /// <param name="graphics">The graphics object used for drawing.</param>
        protected virtual void Draw2D(Graphics2D graphics)
        {
            m_draw2DAction?.Invoke(graphics);
        }

        /// <summary>
        /// Performs 2D rendering. This method gets called directly from RenderLoop or the Scene.
        /// </summary>
        /// <param name="graphics">The graphics object used for drawing.</param>
        internal void Draw2DInternal(Graphics2D graphics)
        {
            Draw2D(graphics);
        }
    }
}
