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
                var aBottom = new Vector3(-0.5f, -0.5f, -0.5f);
                var bBottom = new Vector3(0.5f, -0.5f, -0.5f);
                var cBottom = new Vector3(0.5f, -0.5f, 0.5f);
                var dBottom = new Vector3(-0.5f, -0.5f, 0.5f);
                var aTop = new Vector3(-0.5f, 0.5f, -0.5f);
                var bTop = new Vector3(0.5f, 0.5f, -0.5f);
                var cTop = new Vector3(0.5f, 0.5f, 0.5f);
                var dTop = new Vector3(-0.5f, 0.5f, 0.5f);
                manipulator.AddObject(new WireObject(
                    Color4.GreenColor,
                    new Line(aBottom, bBottom),
                    new Line(bBottom, cBottom),
                    new Line(cBottom, dBottom),
                    new Line(dBottom, aBottom),

                    new Line(aTop, bTop),
                    new Line(bTop, cTop),
                    new Line(cTop, dTop),
                    new Line(dTop, aTop),
                    
                    new Line(aBottom, aTop),
                    new Line(bBottom, bTop),
                    new Line(cBottom, cTop),
                    new Line(dBottom, dTop)));
            });
        }
    }
}
