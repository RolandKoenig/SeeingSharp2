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
using SeeingSharp.Util;

namespace SeeingSharp.AssimpImporter
{
    internal class AssimpIOStream : Assimp.IOStream
    {
        private const int TEMP_BUFFER_SIZE = 1024 * 1024;

        private readonly Stream _stream;

        public AssimpIOStream(Stream stream, string pathToFile, Assimp.FileIOMode fileMode)
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
                    else if (bytesRead != countRead)
                    {
                        Array.Copy(tempBuffer, 0, targetBuffer, actOffset, bytesRead);
                        return actOffset + bytesRead;
                    }
                    else
                    {
                        Array.Copy(tempBuffer, 0, targetBuffer, actOffset, bytesRead);
                    }

                    actOffset += TEMP_BUFFER_SIZE;
                }
            }
            else
            {
                return _stream.Read(targetBuffer, 0, (int) totalReadBytesCount);
            }
            return totalReadBytesCount;
        }

        public override Assimp.ReturnCode Seek(long offset, Assimp.Origin seekOrigin)
        {
            var netOrigin = SeekOrigin.Begin;
            switch (seekOrigin)
            {
                case Assimp.Origin.Set:
                    netOrigin = SeekOrigin.Begin;
                    break;
                
                case Assimp.Origin.Current:
                    netOrigin = SeekOrigin.Current;
                    break;

                case Assimp.Origin.End:
                    netOrigin = SeekOrigin.End;
                    break;
            }

            _stream.Seek(offset, netOrigin);

            return Assimp.ReturnCode.Success;
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

        public override bool IsValid => true;
    }
}
