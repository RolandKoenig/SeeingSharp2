using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using SeeingSharp.Util;

namespace SeeingSharp.Benchmarks.Math
{
    public class RingBufferBenchmark
    {
        [Params(10, 100, 1000, 10000, 100000)]
        public int BufferSize;

        private Random _random;

        private RingBuffer<int> _ringBuffer;
        private List<int> _list;

        [GlobalSetup]
        public void Setup()
        {
            _ringBuffer = new RingBuffer<int>(BufferSize);
            _list = new List<int>(BufferSize);

            _random = new Random(1000);
            for (var loop = 0; loop < BufferSize; loop++)
            {
                _ringBuffer.Add(_random.Next(0, 1000));
                _list.Add(_random.Next(0, 1000));
            }
        }

        [Benchmark]
        public void AddToRingBuffer()
        {
            _ringBuffer.Add(1000);
        }

        [Benchmark]
        public void AddToList()
        {
            _list.RemoveAt(0);
            _list.Add(1000);
        }
    }
}
