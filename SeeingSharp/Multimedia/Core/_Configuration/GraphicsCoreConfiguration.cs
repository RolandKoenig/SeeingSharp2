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
    public class GraphicsCoreConfiguration
    {
        /// <summary>
        /// Can enable debug mode for all created devices.
        /// This value can only be manipulated when loading SeeingSharp.
        /// </summary>
        public bool DebugEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// False in most cases. This property can be used to simulate an initialization error when loading Direct2D.
        /// This value can only be manipulated when loading SeeingSharp.
        /// </summary>
        public bool ThrowD2DInitDeviceError
        {
            get;
            internal set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsCoreConfiguration" /> class.
        /// </summary>
        public GraphicsCoreConfiguration()
        {
            this.DebugEnabled = false;
        }
    }
}
