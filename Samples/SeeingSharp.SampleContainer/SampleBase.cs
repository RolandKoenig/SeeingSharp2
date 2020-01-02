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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer
{
    public abstract class SampleBase
    {
        public abstract Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings);

        public abstract Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop);

        public virtual Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            return Task.FromResult<object>(null);
        }

        public virtual void Update()
        {

        }

        public virtual void OnClosed()
        {

        }

        /// <summary>
        /// Draws the default background for 2D rendering
        /// </summary>
        /// <param name="graphics"></param>
        protected void Draw2DBackground(Graphics2D graphics)
        {
            graphics.Clear(Color4.LightSteelBlue);
        }

        /// <summary>
        /// Builds a floor to the given scene.
        /// </summary>
        protected void BuildStandardFloor(SceneManipulator manipulator, string sceneLayer)
        {
            var bgLayer = manipulator.AddLayer("BACKGROUND");
            manipulator.SetLayerOrderID(bgLayer, 0);
            manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 1);
            ResourceLink sourceBackgroundTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Background.dds");
            ResourceLink sourceTileTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Floor.dds");

            var resBackgroundTexture = manipulator.AddTextureResource(sourceBackgroundTexture);
            manipulator.AddObject(new FullscreenTexture(resBackgroundTexture), bgLayer.Name);

            // Define textures and materials
            var resTileTexture = manipulator.AddResource(device => new StandardTextureResource(sourceTileTexture));
            var resTileMaterial = manipulator.AddResource(device => new StandardMaterialResource(resTileTexture));

            // Define floor geometry
            var floorType = new FloorGeometryFactory(new Vector2(4f, 4f));
            floorType.SetTilemap(25, 25);

            // AddObject floor to scene
            var resFloorGeometry = manipulator.AddResource(device => new GeometryResource(floorType));
            var floorObject = manipulator.AddMeshObject(resFloorGeometry, sceneLayer, resTileMaterial);
            floorObject.Tag1 = "Floor";
        }
    }
}