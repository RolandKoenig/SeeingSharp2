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
    /// <summary>
    /// Helper structure which holds information about all currently
    /// attached components.
    /// </summary>
    internal struct SceneComponentInfo
    {
        public SceneComponentBase Component;
        public object Context;
        public ViewInformation CorrespondingView;
    }

    /// <summary>
    /// Helper structure which holds information about a request
    /// for a component (attach, detach) which normally comes
    /// from the UI thread
    /// </summary>
    internal struct SceneComponentRequest
    {
        public SceneComponentRequestType RequestType;
        public SceneComponentBase Component;
        public ViewInformation CorrespondingView;
    }

    /// <summary>
    /// The type of a component request.
    /// </summary>
    internal enum SceneComponentRequestType
    {
        Attach,
        Detach,
        DetachAll
    }
}
