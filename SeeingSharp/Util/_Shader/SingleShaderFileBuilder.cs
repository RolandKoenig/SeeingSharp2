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
using System.Text;

namespace SeeingSharp.Util
{
    public static class SingleShaderFileBuilder
    {
        /// <summary>
        /// Reads the given shader resource and write all contents into the target <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="resourceLink">Source file of the shader.</param>
        /// <param name="target">The target <see cref="StringBuilder"/> to write the shader source to.</param>
        public static void ReadShaderFileAndResolveIncludes(ResourceLink resourceLink, StringBuilder target)
        {
            using (var stringWriter = new StringWriter(target))
            {
                ReadShaderFileAndResolveIncludes(resourceLink, stringWriter);
            }
        }

        /// <summary>
        /// Reads the given shader resource and write all contents into the target <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="resourceLink">Source file of the shader.</param>
        /// <param name="target">The target <see cref="StringWriter"/> to write the shader source to.</param>
        public static void ReadShaderFileAndResolveIncludes(ResourceLink resourceLink, StringWriter target)
        {
            using (var streamReader = new StreamReader(resourceLink.OpenInputStream()))
            {
                string actLine = null;
                while(null != (actLine = streamReader.ReadLine()))
                {
                    // Handle all lines except include
                    if (!actLine.StartsWith("#include"))
                    {
                        target.WriteLine(actLine);
                        continue;
                    }

                    // Handle include line
                    var indexStringStart = actLine.IndexOf('"');
                    var indexStringEnd = actLine.LastIndexOf('"');
                    if(indexStringStart < 0) { continue; }
                    if(indexStringStart == indexStringEnd) { continue; }

                    var includeFilePathLength = indexStringEnd - indexStringStart - 1;
                    if(includeFilePathLength <= 0) { continue; }

                    // Get full include file path in split it using path separators
                    var includeFilePath = actLine.Substring(indexStringStart + 1, includeFilePathLength);
                    var includeFilePathSplitted = includeFilePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                    // Build the link to the include file
                    var fileName = includeFilePathSplitted[includeFilePathSplitted.Length - 1];
                    ResourceLink resLinkInner = null;
                    if (includeFilePathSplitted.Length < 2)
                    {
                        resLinkInner = resourceLink.GetForAnotherFile(fileName);
                    }
                    else
                    {
                        resLinkInner = resourceLink.GetForAnotherFile(
                            fileName,
                            includeFilePathSplitted.Subset(0, includeFilePathSplitted.Length - 1));
                    }

                    // Include the source file
                    target.WriteLine($"// ######### Start of include file {includeFilePath}");
                    ReadShaderFileAndResolveIncludes(resLinkInner, target);
                    target.WriteLine($"// ######### End of include file {includeFilePath}");
                }
            }
        }
    }
}
