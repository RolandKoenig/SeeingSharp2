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

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Checking;
    using Objects;

    #endregion

    public class SeeingSharpLoader
    {
        private List<ISeeingSharpExtensions> m_extensions;

        internal SeeingSharpLoader()
        {
            m_extensions = new List<ISeeingSharpExtensions>();
            m_extensions.Add(new DefaultImporterExporterExtensions());

            this.LoadSettings = new DeviceLoadSettings();
        }

        public void RegisterExtensions(ISeeingSharpExtensions extensions)
        {
            extensions.EnsureNotNull(nameof(extensions));

            m_extensions.Add(extensions);
        }

        public void Load()
        {
            GraphicsCore.Load(this);
        }

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(() => GraphicsCore.Load(this));
        }

        public SeeingSharpLoader Configure(DeviceLoadSettings loadSettings)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            this.LoadSettings = loadSettings;

            return this;
        }

        public SeeingSharpLoader Configure(Action<DeviceLoadSettings> manipulateConfigAction)
        {
            manipulateConfigAction.EnsureNotNull(nameof(manipulateConfigAction));

            manipulateConfigAction(this.LoadSettings);
            return this;
        }

        public SeeingSharpLoader EnableDirectXDebugMode()
        {
            return this.Configure(
                loadSettings => loadSettings.DebugEnabled = true);
        }

        public IEnumerable<ISeeingSharpExtensions> Extensions
        {
            get => m_extensions;
        }

        public DeviceLoadSettings LoadSettings
        {
            get;
            set;
        }
    }
}
