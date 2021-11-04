using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using SeeingSharp.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.UwpSamples
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RenderLoop _renderLoop;
        private SampleRepository _sampleRepo;
        private SampleSettings _sampleSettings;
        private SampleGroupMetadata _selectedGroup;
        private SampleViewModel _selectedSample;

        public ObservableCollection<SampleGroupMetadata> SampleGroups
        {
            get;
        } = new ObservableCollection<SampleGroupMetadata>();

        public SampleGroupMetadata SelectedGroup
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
                        this.SampleCommands.Add(new SampleCommand(
                            "New Child Window",
                            () => this.NewChildWindowRequest?.Invoke(this, EventArgs.Empty), 
                            () => true,
                            "Segoe MDL2 Assets", (char) 0xE78B));

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

        public event EventHandler NewChildWindowRequest;

        public void LoadSampleData(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            _selectedGroup = null;

            _sampleRepo = sampleRepo;
            _renderLoop = renderLoop;

            // Load samples
            foreach (var actSampleGroup in _sampleRepo.SampleGroups)
            {
                this.SampleGroups.Add(actSampleGroup);
            }
            this.SelectedGroup = this.SampleGroups.FirstOrDefault();
        }

        private void UpdateSampleCollection()
        {
            if (DesignMode.DesignModeEnabled) { return; }

            var sampleGroup = _sampleRepo.SampleGroups
                .FirstOrDefault(actGroup => actGroup == _selectedGroup);
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