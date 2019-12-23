
/* Unmerged change from project 'SeeingSharp.Benchmarks.Math (net471)'
Before:
using System;
using BenchmarkDotNet.Running;
After:
using BenchmarkDotNet.Running;
using System;
*/
using BenchmarkDotNet.Running;

namespace SeeingSharp.Benchmarks.Math
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SceneSpacialObject_Transform>();
        }
    }
}
