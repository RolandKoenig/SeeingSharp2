using SeeingSharp.Multimedia.Core;
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
        private Visibility m_sampleSettingsVisibility;
        private SampleSettings m_sampleSettings;
        private RenderLoop m_renderLoop;

        public MainWindowViewModel(SampleRepository sampleRepo, RenderLoop renderLoop)
        {
            m_selectedGroup = string.Empty;

            m_sampleSettingsVisibility = Visibility.Collapsed;
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

            this.Command_ToggleSampleSettingsVisibility = new DelegateCommand(
                () => this.SampleSettingsVisibility = this.SampleSettingsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
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
                }
            }
        }

        public ICommand Command_ToggleSampleSettingsVisibility
        {
            get;
            private set;
        }

        public Visibility SampleSettingsVisibility
        {
            get => m_sampleSettingsVisibility;
            set
            {
                if(m_sampleSettingsVisibility != value)
                {
                    m_sampleSettingsVisibility = value;
                    RaisePropertyChanged(nameof(SampleSettingsVisibility));
                }
            }
        }

        public SampleSettings SampleSettings
        {
            get => m_sampleSettings;
        }
    }
}
