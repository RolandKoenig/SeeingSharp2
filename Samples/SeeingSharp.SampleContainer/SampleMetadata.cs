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
using SeeingSharp.Util;
using System;

namespace SeeingSharp.SampleContainer
{
    public class SampleMetadata
    {
        private SampleDescriptionAttribute _description;
        private Type _sampleType;

        public SampleMetadata(SampleDescriptionAttribute description, Type sampleType)
        {
            _description = description;
            _sampleType = sampleType;

            this.Name = _description.SampleName;
            this.OrderId = _description.OrderId;
            this.Group = _description.SampleGroupName;
            this.SourceCodeUrl = _description.SourceCodeUrl;
        }

        public SampleBase CreateSampleObject()
        {
            if (_sampleType == null)
            {
                throw new ApplicationException("No sample type given!");
            }

            var result = Activator.CreateInstance(_sampleType) as SampleBase;
            if (result == null) { throw new ApplicationException($"Sample type {_sampleType.FullName} is not derived from {nameof(SampleBase)}!"); }

            return result;
        }

        public SampleSettings CreateSampleSettingsObject()
        {
            var settingsType = _description.SettingsType;

            if (settingsType == null)
            {
                return new SampleSettings();
            }

            var result = Activator.CreateInstance(settingsType) as SampleSettings;

            if (result == null)
            {
                throw new ApplicationException($"SampleSettings type {_sampleType.FullName} is not derived from {nameof(SampleSettings)}!");
            }

            return result;
        }

        public AssemblyResourceLink TryGetSampleImageLink()
        {
            if (_description == null) { return null; }
            if (string.IsNullOrWhiteSpace(_description.SampleImageFileName)) { return null; }

            return new AssemblyResourceLink(
                _sampleType,
                _description.SampleImageFileName);
        }

        public string Name
        {
            get;
            set;
        }

        public int OrderId
        {
            get;
            set;
        }

        public string Group
        {
            get;
            set;
        }

        public string SourceCodeUrl
        {
            get;
            set;
        }
    }
}