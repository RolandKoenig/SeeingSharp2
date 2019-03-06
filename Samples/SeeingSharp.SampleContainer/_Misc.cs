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

namespace SeeingSharp.SampleContainer
{
    #region using

    using System;

    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class SampleDescriptionAttribute : Attribute
    {
        public SampleDescriptionAttribute(string sampleName, int orderID, string sampleGroupName, string sampleImageFileName = "", string sourceCodeUrl = "", Type settingsType = null)
        {
            SampleName = sampleName;
            OrderID = orderID;
            SampleGroupName = sampleGroupName;
            SampleImageFileName = sampleImageFileName;
            SourceCodeUrl = sourceCodeUrl;
            SettingsType = settingsType;
        }

        public string SampleName
        {
            get;
            private set;
        }

        public int OrderID
        {
            get;
            private set;
        }

        public string SampleGroupName
        {
            get;
            private set;
        }

        public string SampleImageFileName
        {
            get;
            private set;
        }

        public string SourceCodeUrl
        {
            get;
            private set;
        }

        public Type SettingsType
        {
            get;
            private set;
        }
    }
}