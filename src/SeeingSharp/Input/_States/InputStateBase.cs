using System;
using SeeingSharp.Core;

namespace SeeingSharp.Input
{
    /// <summary>
    /// Base class for all input states.
    /// </summary>
    public abstract class InputStateBase
    {
        /// <summary>
        /// The view object this input state was queried on.
        /// Null, if this InputState does not depend on a view.
        /// </summary>
        public ViewInformation? RelatedView { get; internal set; }

        /// <summary>
        /// The view index this input state was queried on.
        /// -1, if this InputState does not depend on a view.
        /// </summary>
        public int ViewIndex
        {
            get
            {
                if (this.RelatedView == null) { return -1; }
                return this.RelatedView.ViewIndex;
            }
        }

        internal Type CurrentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputStateBase"/> class.
        /// </summary>
        protected InputStateBase()
        {
            this.CurrentType = this.GetType();
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected abstract void CopyAndResetForUpdatePassInternal(InputStateBase targetState);

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        internal void CopyAndResetForUpdatePass(InputStateBase targetState)
        {
            this.CopyAndResetForUpdatePassInternal(targetState);
        }
    }
}