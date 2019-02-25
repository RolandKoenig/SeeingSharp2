#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Util
{
    #region using

    using System;
    using System.Collections.Concurrent;
    using System.IO;

    #endregion

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
