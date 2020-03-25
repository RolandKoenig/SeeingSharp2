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

using System.Collections.Generic;
using System.IO;
using Assimp;
using SeeingSharp.Util;

namespace SeeingSharp.AssimpImporter
{
    internal class AssimpIOSystem : IOSystem
    {
        private ResourceLink _originalResource;

        public AssimpIOSystem(ResourceLink originalResource)
        {
            _originalResource = originalResource;
        }

        public override IOStream OpenFile(string pathToFile, FileIOMode fileMode)
        {
            if (pathToFile == _originalResource.FileNameWithExtension)
            {
                var openedStream = _originalResource.OpenInputStream();

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
                    pathElements = pathToFile.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
                var link = _originalResource.GetForAnotherFile(
                    pathElements[pathElements.Length - 1], 
                    pathDirectoryBuilder.ToArray());
                var openedStream = link.OpenInputStream();

                return new AssimpIOStream(openedStream, pathToFile, fileMode);
            }
        }
    }
}
