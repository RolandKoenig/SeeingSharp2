using System;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public class EventDrivenStepInfo
    {
        public int AnimationCount
        {
            get;
            internal set;
        }

        public TimeSpan UpdateTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return "" + this.AnimationCount + " Animations (Time: " + SeeingSharpUtil.FormatTimespanCompact(this.UpdateTime) + ")";
        }
    }
}
