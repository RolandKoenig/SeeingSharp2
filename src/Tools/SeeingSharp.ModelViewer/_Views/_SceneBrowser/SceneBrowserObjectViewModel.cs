using System;
using System.Collections.ObjectModel;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Core;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserObjectViewModel : PropertyChangedBase
    {
        private SceneObjectInfo _sceneObjectInfo;

        public SceneBrowserObjectHardwareIndependentDetails HardwareIndependentDetails => 
            new SceneBrowserObjectHardwareIndependentDetails(_sceneObjectInfo);

        public ObservableCollection<SceneBrowserObjectHardwareDependentDetails> HardwareDependentDetails { get; } =
            new ObservableCollection<SceneBrowserObjectHardwareDependentDetails>();

        public SceneBrowserObjectViewModel(SceneObjectInfo sceneObjectInfo)
        {
            _sceneObjectInfo = sceneObjectInfo;

            switch (_sceneObjectInfo.Type)
            {
                case SceneObjectInfoType.Mesh:
                    foreach(var actDevice in GraphicsCore.Current.Devices)
                    {
                        this.HardwareDependentDetails.Add(new SceneBrowserObjectHardwareDependentDetails(_sceneObjectInfo, actDevice));
                    }
                    break;

                case SceneObjectInfoType.Pivot:
                    break;

                case SceneObjectInfoType.FullscreenTexture:
                    break;

                case SceneObjectInfoType.Skybox:
                    break;

                case SceneObjectInfoType.WireObject:
                    break;

                case SceneObjectInfoType.Other:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
