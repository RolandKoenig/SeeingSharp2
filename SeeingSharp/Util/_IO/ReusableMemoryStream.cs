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
using System.IO;

namespace SeeingSharp.Util
{
    /// <summary>
    /// This class works almost like the normal System.MemoryStream class. The difference is that objects
    /// of this class are created to be reused. For a usage example look within the method Common3.GeneralStringSerializer.Serialize.
    /// </summary>
    public class ReusableMemoryStream : Stream
    {
        private static Stack<ReusableMemoryStream> s_streams;
        private static object s_lockObject;

        // Fields
        private byte[] m_buffer;
        private int m_capacity;
        private int m_length;
        private int m_position;
        private const int MEM_STREAM_MAX_LENGTH = 0x7fffffff;

        static ReusableMemoryStream()
        {
            s_streams = new Stack<ReusableMemoryStream>();
            s_lockObject = new object();
        }

        // Methods
        private ReusableMemoryStream()
            : this(0)
        {
        }

        private ReusableMemoryStream(int capacity)
        {
            this.m_buffer = new byte[capacity];
            this.m_capacity = capacity;
        }

        /// <summary>
        /// Takes a Stream that was created before (creates one, if there is no one in the cache).
        /// Note: You have to call ReregisterMemoryStream if you need the returned stream no more.
        /// </summary>
        public static ReusableMemoryStream TakeMemoryStream()
        {
            lock (s_lockObject)
            {
                if (s_streams.Count > 0) { return s_streams.Pop(); }
                else
                {
                    //Start capacity is 20 kb. This big value minimizes the need of risizing the underlying buffer
                    return new ReusableMemoryStream(20000);
                }
            }
        }

        /// <summary>
        /// Reregisters a stream for public use
        /// </summary>
        public static void ReregisterMemoryStream(ReusableMemoryStream memoryStream)
        {
            lock (s_lockObject)
            {
                memoryStream.SetLength(0L);
                s_streams.Push(memoryStream);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                //Nothing to do here
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private bool EnsureCapacity(int value)
        {
            if (value <= this.m_capacity)
            {
                return false;
            }

            int num = value;
            if (num < 0x100)
            {
                num = 0x100;
            }
            if (num < (this.m_capacity * 2))
            {
                num = this.m_capacity * 2;
            }
            this.Capacity = num;
            return true;
        }

        public override void Flush()
        {
        }

        public virtual byte[] GetBuffer()
        {
            return this.m_buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = this.m_length - this.m_position;
            if (num > count)
            {
                num = count;
            }
            if (num <= 0)
            {
                return 0;
            }
            if (num <= 8)
            {
                int num2 = num;
                while (--num2 >= 0)
                {
                    buffer[offset + num2] = this.m_buffer[this.m_position + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(m_buffer, m_position, buffer, offset, num);
            }
            this.m_position += num;
            return num;
        }

        public override int ReadByte()
        {
            if (this.m_position >= this.m_length)
            {
                return -1;
            }
            return this.m_buffer[this.m_position++];
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            switch (loc)
            {
                case SeekOrigin.Begin:
                    this.m_position = ((int)offset);
                    break;

                case SeekOrigin.Current:
                    this.m_position += (int)offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.m_length + ((int)offset);
                    break;
            }
            return (long)this.m_position;
        }

        public override void SetLength(long value)
        {
            int num = ((int)value);
            if (!this.EnsureCapacity(num) && (num > this.m_length))
            {
                Array.Clear(this.m_buffer, this.m_length, num - this.m_length);
            }
            this.m_length = num;
            if (this.m_position > num)
            {
                this.m_position = num;
            }
        }

        public virtual byte[] ToArray()
        {
            byte[] dst = new byte[this.m_length];
            Buffer.BlockCopy(this.m_buffer, 0, dst, 0, this.m_length);
            return dst;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int num = this.m_position + count;
            if (num > this.m_length)
            {
                bool flag = this.m_position > this.m_length;
                if ((num > this.m_capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    Array.Clear(this.m_buffer, this.m_length, num - this.m_length);
                }
                this.m_length = num;
            }
            if (count <= 8)
            {
                int num2 = count;
                while (--num2 >= 0)
                {
                    this.m_buffer[this.m_position + num2] = buffer[offset + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, this.m_buffer, this.m_position, count);
            }
            this.m_position = num;
        }

        public override void WriteByte(byte value)
        {
            if (this.m_position >= this.m_length)
            {
                int num = this.m_position + 1;
                bool flag = this.m_position > this.m_length;
                if ((num >= this.m_capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    Array.Clear(this.m_buffer, this.m_length, this.m_position - this.m_length);
                }
                this.m_length = num;
            }
            this.m_buffer[this.m_position++] = value;
        }

        public virtual void WriteTo(Stream stream)
        {
            stream.Write(this.m_buffer, 0, this.m_length);
        }

        // Properties
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public virtual int Capacity
        {
            get
            {
                return (this.m_capacity);
            }
            set
            {
                if (value != this.m_capacity)
                {
                    if (value > 0)
                    {
                        byte[] dst = new byte[value];
                        if (this.m_length > 0)
                        {
                            Buffer.BlockCopy(this.m_buffer, 0, dst, 0, this.m_length);
                        }
                        this.m_buffer = dst;
                    }
                    else
                    {
                        this.m_buffer = null;
                    }
                    this.m_capacity = value;
                }
            }
        }

        public override long Length
        {
            get
            {
                return (long)(this.m_length);
            }
        }

        public override long Position
        {
            get
            {
                return (long)(this.m_position);
            }
            set
            {
                this.m_position = ((int)value);
            }
        }
    }
}
