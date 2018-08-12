#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeeingSharp.Util
{
    /// <summary>
    /// A class that helps for locating and loading assembly resource files.
    /// </summary>
    public class AssemblyResourceLink
    {
        private Assembly m_targetAssembly;
        private string m_resourceNamespace;
        private string m_resourceFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResourceLink"/> class.
        /// </summary>
        /// <param name="targetAssembly">The target assembly.</param>
        /// <param name="resourceNamespace">The namespace of the resource.</param>
        /// <param name="resourceFile">The resource file.</param>
        public AssemblyResourceLink(Assembly targetAssembly, string resourceNamespace, string resourceFile)
        {
            if (string.IsNullOrEmpty(resourceNamespace)) { throw new ArgumentNullException("resourceNamespace"); }
            if (string.IsNullOrEmpty(resourceFile)) { throw new ArgumentNullException("resourceFile"); }

            m_targetAssembly = targetAssembly;
            m_resourceNamespace = resourceNamespace;
            m_resourceFile = resourceFile;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder resultBuilder = new StringBuilder();
            if (m_targetAssembly != null) { resultBuilder.Append(m_targetAssembly.GetName().Name + ": "); }
            resultBuilder.Append(m_resourceNamespace);
            resultBuilder.Append(".");
            resultBuilder.Append(m_resourceFile);
            return resultBuilder.ToString();
        }

        /// <summary>
        /// Gets the resource link for another file within the same assembly and namespace.
        /// </summary>
        /// <param name="fileName">The filename for which to get the link.</param>
        /// <param name="subdirectories">The subdirectory path to the file (optional).</param>
        public AssemblyResourceLink GetForAnotherFile(string fileName, params string[] subdirectories)
        {
            string subdirectoryPath = string.Empty;
            for(int loop=0; loop<subdirectories.Length; loop++)
            {
                subdirectoryPath += subdirectories[loop] + ".";
            }

            return new AssemblyResourceLink(
                m_targetAssembly,
                m_resourceNamespace,
                subdirectoryPath + fileName);
        }

        /// <summary>
        /// Opens the resource for reading.
        /// </summary>
        public Stream OpenRead()
        {
            return m_targetAssembly.GetManifestResourceStream(this.ResourcePath);
        }

        /// <summary>
        /// Is this link valid?
        /// </summary>
        public bool IsValid()
        {
            var resourceInfo = m_targetAssembly.GetManifestResourceInfo(this.ResourcePath);
            return resourceInfo != null;
        }

        /// <summary>
        /// Gets the raw resource in text form.
        /// </summary>
        public string GetText()
        {
            using(Stream inStream = OpenRead())
            using(StreamReader inStreamReader = new StreamReader(inStream))
            {
                return inStreamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the target assembly.
        /// </summary>
        public Assembly TargetAssembly
        {
            get { return m_targetAssembly; }
        }

        /// <summary>
        /// Gets the namespace of the resource.
        /// </summary>
        public string ResourceNamespace
        {
            get { return m_resourceNamespace; }
        }

        /// <summary>
        /// Gets the name of the file (without namespace).
        /// </summary>
        public string ResourceFile
        {
            get { return m_resourceFile; }
        }

        /// <summary>
        /// Gets the resource path.
        /// </summary>
        public string ResourcePath
        {
            get 
            {
                StringBuilder resultBuilder = new StringBuilder();
                resultBuilder.Append(m_resourceNamespace);
                resultBuilder.Append(".");
                resultBuilder.Append(m_resourceFile);
                return resultBuilder.ToString();
            }
        }
    }
}
