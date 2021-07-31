/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
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
