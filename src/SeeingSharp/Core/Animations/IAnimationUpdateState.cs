using System;

namespace SeeingSharp.Core.Animations
{
    public interface IAnimationUpdateState
    {
        TimeSpan UpdateTime
        {
            get;
        }

        int UpdateTimeMilliseconds
        {
            get;
        }

        bool IgnorePauseState
        {
            get;
            set;
        }
    }
}
