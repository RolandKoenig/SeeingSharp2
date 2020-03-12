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
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer.Util
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
                manipulator.SetLayerOrderId(bgLayer, 0);
                manipulator.SetLayerOrderId(Scene.DEFAULT_LAYER_NAME, 1);

                var sourceBackgroundTexture = new AssemblyResourceLink(
                    typeof(App),
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
