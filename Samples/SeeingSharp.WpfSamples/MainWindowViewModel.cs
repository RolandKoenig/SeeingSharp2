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
        private RenderLoop m_renderLoop;

        public MainWindowViewModel(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            m_selectedGroup = string.Empty;

            SampleSettings = null;
            m_renderLoop = renderLoop;

            // Load samples
            m_sampleRepo = sampleRepo;

            foreach (var actSampleGroupName in m_sampleRepo.SampleGroups
                .Select((actGroup) => actGroup.GroupName))
            {
                SampleGroups.Add(actSampleGroupName);
            }

            SelectedGroup = SampleGroups.FirstOrDefault();
        }

        private void UpdateSampleCollection()
        {
            var sampleGroup = m_sampleRepo.SampleGroups
                .Where((actGroup) => actGroup.GroupName == m_selectedGroup)
                .FirstOrDefault();

            if (sampleGroup == null)
            {
                return;
            }

            Samples.Clear();

            foreach (var actSampleMetadata in sampleGroup.Samples)
            {
                Samples.Add(new SampleViewModel(actSampleMetadata));
            }

            SelectedSample = Samples.FirstOrDefault();
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
                if (m_selectedGroup == value)
                {
                    return;
                }

                m_selectedGroup = value;
                RaisePropertyChanged(nameof(SelectedGroup));

                UpdateSampleCollection();
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
                if (m_selectedSample == value)
                {
                    return;
                }

                m_selectedSample = value;

                if (m_selectedSample == null)
                {
                    SampleSettings = null;
                }
                else
                {
                    SampleSettings = m_selectedSample.SampleMetadata.CreateSampleSettingsObject();
                    SampleSettings.SetEnvironment(m_renderLoop, m_selectedSample.SampleMetadata);
                }

                RaisePropertyChanged(nameof(SelectedSample));
                RaisePropertyChanged(nameof(SampleSettings));

                SampleCommands.Clear();

                if (SampleSettings == null)
                {
                    return;
                }

                foreach (var actSampleCommand in SampleSettings.GetCommands())
                {
                    SampleCommands.Add(actSampleCommand);
                }
            }
        }

        public ObservableCollection<SampleCommand> SampleCommands
        {
            get;
            private set;
        } = new ObservableCollection<SampleCommand>();

        public SampleSettings SampleSettings { get; private set; }
    }
}