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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.Multimedia.Core
{
    public class SeeingSharpLoader : ISeeingSharpExtensionProvider
    {
        private List<ISeeingSharpExtensions> _extensions;

        public IEnumerable<ISeeingSharpExtensions> Extensions => _extensions;

        public SeeingSharpLoadSettings LoadSettings
        {
            get;
            set;
        }

        internal SeeingSharpLoader()
        {
            _extensions = new List<ISeeingSharpExtensions>();
            _extensions.Add(new DefaultImporterExporterExtensions());

            this.LoadSettings = new SeeingSharpLoadSettings();
        }

        public SeeingSharpLoader RegisterExtension(ISeeingSharpExtensions extensions)
        {
            extensions.EnsureNotNull(nameof(extensions));

            _extensions.Add(extensions);

            return this;
        }

        public void Load()
        {
            GraphicsCore.Load(this);
        }

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(() => GraphicsCore.Load(this));
        }

        public SeeingSharpLoader RegisterModelImporter(IModelImporter importer)
        {
            return this.RegisterExtension(
                new LoaderModelImporterExtension(importer));
        }

        public SeeingSharpLoader RegisterModelExporter(IModelExporter exporter)
        {
            return this.RegisterExtension(
                new LoaderModelExporterExtension(exporter));
        }

        public SeeingSharpLoader ConfigureLoading(SeeingSharpLoadSettings loadSettings)
        {
            loadSettings.EnsureNotNull(nameof(loadSettings));
            this.LoadSettings = loadSettings;

            return this;
        }

        public SeeingSharpLoader ConfigureLoading(Action<SeeingSharpLoadSettings> manipulateConfigAction)
        {
            manipulateConfigAction.EnsureNotNull(nameof(manipulateConfigAction));

            manipulateConfigAction(this.LoadSettings);
            return this;
        }

        /// <summary>
        /// Change some settings before loading the core the first time.
        /// </summary>
        /// <param name="manipulateCoreConfigAction">An action which gets the <see cref="GraphicsCoreConfiguration"/> object.</param>
        public SeeingSharpLoader ConfigureCore(Action<GraphicsCoreConfiguration> manipulateCoreConfigAction)
        {
            return this.RegisterExtension(
                new LoaderConfigurationExtension(manipulateCoreConfigAction, null, null));
        }

        /// <summary>
        /// Change some settings before loading a device the first time.
        /// </summary>
        /// <param name="manipulateDeviceConfigAction">An action which gets the <see cref="EngineAdapterInfo"/> which is loading currently and its <see cref="GraphicsDeviceConfiguration"/> object.</param>
        public SeeingSharpLoader ConfigureDevices(Action<EngineAdapterInfo, GraphicsDeviceConfiguration> manipulateDeviceConfigAction)
        {
            return this.RegisterExtension(
                new LoaderConfigurationExtension(null, manipulateDeviceConfigAction, null));
        }

        /// <summary>
        /// Change some settings before loading a view the first time.
        /// </summary>
        /// <param name="manipulateViewConfigAction">An action which gets the <see cref="RenderLoop"/> which is loading currently and its <see cref="GraphicsViewConfiguration"/> object.</param>
        public SeeingSharpLoader ConfigureViews(Action<RenderLoop, GraphicsViewConfiguration> manipulateViewConfigAction)
        {
            return this.RegisterExtension(
                new LoaderConfigurationExtension(null, null, manipulateViewConfigAction));
        }

        public SeeingSharpLoader EnableDirectXDebugMode()
        {
            return this.ConfigureLoading(
                loadSettings => loadSettings.DebugEnabled = true);
        }
    }
}
