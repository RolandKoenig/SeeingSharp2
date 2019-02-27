#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

#region using

using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    public class EngineFactory
    {
        public EngineFactory(DeviceLoadSettings loadSettings)
        {
            WindowsImagingComponent = new FactoryHandlerWIC(loadSettings);
            Direct2D = new FactoryHandlerD2D(loadSettings);
            DirectWrite = new FactoryHandlerDWrite(loadSettings);
        }

        public FactoryHandlerD2D Direct2D { get; }

        public FactoryHandlerDWrite DirectWrite { get; }

        public FactoryHandlerWIC WindowsImagingComponent { get; }

        internal D2D.Factory2 FactoryD2D_2 => Direct2D.Factory2;
    }
}