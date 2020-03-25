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
using System.IO;
using Assimp;
using SeeingSharp.Util;

namespace SeeingSharp.AssimpImporter
{
    internal class AssimpIOStream : IOStream
    {
        private const int TEMP_BUFFER_SIZE = 1024 * 1024;

        private readonly Stream _stream;

        public override bool IsValid => true;

        public AssimpIOStream(Stream stream, string pathToFile, FileIOMode fileMode)
            : base(pathToFile, fileMode)
        {
            _stream = stream;
        }

        public override long Write(byte[] sourceBuffer, long totalWriteBytesCount)
        {
            if (totalWriteBytesCount > TEMP_BUFFER_SIZE)
            {
                var tempBuffer = new byte[TEMP_BUFFER_SIZE];
                long actOffset = 0;

                while (actOffset < totalWriteBytesCount)
                {
                    var countWrite = (int)Math.Min(totalWriteBytesCount - actOffset, TEMP_BUFFER_SIZE);

                    Array.Copy(sourceBuffer, actOffset, tempBuffer, 0, countWrite);
                    _stream.Write(tempBuffer, 0, countWrite);

                    actOffset += TEMP_BUFFER_SIZE;
                }
            }
            else
            {
                _stream.Write(sourceBuffer, 0, (int)totalWriteBytesCount);
            }

            return totalWriteBytesCount;
        }

        public override long Read(byte[] targetBuffer, long totalReadBytesCount)
        {
            if (totalReadBytesCount > TEMP_BUFFER_SIZE)
            {
                var tempBuffer = new byte[TEMP_BUFFER_SIZE];
                long actOffset = 0;

                while (actOffset < totalReadBytesCount)
                {
                    var countRead = (int) Math.Min(totalReadBytesCount - actOffset, TEMP_BUFFER_SIZE);

                    var bytesRead = _stream.Read(tempBuffer, 0, countRead);
                    if (bytesRead == 0)
                    {
                        return actOffset;
                    }
                    if (bytesRead != countRead)
                    {
                        Array.Copy(tempBuffer, 0, targetBuffer, actOffset, bytesRead);
                        return actOffset + bytesRead;
                    }
                    Array.Copy(tempBuffer, 0, targetBuffer, actOffset, bytesRead);

                    actOffset += TEMP_BUFFER_SIZE;
                }
            }
            else
            {
                return _stream.Read(targetBuffer, 0, (int) totalReadBytesCount);
            }
            return totalReadBytesCount;
        }

        public override ReturnCode Seek(long offset, Origin seekOrigin)
        {
            var netOrigin = seekOrigin switch
            {
                Origin.Set => SeekOrigin.Begin,
                Origin.Current => SeekOrigin.Current,
                Origin.End => SeekOrigin.End,
                _ => SeekOrigin.Begin
            };

            _stream.Seek(offset, netOrigin);

            return ReturnCode.Success;
        }

        public override long GetPosition()
        {
            return _stream.Position;
        }

        public override long GetFileSize()
        {
            return _stream.Length;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SeeingSharpUtil.DisposeObject(_stream);
            }
        }
    }
}
