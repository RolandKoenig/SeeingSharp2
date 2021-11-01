using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SeeingSharp.Checking
{
    public static partial class Ensure
    {
        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector4 vectorValueLeft, Vector4 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Z, vectorValueRight.Z) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.W, vectorValueRight.W))
            {
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector3 vectorValueLeft, Vector3 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Z, vectorValueRight.Z))
            {
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNotEqual(
            this Vector2 vectorValueLeft, Vector2 vectorValueRight, string checkedVariableName, string comparedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (EngineMath.EqualsWithTolerance(vectorValueLeft.X, vectorValueRight.X) &&
                EngineMath.EqualsWithTolerance(vectorValueLeft.Y, vectorValueRight.Y))
            {
                throw new SeeingSharpCheckException(
                    $"Vector {checkedVariableName} within method {callerMethod} with value {vectorValueLeft} musst not be equal with the vector {comparedVariableName}!");
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNormalized(
            this Vector3 vectorValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!EngineMath.EqualsWithTolerance(vectorValue.Length(), 1f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} must be normalized!",
                    checkedVariableName, callerMethod, vectorValue));
            }
        }

        [Conditional("DEBUG")]
        public static void EnsureNormalized(
            this Vector2 vectorValue, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            if (!EngineMath.EqualsWithTolerance(vectorValue.Length(), 1f))
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Vector {0} within method {1} must be normalized!",
                    checkedVariableName, callerMethod, vectorValue));
            }
        }
    }
}
