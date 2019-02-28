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
namespace SeeingSharp.WpfSamples
{
    #region using

    using System.Collections.ObjectModel;
    using System.Linq;
    using Multimedia.Core;
    using SampleContainer;
    using SampleContainer.Util;

    #endregion

    public class MainWindowViewModel : PropertyChangedBase
    {
        private SampleRepository m_sampleRepo;
        private string m_selectedGroup;
        private SampleViewModel m_selectedSample;
        private SampleSettings m_sampleSettings;
        private RenderLoop m_renderLoop;

        public MainWindowViewModel(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            m_selectedGroup = string.Empty;

            m_sampleSettings = null;
            m_renderLoop = renderLoop;

            // Load samples
            m_sampleRepo = sampleRepo;
            foreach(string actSampleGroupName in m_sampleRepo.SampleGroups
                .Select((actGroup) => actGroup.GroupName))
            {
                this.SampleGroups.Add(actSampleGroupName);
            }
            this.SelectedGroup = this.SampleGroups.FirstOrDefault();
        }

        private void UpdateSampleCollection()
        {
            var sampleGroup = m_sampleRepo.SampleGroups
                .Where((actGroup) => actGroup.GroupName == m_selectedGroup)
                .FirstOrDefault();
            if (sampleGroup == null) { return; }

            this.Samples.Clear();
            foreach(var actSampleMetadata in sampleGroup.Samples)
            {
                this.Samples.Add(new SampleViewModel(actSampleMetadata));
            }
            this.SelectedSample = this.Samples.FirstOrDefault();
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
            get => m_selectedSample;
            set
            {
                if(m_selectedSample != value)
                {
                    m_selectedSample = value;

                    if(m_selectedSample == null) { m_sampleSettings = null; }
                    else
                    {
                        m_sampleSettings = m_selectedSample.SampleMetadata.CreateSampleSettingsObject();
                        m_sampleSettings.SetEnvironment(m_renderLoop, m_selectedSample.SampleMetadata);
                    }

                    RaisePropertyChanged(nameof(SelectedSample));
                    RaisePropertyChanged(nameof(SampleSettings));

                    this.SampleCommands.Clear();
                    if (m_sampleSettings != null)
                    {
                        foreach (var actSampleCommand in m_sampleSettings.GetCommands())
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

        public SampleSettings SampleSettings
        {
            get => m_sampleSettings;
        }
    }
}