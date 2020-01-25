using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assimp;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public class AssimpIOSystem : Assimp.IOSystem
    {
        private ResourceLink m_originalResource;

        public AssimpIOSystem(ResourceLink originalResource)
        {
            m_originalResource = originalResource;
        }

        public override IOStream OpenFile(string pathToFile, FileIOMode fileMode)
        {
            if (pathToFile == m_originalResource.FileNameWithExtension)
            {
                var openedStream = m_originalResource.OpenInputStream();

                return new AssimpIOStream(openedStream, pathToFile, fileMode);
            }
            else
            {
                // TODO: Split path to separate folders
                var link = m_originalResource.GetForAnotherFile(pathToFile);
                var openedStream = link.OpenInputStream();

                return new AssimpIOStream(openedStream, pathToFile, fileMode);
            }
        }
    }
}
