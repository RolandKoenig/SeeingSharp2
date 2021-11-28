using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeeingSharp.WinUIDesktopSamples
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}