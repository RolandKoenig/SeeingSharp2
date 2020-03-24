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
using System.Collections.Concurrent;
using System.IO;

namespace SeeingSharp.Util
{
    public class ReusableMemoryStreams
    {
        private ConcurrentStack<MemoryStream> _memoryStreams;

        public int Count => _memoryStreams.Count;

        public static ReusableMemoryStreams Current
        {
            get;
            private set;
        }

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
            var buffer = memoryStream.GetBuffer();
            var newAroundSameBuffer = new MemoryStream(buffer, 0, buffer.Length, true, true);
            newAroundSameBuffer.SetLength(0);

            _memoryStreams.Push(newAroundSameBuffer);
        }

        public void Clear()
        {
            _memoryStreams.Clear();
        }
    }
}
