using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    internal static class CommonExtensions
    {
        /// <summary>
        /// "Forgets" the given task, but still tries to dispatch exception somewhere the user / developer
        /// can see them.
        /// </summary>
        /// <param name="asyncAction">The action to be fired.</param>
        public static async void FireAndForget(this Task asyncAction)
        {
            await asyncAction;
        }
    }
}
