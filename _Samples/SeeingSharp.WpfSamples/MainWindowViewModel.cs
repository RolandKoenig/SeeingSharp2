﻿using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SeeingSharp.WpfSamples
{
    public class MainWindowViewModel : ViewModelBase
    {
        private SampleRepository m_sampleRepo;
        private string m_selectedGroup;
        private SampleViewModel m_selectedSample;
        private Visibility m_settingsVisibility;
        private SettingsViewModel m_settingsVM;
        private RenderLoop m_renderLoop;

        public MainWindowViewModel(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            m_selectedGroup = string.Empty;

            m_settingsVisibility = Visibility.Collapsed;
            m_settingsVM = null;
            m_renderLoop = renderLoop;

            // Load samples
            m_sampleRepo = sampleRepo;
            foreach(string actSampleGroupName in m_sampleRepo.SampleGroups
                .Select((actGroup) => actGroup.GroupName))
            {
                this.SampleGroups.Add(actSampleGroupName);
            }
            this.SelectedGroup = this.SampleGroups.FirstOrDefault();

            this.Command_ToggleOptionsVisibility = new DelegateCommand(
                () => this.SetttingsVisibility = this.SetttingsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
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
                    RaisePropertyChanged(nameof(SelectedSample));

                    if(m_selectedSample == null) { this.SettingsVM = null; }
                    else { this.SettingsVM = new SettingsViewModel(m_renderLoop, m_selectedSample.Sample); }
                }
            }
        }

        public ICommand Command_ToggleOptionsVisibility
        {
            get;
            private set;
        }

        public Visibility SetttingsVisibility
        {
            get => m_settingsVisibility;
            set
            {
                if(m_settingsVisibility != value)
                {
                    m_settingsVisibility = value;
                    RaisePropertyChanged(nameof(SetttingsVisibility));
                }
            }
        }

        public SettingsViewModel SettingsVM
        {
            get => m_settingsVM;
            set
            {
                if(m_settingsVM != value)
                {
                    m_settingsVM = value;
                    RaisePropertyChanged(nameof(SettingsVM));
                }
            }
        }
    }
}
