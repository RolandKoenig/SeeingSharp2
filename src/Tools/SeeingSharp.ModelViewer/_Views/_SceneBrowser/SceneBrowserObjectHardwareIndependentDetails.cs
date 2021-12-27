using System.ComponentModel;
using SeeingSharp.Core;
using SeeingSharp.ModelViewer.Util;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserObjectHardwareIndependentDetails : PropertyChangedBase
    {
        private const string CATEGORY_PROPERTIES = "Properties (hardware independent)";

        private SceneObjectInfo _sceneObjectInfo;

        public SceneBrowserObjectHardwareIndependentDetails(SceneObjectInfo sceneObjectInfo)
        {
            _sceneObjectInfo = sceneObjectInfo;
        }

        public void RefreshData()
        {
            this.RaisePropertyChanged(string.Empty);
        }

        [Category(CATEGORY_PROPERTIES)]
        public string Type => _sceneObjectInfo.Type.ToString();

        [Category(CATEGORY_PROPERTIES)]
        public int CountChildren => _sceneObjectInfo.OriginalObject.CountChildren;

        [Category(CATEGORY_PROPERTIES)]
        public bool HasParent => _sceneObjectInfo.OriginalObject.HasParent;

        [Category(CATEGORY_PROPERTIES)]
        public bool HasChildren => _sceneObjectInfo.OriginalObject.HasChildren;

        [Category(CATEGORY_PROPERTIES)]
        public bool IsStatic => _sceneObjectInfo.OriginalObject.IsStatic;

        [Category(CATEGORY_PROPERTIES)]
        public string TargetDetailLevel => _sceneObjectInfo.OriginalObject.TargetDetailLevel.ToString();
    }
}
