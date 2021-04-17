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
using System;
using System.Collections.ObjectModel;
using System.Linq;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private RenderLoop _renderLoop;
        private SampleRepository _sampleRepo;
        private SampleSettings _sampleSettings;
        private string _selectedGroup;
        private SampleViewModel _selectedSample;

        public EventHandler ReloadRequest;

        public ObservableCollection<string> SampleGroups
        {
            get;
        } = new ObservableCollection<string>();

        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    this.RaisePropertyChanged(nameof(this.SelectedGroup));

                    this.UpdateSampleCollection();
                }
            }
        }

        public ObservableCollection<SampleViewModel> Samples
        {
            get;
        } = new ObservableCollection<SampleViewModel>();

        public SampleViewModel SelectedSample
        {
            get => _selectedSample;
            set
            {
                if (_selectedSample != value)
                {
                    if (_sampleSettings != null)
                    {
                        _sampleSettings.RecreateRequest -= this.OnSampleSettings_RecreateRequest;
                    }

                    _selectedSample = value;

                    if (_selectedSample == null) { _sampleSettings = null; }
                    else
                    {
                        _sampleSettings = _selectedSample.SampleMetadata.CreateSampleSettingsObject();
                        _sampleSettings.SetEnvironment(_renderLoop, _selectedSample.SampleMetadata);
                        _sampleSettings.RecreateRequest += this.OnSampleSettings_RecreateRequest;
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedSample));
                    this.RaisePropertyChanged(nameof(this.SampleSettings));

                    this.SampleCommands.Clear();
                    if (_sampleSettings != null)
                    {
                        foreach (var actSampleCommand in _sampleSettings.GetCommands())
                        {
                            this.SampleCommands.Add(actSampleCommand);
                        }
                    }
                }
            }
        }

        public ObservableCollection<SampleCommand> SampleCommands
        {
            get;
        } = new ObservableCollection<SampleCommand>();

        public SampleSettings SampleSettings => _sampleSettings;

        public MainWindowViewModel(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            _selectedGroup = string.Empty;

            _sampleSettings = null;
            _renderLoop = renderLoop;

            _renderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            // Load samples
            _sampleRepo = sampleRepo;
            foreach (var actSampleGroupName in _sampleRepo.SampleGroups
                .Select(actGroup => actGroup.GroupName))
            {
                this.SampleGroups.Add(actSampleGroupName);
            }
            this.SelectedGroup = this.SampleGroups.FirstOrDefault();
        }

        private void UpdateSampleCollection()
        {
            var sampleGroup = _sampleRepo.SampleGroups
                .FirstOrDefault(actGroup => actGroup.GroupName == _selectedGroup);
            if (sampleGroup == null) { return; }

            this.Samples.Clear();
            foreach (var actSampleMetadata in sampleGroup.Samples)
            {
                this.Samples.Add(new SampleViewModel(actSampleMetadata));
            }
            this.SelectedSample = this.Samples.FirstOrDefault();
        }

        private void OnSampleSettings_RecreateRequest(object sender, EventArgs e)
        {
            ReloadRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}