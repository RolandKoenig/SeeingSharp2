using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public static class DesignData
    {
        public static MainWindowVM MainWindowVM
        {
            get
            {
                var renderLoopHost = A.Fake<IRenderLoopHost>();

                var result = new MainWindowVM(new RenderLoop(
                    new SynchronizationContext(),
                    renderLoopHost,
                    true));
                result.IsLoading = true;
                return result;
            }
        }

        public static SceneBrowserViewModel SceneBrowserViewModel
        {
            get
            {
                return new SceneBrowserViewModel( () =>
                {
                    var sceneObjects = new List<SceneObjectInfo>();
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));

                    return sceneObjects;
                });
            }
        }
    }
}
