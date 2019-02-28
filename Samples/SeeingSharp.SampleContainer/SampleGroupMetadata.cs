#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
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
namespace SeeingSharp.SampleContainer
{
    #region using

    using System.Collections.Generic;

    #endregion

    public class SampleGroupMetadata
    {
        public SampleGroupMetadata(string groupName)
        {
            this.Samples = new List<SampleMetadata>();

            this.GroupName = groupName;
        }

        public string GroupName
        {
            get;
            private set;
        }

        public List<SampleMetadata> Samples
        {
            get;
            private set;
        }
    }
}