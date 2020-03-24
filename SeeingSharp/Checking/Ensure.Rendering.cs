/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SeeingSharp.Multimedia.Core;

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
