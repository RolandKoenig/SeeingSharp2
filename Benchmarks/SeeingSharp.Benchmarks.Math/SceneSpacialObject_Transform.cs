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
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using BenchmarkDotNet.Jobs;

namespace SeeingSharp.Benchmarks.Math
{
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [SimpleJob(RuntimeMoniker.Net471)]
    public class SceneSpacialObject_Transform
    {
        private SharpDX.Matrix _sdxScaling = SharpDX.Matrix.Scaling(2.5f, -2f, 3f);
        private SharpDX.Matrix _sdxRotation = SharpDX.Matrix.RotationYawPitchRoll(0.23f, 0.42f, -0.23f);
        private SharpDX.Matrix _sdxTranslation = SharpDX.Matrix.Translation(3f, 1.5f, 5f);
        private SharpDX.Matrix _sdxPrevTop = SharpDX.Matrix.Identity;

        private System.Numerics.Matrix4x4 _numericsScaling = System.Numerics.Matrix4x4.CreateScale(2.5f, -2f, 3f);
        private System.Numerics.Matrix4x4 _numericsRotation = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(0.23f, 0.42f, -0.23f);
        private System.Numerics.Matrix4x4 _numericsTranslation = System.Numerics.Matrix4x4.CreateTranslation(3f, 1.5f, 5f);
        private System.Numerics.Matrix4x4 _numericsPrevTop = System.Numerics.Matrix4x4.Identity;

        [Benchmark]
        public SharpDX.Matrix SharpDX_SceneSpacialObject_FullTransfor_Precalculated()
        {
            return _sdxScaling *
                   _sdxRotation *
                   _sdxTranslation *
                   _sdxPrevTop;
        }

        [Benchmark]
        public SharpDX.Matrix SharpDX_SceneSpacialObject_FullTransfor_Precalculated_Refs()
        {
            SharpDX.Matrix.Multiply(ref _sdxScaling, ref _sdxRotation, out SharpDX.Matrix result);
            SharpDX.Matrix.Multiply(ref result, ref _sdxTranslation, out result);
            SharpDX.Matrix.Multiply(ref result, ref _sdxPrevTop, out result);

            return result;
        }

        [Benchmark]
        public SharpDX.Matrix SharpDX_SceneSpacialObject_FullTransform()
        {
            return SharpDX.Matrix.Scaling(2.5f, -2f, 3f) *
                   SharpDX.Matrix.RotationYawPitchRoll(0.23f, 0.42f, -0.23f) *
                   SharpDX.Matrix.Translation(3f, 1.5f, 5f) *
                   SharpDX.Matrix.Identity;
        }

        [Benchmark]
        public System.Numerics.Matrix4x4 Numerics_SceneSpacialObject_FullTransfor_Precalculated()
        {
            return _numericsScaling *
                   _numericsRotation *
                   _numericsTranslation *
                   _numericsPrevTop;
        }

        [Benchmark]
        public System.Numerics.Matrix4x4 Numerics_SceneSpacialObject_FullTransform()
        {
            return System.Numerics.Matrix4x4.CreateScale(2.5f, -2f, 3f) *
                   System.Numerics.Matrix4x4.CreateFromYawPitchRoll(0.23f, 0.42f, -0.23f) *
                   System.Numerics.Matrix4x4.CreateTranslation(3f, 1.5f, 5f) *
                   System.Numerics.Matrix4x4.Identity;
        }
    }
}
