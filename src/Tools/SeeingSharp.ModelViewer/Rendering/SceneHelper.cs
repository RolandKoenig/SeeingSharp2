using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer.Rendering
{
    public static class SceneHelper
    {
        public static async Task ResetScene(RenderLoop targetRenderLoop)
        {
            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear the scene first
                manipulator.Clear();

                // Define layers
                var bgLayer = manipulator.AddLayer("BACKGROUND");
                var gridLayer = manipulator.AddLayer("GRID");
                manipulator.SetLayerOrderId(bgLayer, 0);
                manipulator.SetLayerOrderId(gridLayer, 1);
                manipulator.SetLayerOrderId(Scene.DEFAULT_LAYER_NAME, 2);

                // Build new background layer with fullscreen texture (layer BACKGROUND)
                var sourceBackgroundTexture = new AssemblyResourceLink(
                    typeof(App),
                    "Assets.Background.dds");
                var resBackgroundTexture = manipulator.AddTextureResource(sourceBackgroundTexture);
                var objBackgroundTexture = new FullscreenTexture(resBackgroundTexture);
                objBackgroundTexture.Tag1 = new ModelViewerSceneObjectMetadata(true);
                manipulator.AddObject(objBackgroundTexture, bgLayer.Name);

                // Add bottom grid (layer GRID)
                var resGridGeometry = manipulator.AddResource(
                    _ => new GeometryResource(new Grid3DGeometryFactory
                    {
                        TileWidth = 0.05f,
                        TilesX = 50,
                        TilesZ = 50
                    }));
                var gridMesh = manipulator.AddMeshObject(resGridGeometry, gridLayer.Name);
                gridMesh.YPos = -0.5f;
                gridMesh.Name = Constants.OBJ_NAME_GRID;
                gridMesh.Tag1 = new ModelViewerSceneObjectMetadata(true);

                // Add bounding box (layer GRID)
                var unitCube = new WireObject(
                    Color4.GreenColor,
                    new BoundingBox(
                        new Vector3(-0.5f, -0.5f, -0.5f),
                        new Vector3(0.5f, 0.5f, 0.5f)));
                unitCube.Name = Constants.OBJ_NAME_UNIT_CUBE;
                unitCube.Tag1 = new ModelViewerSceneObjectMetadata(true);
                manipulator.AddObject(unitCube, gridLayer.Name);
            });
        }

        public static async Task AddModelToScene(RenderLoop targetRenderLoop, ImportedModelContainer importedModel)
        {
            foreach (var actImportedObject in importedModel.Objects)
            {
                actImportedObject.Tag1 = new ModelViewerSceneObjectMetadata(false);
            }

            await targetRenderLoop.Scene.ImportAsync(importedModel);
        }
    }
}
