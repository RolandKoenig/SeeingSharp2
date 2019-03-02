#region License information
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
#endregion

using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.UwpSamples
{
    #region using
    #endregion

    public class MainWindowViewModel : ViewModelBase
    {
        private RenderLoop m_renderLoop;
        private SampleRepository m_sampleRepo;
        private SampleSettings m_sampleSettings;
        private string m_selectedGroup;
        private SampleViewModel m_selectedSample;

        public void LoadSampleData(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            m_selectedGroup = string.Empty;

            m_sampleRepo = sampleRepo;
            m_renderLoop = renderLoop;

            // Load samples
            foreach(var actSampleGroupName in m_sampleRepo.SampleGroups
                .Select(actGroup => actGroup.GroupName))
            {
                SampleGroups.Add(actSampleGroupName);
            }
            SelectedGroup = SampleGroups.FirstOrDefault();
        }

        private void UpdateSampleCollection()
        {
            if (DesignMode.DesignModeEnabled) { return; }

            var sampleGroup = m_sampleRepo.SampleGroups
                .Where(actGroup => actGroup.GroupName == m_selectedGroup)
                .FirstOrDefault();
            if (sampleGroup == null) { return; }

            Samples.Clear();
            foreach(var actSampleMetadata in sampleGroup.Samples)
            {
                Samples.Add(new SampleViewModel(actSampleMetadata));
            }
            SelectedSample = Samples.FirstOrDefault();
        }

        public MainWindowViewModel()
        {
            if (!DesignMode.DesignModeEnabled) { return; }

            for (var loop = 1; loop < 5; loop++)
            {
                SampleGroups.Add($"DummyGroup {loop}");
            }
            SelectedGroup = "DummyGroup 2";

            for (var loop=1; loop<5; loop++)
            {
                Samples.Add(new SampleViewModel(new SampleMetadata(
                    new SampleDescriptionAttribute($"DummySample {loop}", 3, "DummyGroup 2"),
                    GetType())));
            }
        }

        public ObservableCollection<string> SampleGroups
        {
            get;
            private set;
        } = new ObservableCollection<string>();

        public string SelectedGroup
        {
            get => m_selectedGroup;
            set
            {
                if(m_selectedGroup != value)
                {
                    m_selectedGroup = value;
                    RaisePropertyChanged(nameof(SelectedGroup));

                    UpdateSampleCollection();
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
            get => m_selectedSample;
            set
            {
                if(m_selectedSample != value)
                {
                    m_selectedSample = value;
                    if (m_selectedSample == null) { m_sampleSettings = null; }
                    else
                    {
                        m_sampleSettings = m_selectedSample.SampleMetadata.CreateSampleSettingsObject();
                        m_sampleSettings.SetEnvironment(m_renderLoop, m_selectedSample.SampleMetadata);
                    }

                    RaisePropertyChanged(nameof(SelectedSample));
                    RaisePropertyChanged(nameof(SampleSettings));

                    SampleCommands.Clear();
                    if (m_sampleSettings != null)
                    {
                        foreach (var actSampleCommand in m_sampleSettings.GetCommands())
                        {
                            SampleCommands.Add(actSampleCommand);
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

        public SampleSettings SampleSettings => m_sampleSettings;
    }
}