using System;
using System.Collections.Concurrent;
using System.IO;

namespace SeeingSharp.Util
{
    public class ReusableMemoryStreams
    {
        private ConcurrentStack<MemoryStream> _memoryStreams;

        public int Count => _memoryStreams.Count;

        public static ReusableMemoryStreams Current { get; }

        static ReusableMemoryStreams()
        {
            Current = new ReusableMemoryStreams();
        }

        public ReusableMemoryStreams()
        {
            _memoryStreams = new ConcurrentStack<MemoryStream>();
        }

        public IDisposable UseMemoryStream(out MemoryStream memoryStream, int requiredCapacity = 128)
        {
            memoryStream = this.TakeMemoryStream(requiredCapacity);

            var cachedMemoryStream = memoryStream;
            return new DummyDisposable(() => this.ReRegisterMemoryStream(cachedMemoryStream));
        }

        public MemoryStream TakeMemoryStream(int requiredCapacity = 128)
        {
            if (!_memoryStreams.TryPop(out var result))
            {
                result = new MemoryStream(requiredCapacity);
            }
            else
            {
                if (result.Capacity < requiredCapacity) { result.Capacity = requiredCapacity; }
            }
            return result;
        }

        public void ReRegisterMemoryStream(MemoryStream memoryStream)
        {
            memoryStream.SetLength(0);
            _memoryStreams.Push(memoryStream);
        }

        public void Clear()
        {
            _memoryStreams.Clear();
        }
    }
}
