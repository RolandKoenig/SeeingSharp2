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
    using SeeingSharp.Util;

    #endregion

    public class SampleMetadata
    {
        private SampleDescriptionAttribute m_description;
        private Type m_sampleType;

        public SampleMetadata()
        {

        }

        public SampleMetadata(SampleDescriptionAttribute description, Type sampleType)
        {
            m_description = description;
            m_sampleType = sampleType;

            this.Name = m_description.SampleName;
            this.OrderId = m_description.OrderID;
            this.Group = m_description.SampleGroupName;
            this.SourceCodeUrl = m_description.SourceCodeUrl;
        }

        public SampleBase CreateSampleObject()
        {
            if(m_sampleType == null)
            {
                throw new ApplicationException($"No sample type given!");
            }

            var result = Activator.CreateInstance(m_sampleType) as SampleBase;
            if(result == null) { throw new ApplicationException($"Sample type {m_sampleType.FullName} is not derived from {nameof(SampleBase)}!"); }

            return result;
        }

        public SampleSettings CreateSampleSettingsObject()
        {
            var settingsType = m_description.SettingsType;

            if (settingsType == null)
            {
                return new SampleSettings();
            }

            var result = Activator.CreateInstance(settingsType) as SampleSettings;

            if (result == null)
            {
                throw new ApplicationException($"SampleSettings type {m_sampleType.FullName} is not derived from {nameof(SampleSettings)}!");
            }

            return result;
        }

        public AssemblyResourceLink TryGetSampleImageLink()
        {
            if (m_description == null) { return null; }
            if (string.IsNullOrWhiteSpace((m_description.SampleImageFileName))) { return null; }

            return new AssemblyResourceLink(
                m_sampleType,
                m_description.SampleImageFileName);
        }

        public string Name
        {
            get;
            set;
        } = string.Empty;

        public int OrderId
        {
            get;
            set;
        } = 0;

        public string Group
        {
            get;
            set;
        } = string.Empty;

        public string SourceCodeUrl
        {
            get;
            set;
        } = string.Empty;
    }
}