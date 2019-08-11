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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    /// <summary>
    /// A class that helps for locating and loading assembly resource files.
    /// </summary>
    public class AssemblyResourceLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResourceLink"/> class.
        /// </summary>
        /// <param name="targetAssembly">The target assembly.</param>
        /// <param name="resourceNamespace">The namespace of the resource.</param>
        /// <param name="resourceFile">The resource file.</param>
        public AssemblyResourceLink(Assembly targetAssembly, string resourceNamespace, string resourceFile)
        {
            targetAssembly.EnsureNotNull(nameof(targetAssembly));
            resourceNamespace.EnsureNotNullOrEmpty(nameof(resourceNamespace));
            resourceFile.EnsureNotNullOrEmpty(nameof(resourceFile));

            this.TargetAssembly = targetAssembly;
            this.ResourceNamespace = resourceNamespace;
            this.ResourceFile = resourceFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResourceLink"/> class.
        /// </summary>
        /// <param name="type">The type from which to get the assembly and namespace.</param>
        /// <param name="resourceFile">The resource file.</param>
        public AssemblyResourceLink(Type type, string resourceFile)
            : this(type.GetTypeInfo().Assembly, type.Namespace, resourceFile)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResourceLink"/> class.
        /// </summary>
        /// <param name="type">The type from which to get the assembly and namespace.</param>
        /// <param name="deeperNamespace">The deeper namespace onwards from the given type's namespace.</param>
        /// <param name="resourceFile">The resource file.</param>
        public AssemblyResourceLink(Type type, string deeperNamespace, string resourceFile)
            : this(type.GetTypeInfo().Assembly, type.Namespace + "." + deeperNamespace, resourceFile)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder();

            if (this.TargetAssembly != null)
            {
                resultBuilder.Append(this.TargetAssembly.GetName().Name + ": ");
            }

            resultBuilder.Append(this.ResourceNamespace);
            resultBuilder.Append(".");
            resultBuilder.Append(this.ResourceFile);
            return resultBuilder.ToString();
        }

        /// <summary>
        /// Gets the resource link for another file within the same assembly and namespace.
        /// </summary>
        /// <param name="fileName">The filename for which to get the link.</param>
        /// <param name="subdirectories">The sub directory path to the file (optional).</param>
        public AssemblyResourceLink GetForAnotherFile(string fileName, params string[] subdirectories)
        {
            // Build new namespace
            var newTargetNamespace = this.ResourceNamespace;
            if (subdirectories.Length > 0)
            {
                // Build a stack representing the current namespace path
                var currentDirectoryParts = this.ResourceNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var currentDirectoryPathStack = new Stack<string>(currentDirectoryParts.Length);
                for(var loop=0; loop<currentDirectoryParts.Length; loop++)
                {
                    currentDirectoryPathStack.Push(currentDirectoryParts[loop]);
                }

                // Modify the stack using given subdirectories
                foreach (var actSubdirectory in subdirectories)
                {
                    switch (actSubdirectory)
                    {
                        case ".":
                            // Nothing to do.. directory remains the same
                            break;

                        case "..":
                            // Go one level down
                            if (currentDirectoryPathStack.Count <= 0)
                            {
                                var requestedSubDirectoryPath = subdirectories.ToCommaSeparatedString("/");
                                throw new SeeingSharpException($"Unable to go one level down in directory path. Initial namespace: {this.ResourceNamespace}, Requested path: {requestedSubDirectoryPath}");
                            }
                            currentDirectoryPathStack.Pop();
                            break;

                        default:
                            // Go one level up
                            currentDirectoryPathStack.Push(actSubdirectory);
                            break;
                    }
                }

                // Generate new target namespace out of the stack
                newTargetNamespace = currentDirectoryPathStack
                    .Reverse()
                    .ToCommaSeparatedString(".");
            }

            // Build new resource link
            return new AssemblyResourceLink(this.TargetAssembly,
                newTargetNamespace,
                fileName);
        }

        /// <summary>
        /// Opens the resource for reading.
        /// </summary>
        public Stream OpenRead()
        {
            var result = this.TargetAssembly.GetManifestResourceStream(this.ResourcePath);
            if(result == null) { throw new SeeingSharpException($"Resource {this.ResourcePath} not found in assembly {this.TargetAssembly.FullName}!"); }
            return result;
        }

        /// <summary>
        /// Is this link valid?
        /// </summary>
        public bool IsValid()
        {
            var resourceInfo = this.TargetAssembly.GetManifestResourceInfo(this.ResourcePath);
            return resourceInfo != null;
        }

        /// <summary>
        /// Gets the raw resource in text form.
        /// </summary>
        public string GetText()
        {
            using(var inStream = this.OpenRead())
            using(var inStreamReader = new StreamReader(inStream))
            {
                return inStreamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the target assembly.
        /// </summary>
        public Assembly TargetAssembly { get; }

        /// <summary>
        /// Gets the namespace of the resource.
        /// </summary>
        public string ResourceNamespace { get; }

        /// <summary>
        /// Gets the name of the file (without namespace).
        /// </summary>
        public string ResourceFile { get; }

        /// <summary>
        /// Gets the resource path.
        /// </summary>
        public string ResourcePath
        {
            get
            {
                var resultBuilder = new StringBuilder();
                resultBuilder.Append(this.ResourceNamespace);
                resultBuilder.Append(".");
                resultBuilder.Append(this.ResourceFile);
                return resultBuilder.ToString();
            }
        }
    }
}