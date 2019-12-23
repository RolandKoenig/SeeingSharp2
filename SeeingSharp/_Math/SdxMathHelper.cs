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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Numerics;

namespace SeeingSharp
{  
    internal static class SdxMathHelper
    {
        internal static unsafe SharpDX.Mathematics.Interop.RawColor4 RawFromColor4(Color4 value)
        {
            return *(SharpDX.Mathematics.Interop.RawColor4*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawMatrix3x2 RawFromMatrix3x2(Matrix3x2 value)
        {
            return *(SharpDX.Mathematics.Interop.RawMatrix3x2*)(&value);
        }

        internal static unsafe Matrix3x2 Matrix3x2FromRaw(SharpDX.Mathematics.Interop.RawMatrix3x2 value)
        {
            return *(Matrix3x2*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawVector2 RawFromVector2(Vector2 value)
        {
            return *(SharpDX.Mathematics.Interop.RawVector2*) (&value);
        }

        internal static unsafe Vector2 Vector2FromRaw(SharpDX.Mathematics.Interop.RawVector2 value)
        {
            return *(Vector2*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawRectangle RawFromRectangle(Rectangle value)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangle*)(&value);
        }

        internal static unsafe SharpDX.Mathematics.Interop.RawRectangleF RawFromRectangleF(RectangleF value)
        {
            return *(SharpDX.Mathematics.Interop.RawRectangleF*)(&value);
        }

        internal static unsafe SharpDX.Size2 SdxFromSize2(Size2 value)
        {
            return *(SharpDX.Size2*)(&value);
        }
    }
}
