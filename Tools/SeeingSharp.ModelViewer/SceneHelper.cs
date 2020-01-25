using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public static class SceneHelper
    {
        public static async Task ResetScene(RenderLoop targetRenderLoop)
        {
            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear the scene first
                manipulator.Clear();

                // Build new background layer with fullscreen texture
                var bgLayer = manipulator.AddLayer("BACKGROUND");
                manipulator.SetLayerOrderID(bgLayer, 0);
                manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 1);

                var sourceBackgroundTexture = new AssemblyResourceLink(
                    typeof(SceneHelper),
                    "Assets.Background.dds");
                var resBackgroundTexture = manipulator.AddTextureResource(sourceBackgroundTexture);
                manipulator.AddObject(new FullscreenTexture(resBackgroundTexture), bgLayer.Name);

                // Add bottom grid
                var resGridGeometry = manipulator.AddResource(
                    (device) => new GeometryResource(new Grid3DGeometryFactory()
                    {
                        TileWidth = 0.05f,
                        TilesX = 50,
                        TilesZ = 50,
                    }));
                var gridMesh = manipulator.AddMeshObject(resGridGeometry);
                gridMesh.YPos = -0.5f;

                // Add bounding box
                manipulator.AddObject(new WireObject(
                    Color4.GreenColor,
                    new BoundingBox(
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, 0.5f, 0.5f))));
            });
        }
    }
}
