using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;

namespace SeeingSharp.Util
{
    public class ReusableMemoryStreams
    {
        private ConcurrentStack<MemoryStream> m_memoryStreams;

        static ReusableMemoryStreams()
        {
            Current = new ReusableMemoryStreams();
        }

        public ReusableMemoryStreams()
        {
            m_memoryStreams = new ConcurrentStack<MemoryStream>();
        }

        public IDisposable UseMemoryStream(out MemoryStream memoryStream, int requiredCapacity = 128)
        {
            memoryStream = TakeMemoryStream(requiredCapacity);

            MemoryStream cachedMemoryStream = memoryStream;
            return new DummyDisposable(() => ReregisterMemoryStream(cachedMemoryStream));
        }

        public MemoryStream TakeMemoryStream(int requiredCapacity = 128)
        {
            MemoryStream result;
            if(!m_memoryStreams.TryPop(out result))
            {
                result = new MemoryStream(requiredCapacity);
            }
            else
            {
                if(result.Capacity < requiredCapacity) { result.Capacity = requiredCapacity; }
            }
            return result;
        }

        public void ReregisterMemoryStream(MemoryStream memoryStream)
        {
            var buffer = memoryStream.GetBuffer();
            MemoryStream newAroundSameBuffer = new MemoryStream(buffer, 0, buffer.Length, true, true);
            newAroundSameBuffer.SetLength(0);

            m_memoryStreams.Push(newAroundSameBuffer);
        }

        public void Clear()
        {
            m_memoryStreams.Clear();
        }

        public int Count
        {
            get { return m_memoryStreams.Count; }
        }

        public static ReusableMemoryStreams Current
        {
            get;
            private set;
        }
    }
}
