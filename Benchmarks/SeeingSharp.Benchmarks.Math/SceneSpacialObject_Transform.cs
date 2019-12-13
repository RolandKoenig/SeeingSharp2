using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SharpDX;

namespace SeeingSharp.Benchmarks.Math
{
    [SimpleJob(RuntimeMoniker.CoreRt31)]
    [SimpleJob(RuntimeMoniker.Net471)]
    public class SceneSpacialObject_Transform
    { 
        private SharpDX.Matrix m_sdxScaling = SharpDX.Matrix.Scaling(2.5f, -2f, 3f);
        private SharpDX.Matrix m_sdxRotation = SharpDX.Matrix.RotationYawPitchRoll(0.23f, 0.42f, -0.23f);
        private SharpDX.Matrix m_sdxTranslation = SharpDX.Matrix.Translation(3f, 1.5f, 5f);
        private SharpDX.Matrix m_sdxPrevTop = SharpDX.Matrix.Identity;

        private System.Numerics.Matrix4x4 m_numericsScaling = System.Numerics.Matrix4x4.CreateScale(2.5f, -2f, 3f);
        private System.Numerics.Matrix4x4 m_numericsRotation = System.Numerics.Matrix4x4.CreateFromYawPitchRoll(0.23f, 0.42f, -0.23f);
        private System.Numerics.Matrix4x4 m_numericsTranslation = System.Numerics.Matrix4x4.CreateTranslation(3f, 1.5f, 5f);
        private System.Numerics.Matrix4x4 m_numericsPrevTop = System.Numerics.Matrix4x4.Identity;

        [Benchmark]
        public SharpDX.Matrix SharpDX_SceneSpacialObject_FullTransform_Precalculated()
        {
            return m_sdxScaling *
                   m_sdxRotation *
                   m_sdxTranslation *
                   m_sdxPrevTop;
        }

        [Benchmark]
        public SharpDX.Matrix SharpDX_SceneSpacialObject_FullTransform_Precalculated_Refs()
        {
            SharpDX.Matrix.Multiply(ref m_sdxScaling, ref m_sdxRotation, out SharpDX.Matrix result);
            SharpDX.Matrix.Multiply(ref result, ref m_sdxTranslation, out result);
            SharpDX.Matrix.Multiply(ref result, ref m_sdxPrevTop, out result);

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
        public System.Numerics.Matrix4x4 Numerics_SceneSpacialObject_FullTransform_Precalculated()
        {
            return m_numericsScaling *
                   m_numericsRotation *
                   m_numericsTranslation *
                   m_numericsPrevTop;
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
