/*
    SeeingSharp and all applications distributed together with it. 
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
namespace SeeingSharp.Multimedia.Core
{
    public class GraphicsDeviceConfiguration
    {
        /// <summary>
        /// Gets or sets the texture quality level.
        /// </summary>
        public TextureQuality TextureQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the geometry quality level.
        /// </summary>
        public GeometryQuality GeometryQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        public GraphicsCoreConfiguration CoreConfiguration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceConfiguration" /> class.
        /// </summary>
        /// <param name="coreConfig">The core configuration object.</param>
        public GraphicsDeviceConfiguration(GraphicsCoreConfiguration coreConfig)
        {
            this.CoreConfiguration = coreConfig;
        }
    }
}