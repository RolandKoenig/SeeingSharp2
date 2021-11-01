using System;
using System.Threading;

namespace SeeingSharp.Util
{
    public static class SynchronizationContextExtensions
    {
        private static readonly SendOrPostCallback s_postAsyncCallBack = PostAsyncCallback;
        private static readonly ConcurrentObjectPool<PostAsyncState> s_cachedObjects = new ConcurrentObjectPool<PostAsyncState>(
            () => new PostAsyncState(),
            16);

        /// <summary>
        /// Post the given action in an async manner to the given SynchronizationContext.
        /// </summary>
        /// <param name="syncContext">The target SynchronizationContext.</param>
        /// <param name="postAction">The action to be posted.</param>
        public static ReusableAwaiter PostAsync(this SynchronizationContext syncContext, Action postAction)
        {
            var resultTask = ReusableAwaiter.Take();

            var asyncState = PostAsyncState.Take();
            asyncState.Awaiter = resultTask;
            asyncState.PostAction = postAction;

            syncContext.Post(s_postAsyncCallBack, asyncState);

            return resultTask;
        }

        private static void PostAsyncCallback(object arg)
        {
            var asyncStateInner = (PostAsyncState) arg;
            try
            {
                asyncStateInner.PostAction();
                asyncStateInner.Awaiter.TrySetCompleted();
            }
            catch (Exception ex)
            {
                asyncStateInner.Awaiter.TrySetException(ex);
            }
            finally
            {
                PostAsyncState.Return(asyncStateInner);
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper for PostAsync method
        /// </summary>
        private class PostAsyncState
        {
            public Action PostAction;
            public ReusableAwaiter Awaiter;

            public static PostAsyncState Take()
            {
                return s_cachedObjects.Rent();
            }

            public static void Return(PostAsyncState state)
            {
                s_cachedObjects.Return(state);
            }
        }
    }
}
