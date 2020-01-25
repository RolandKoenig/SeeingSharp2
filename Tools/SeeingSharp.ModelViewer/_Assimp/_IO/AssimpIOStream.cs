using System.IO;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public class AssimpIOStream : Assimp.IOStream
    {
        private Stream m_stream;

        public AssimpIOStream(Stream stream, string pathToFile, Assimp.FileIOMode fileMode)
            : base(pathToFile, fileMode)
        {
            m_stream = stream;
        }

        public override long Write(byte[] dataToWrite, long count)
        {
            m_stream.Write(dataToWrite, 0, (int)count);
            return count;
        }

        public override long Read(byte[] dataRead, long count)
        {
            return m_stream.Read(dataRead, 0, (int)count);
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
                SeeingSharpUtil.SafeDispose(ref m_stream);
            }
        }

        public override bool IsValid => true;
    }
}
