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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettings : PropertyChangedBase
    {
        public event EventHandler RecreateRequest;

        private int _lastRecreateRequestID;

        public virtual IEnumerable<SampleCommand> GetCommands()
        {
            yield return new SampleCommand(
                "Show Source",
                () => PlatformDependentMethods.OpenUrlInBrowser(this.SampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(this.SampleMetadata?.SourceCodeUrl));
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
                var myID = Interlocked.Increment(ref _lastRecreateRequestID);

                await Task.Delay(2000);

                if (_lastRecreateRequestID == myID)
                {
                    this.RecreateRequest?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                this.RecreateRequest?.Invoke(this, EventArgs.Empty);
            }
        }

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
    }
}
