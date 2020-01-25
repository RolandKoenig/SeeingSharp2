using System;
using System.IO;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public class AssimpIOStream : Assimp.IOStream
    {
        private const int TEMP_BUFFER_SIZE = 1024 * 1024;

        private readonly Stream m_stream;

        public AssimpIOStream(Stream stream, string pathToFile, Assimp.FileIOMode fileMode)
            : base(pathToFile, fileMode)
        {
            m_stream = stream;
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
                    m_stream.Write(tempBuffer, 0, countWrite);

                    actOffset += TEMP_BUFFER_SIZE;
                }
            }
            else
            {
                m_stream.Write(sourceBuffer, 0, (int)totalWriteBytesCount);
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

                    var bytesRead = m_stream.Read(tempBuffer, 0, countRead);
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
                return m_stream.Read(targetBuffer, 0, (int) totalReadBytesCount);
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

            m_stream.Seek(offset, netOrigin);

            return Assimp.ReturnCode.Success;
        }

        public override long GetPosition()
        {
            return m_stream.Position;
        }

        public override long GetFileSize()
        {
            return m_stream.Length;
        }

        public override void Flush()
        {
            m_stream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SeeingSharpUtil.DisposeObject(m_stream);
            }
        }

        public override bool IsValid => true;
    }
}
