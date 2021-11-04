using System;
using SeeingSharp.Core;

namespace SeeingSharp.Drawing2D
{
    public class Custom2DDrawingLayer
    {
        private Action<Graphics2D> _draw2DAction;
        private Action<UpdateState> _updateAction;

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
            _draw2DAction = draw2DAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Custom2DDrawingLayer"/> class.
        /// </summary>
        /// <param name="draw2DAction">The action which is used for rendering.</param>
        /// <param name="updateAction">The action which is called before rendering.</param>
        public Custom2DDrawingLayer(Action<Graphics2D> draw2DAction, Action<UpdateState> updateAction)
        {
            _draw2DAction = draw2DAction;
            _updateAction = updateAction;
        }

        /// <summary>
        /// Performs custom 2D rendering.
        /// Be careful: This method is called from background thread!
        /// </summary>
        /// <param name="graphics">The graphics object used for drawing.</param>
        protected virtual void Draw2D(Graphics2D graphics)
        {
            _draw2DAction?.Invoke(graphics);
        }

        /// <summary>
        /// Performs custom update logic.
        /// Be careful: This method is called from background thread!
        /// </summary>
        /// <param name="updateState">Contains all information about current update pass.</param>
        protected virtual void Update(UpdateState updateState)
        {
            _updateAction?.Invoke(updateState);
        }

        internal void Draw2DInternal(Graphics2D graphics)
        {
            this.Draw2D(graphics);
        }

        internal void UpdateInternal(UpdateState updateState)
        {
            this.Update(updateState);
        }
    }
}
