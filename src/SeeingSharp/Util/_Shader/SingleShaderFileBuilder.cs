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
                while (null != (actLine = streamReader.ReadLine()))
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
                    if (indexStringStart < 0) { continue; }
                    if (indexStringStart == indexStringEnd) { continue; }

                    var includeFilePathLength = indexStringEnd - indexStringStart - 1;
                    if (includeFilePathLength <= 0) { continue; }

                    // Get full include file path in split it using path separators
                    var includeFilePath = actLine.Substring(indexStringStart + 1, includeFilePathLength);
                    var includeFilePathParts = includeFilePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                    // Build the link to the include file
                    var fileName = includeFilePathParts[includeFilePathParts.Length - 1];
                    ResourceLink resLinkInner = null;
                    if (includeFilePathParts.Length < 2)
                    {
                        resLinkInner = resourceLink.GetForAnotherFile(fileName);
                    }
                    else
                    {
                        resLinkInner = resourceLink.GetForAnotherFile(
                            fileName,
                            includeFilePathParts.Subset(0, includeFilePathParts.Length - 1));
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
