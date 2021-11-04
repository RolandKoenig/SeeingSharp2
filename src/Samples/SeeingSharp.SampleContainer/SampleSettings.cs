using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettings : PropertyChangedBase
    {
        private int _lastRecreateRequestId;

        [Browsable(false)]
        public bool ThrottleRecreateRequest { get; set; } = true;

        protected RenderLoop RenderLoop
        {
            get;
            private set;
        }

        protected SampleMetadata SampleMetadata
        {
            get;
            private set;
        }
        public event EventHandler RecreateRequest;

        public virtual IEnumerable<SampleCommand> GetCommands()
        {
            yield return new SampleCommand(
                "Show Source",
                () => PlatformDependentMethods.OpenUrlInBrowser(this.SampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(this.SampleMetadata?.SourceCodeUrl),
                "Segoe MDL2 Assets", (char) 0xE71B);
        }

        public virtual void SetEnvironment(RenderLoop renderLoop, SampleMetadata sampleMetadata)
        {
            this.RenderLoop = renderLoop;
            this.SampleMetadata = sampleMetadata;

            // Trigger refresh of all properties
            this.RaisePropertyChanged();
        }

        protected async void RaiseRecreateRequest()
        {
            if (this.ThrottleRecreateRequest)
            {
                var myId = Interlocked.Increment(ref _lastRecreateRequestId);

                await Task.Delay(500);

                if (_lastRecreateRequestId == myId)
                {
                    this.RecreateRequest?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                this.RecreateRequest?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
