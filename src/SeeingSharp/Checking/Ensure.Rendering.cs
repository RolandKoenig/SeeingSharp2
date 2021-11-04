using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SeeingSharp.Core;

namespace SeeingSharp.Checking
{
    public static partial class EnsureMultimedia
    {
        [Conditional("DEBUG")]
        public static void EnsureValidTextureSize(
            this int sizeValue, HardwareDriverLevel driverLevel, string checkedVariableName,
            [CallerMemberName]
            string callerMethod = "")
        {
            if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

            // Check for positive value
            if (sizeValue < 1)
            {
                throw new SeeingSharpCheckException(
                    $"Texture Size value {checkedVariableName} within method {callerMethod} musst not be smaller that 1!");
            }

            // Check for "power of 2"
            if (Math.Abs((double)sizeValue / 2) > EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                throw new SeeingSharpCheckException(
                    $"Texture Size value {checkedVariableName} within method {callerMethod} musst be a power of 2!");
            }

            // Check for maximum dimension
            //  see https://msdn.microsoft.com/en-us/library/windows/desktop/ff476876(v=vs.85).aspx#Overview
            var maxDimension = 0;
            switch (driverLevel)
            {
                case HardwareDriverLevel.Direct3D10:
                    maxDimension = 8192;
                    break;

                case HardwareDriverLevel.Direct3D11:
                case HardwareDriverLevel.Direct3D12:
                    maxDimension = 16384;
                    break;
            }
            if (sizeValue > maxDimension)
            {
                throw new SeeingSharpCheckException(
                    $"Texture Size value {checkedVariableName} within method {callerMethod} can have a maximum of {maxDimension}!");
            }
        }
    }
}
