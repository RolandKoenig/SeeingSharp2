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

using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.UwpSamples
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RenderLoop _renderLoop;
        private SampleRepository _sampleRepo;
        private SampleSettings _sampleSettings;
        private string _selectedGroup;
        private SampleViewModel _selectedSample;

        public ObservableCollection<string> SampleGroups
        {
            get;
            private set;
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
            private set;
        } = new ObservableCollection<SampleViewModel>();

        public SampleViewModel SelectedSample
        {
            get => _selectedSample;
            set
            {
                if (_selectedSample != value)
                {
                    _selectedSample = value;
                    if (_selectedSample == null) { _sampleSettings = null; }
                    else
                    {
                        _sampleSettings = _selectedSample.SampleMetadata.CreateSampleSettingsObject();
                        _sampleSettings.SetEnvironment(_renderLoop, _selectedSample.SampleMetadata);
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
            private set;
        } = new ObservableCollection<SampleCommand>();

        public SampleSettings SampleSettings => _sampleSettings;

        public MainWindowViewModel()
        {
            if (!DesignMode.DesignModeEnabled) { return; }

            for (var loop = 1; loop < 5; loop++)
            {
                this.SampleGroups.Add($"DummyGroup {loop}");
            }
            this.SelectedGroup = "DummyGroup 2";

            for (var loop = 1; loop < 5; loop++)
            {
                this.Samples.Add(new SampleViewModel(new SampleMetadata(
                    new SampleDescriptionAttribute($"DummySample {loop}", 3, "DummyGroup 2"), this.GetType())));
            }
        }

        public void LoadSampleData(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            _selectedGroup = string.Empty;

            _sampleRepo = sampleRepo;
            _renderLoop = renderLoop;

            // Load samples
            foreach (var actSampleGroupName in _sampleRepo.SampleGroups
                .Select(actGroup => actGroup.GroupName))
            {
                this.SampleGroups.Add(actSampleGroupName);
            }
            this.SelectedGroup = this.SampleGroups.FirstOrDefault();

            _renderLoop.Filters.Add(new SceneViewboxObjectFilter());
        }

        private void UpdateSampleCollection()
        {
            if (DesignMode.DesignModeEnabled) { return; }

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
    }
}