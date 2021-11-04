using System.ComponentModel;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserObjectHardwareDependentDetails : PropertyChangedBase
    {
        private const string CATEGORY_PROPERTIES = "Properties (hardware)";

        private SceneObjectInfo _sceneObjectInfo;
        private EngineDevice _device;
        private GeometryResource? _resGeometry;

        public SceneBrowserObjectHardwareDependentDetails(SceneObjectInfo sceneObjectInfo, EngineDevice device)
        {
            _sceneObjectInfo = sceneObjectInfo;
            _device = device;

            this.RefreshData();
        }

        public void RefreshData()
        {
            GeometryResource? newResGeometry = null;

            if(_sceneObjectInfo.OriginalObject is Mesh originalMesh)
            {
                newResGeometry = originalMesh.TryGetGeometryResource(_device);
            }

            if (newResGeometry != _resGeometry)
            {
                _resGeometry = newResGeometry;
            }

            this.RaisePropertyChanged(string.Empty);
        }

        [Category(CATEGORY_PROPERTIES)]
        public string GraphicsDevice => _device.AdapterDescription;

        [Category(CATEGORY_PROPERTIES)]
        public bool IsLoaded => _resGeometry != null;

        [Category(CATEGORY_PROPERTIES)]
        public int TriangleCount => _resGeometry?.LoadedGeometryTriangleCount ?? 0;

        [Category(CATEGORY_PROPERTIES)]
        public int RenderingChunkCount => _resGeometry?.LoadedGeometryRenderingChunkCount ?? 0;
    }
}
