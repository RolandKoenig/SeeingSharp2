#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;

using SDX = SharpDX;

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
            if(sizeValue < 1)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Texture Size value {0} within method {1} musst not be smaller that 1!",
                    checkedVariableName, callerMethod));
            }

            // Check for "power of 2"
            if (Math.Abs((double)sizeValue / 2) > EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Texture Size value {0} within method {1} musst be a power of 2!",
                    checkedVariableName, callerMethod));
            }

            // Check for maximum dimension
            //  see https://msdn.microsoft.com/en-us/library/windows/desktop/ff476876(v=vs.85).aspx#Overview
            int maxDimension = 0;
            switch(driverLevel)
            {
                case HardwareDriverLevel.Direct3D9_1:
                case HardwareDriverLevel.Direct3D9_2:
                    maxDimension = 2048;
                    break;

                case HardwareDriverLevel.Direct3D9_3:
                    maxDimension = 4096;
                    break;

                case HardwareDriverLevel.Direct3D10:
                    maxDimension = 8192;
                    break;

                case HardwareDriverLevel.Direct3D11:
                case HardwareDriverLevel.Direct3D12:
                    maxDimension = 16384;
                    break;
            }
            if(sizeValue > maxDimension)
            {
                throw new SeeingSharpCheckException(string.Format(
                    "Texture Size value {0} within method {1} can have a maximum of {2}!",
                    checkedVariableName, callerMethod, maxDimension));
            }
        }
   }
}
