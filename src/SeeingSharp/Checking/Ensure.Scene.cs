using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Checking
{
    public static partial class EnsureMultimedia
    {
        [Conditional("DEBUG")]
        public static void EnsureObjectOfScene(
            this SceneObject sceneObject, Scene scene, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (sceneObject.Scene != scene)
            {
                throw new SeeingSharpCheckException(
                    $"The object {checkedVariableName} within method {callerMethod} is not part of the expected Scene!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureObjectOfScene(
            this IEnumerable<SceneObject> sceneObjects, Scene scene, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            var actIndex = 0;

            foreach (var actObject in sceneObjects)
            {
                actObject.EnsureObjectOfScene(scene, $"{checkedVariableName}[{actIndex}]", callerMethod);
                actIndex++;
            }
        }
    }
}