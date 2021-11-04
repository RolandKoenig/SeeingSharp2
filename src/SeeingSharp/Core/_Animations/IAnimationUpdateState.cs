using System;

namespace SeeingSharp.Core
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
