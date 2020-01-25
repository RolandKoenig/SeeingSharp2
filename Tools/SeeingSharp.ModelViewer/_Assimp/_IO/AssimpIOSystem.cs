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
                // Split all elements (directories and the file) form the given path
                string[] pathElements;
                if (Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar)
                {
                    pathElements = pathToFile.Split(Path.DirectorySeparatorChar);
                }
                else
                {
                    pathElements = pathToFile.Split(new[]
                        {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar});
                }

                // Now get true directory path
                var pathDirectoryBuilder = new List<string>(pathElements.Length);
                for (var loop = 0; loop < pathElements.Length - 1; loop++)
                {
                    var actDirectoryName = pathElements[loop];
                    if(string.IsNullOrEmpty(actDirectoryName)) { continue; }
                    if(actDirectoryName == "."){ continue; }

                    pathDirectoryBuilder.Add(actDirectoryName);
                }

                // Load the file
                var link = m_originalResource.GetForAnotherFile(
                    pathElements[^1], 
                    pathDirectoryBuilder.ToArray());
                var openedStream = link.OpenInputStream();

                return new AssimpIOStream(openedStream, pathToFile, fileMode);
            }
        }
    }
}
