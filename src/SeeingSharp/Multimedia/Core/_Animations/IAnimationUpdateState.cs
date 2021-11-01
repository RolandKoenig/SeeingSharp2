using System;

namespace SeeingSharp.Multimedia.Core
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
