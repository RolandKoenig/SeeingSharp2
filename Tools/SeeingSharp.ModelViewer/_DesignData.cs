using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SeeingSharp.Multimedia.Core;
using FakeItEasy;

namespace SeeingSharp.ModelViewer
{
    public static class DesignData
    {
        public static MainWindowVM MainWindowVM
        {
            get
            {
                var renderLoopHost = A.Fake<IRenderLoopHost>();

                return new MainWindowVM(new RenderLoop(
                    new SynchronizationContext(),
                    renderLoopHost,
                    isDesignMode: true));
            }
        }
    }
}
