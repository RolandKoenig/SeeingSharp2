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

using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer
{
    public abstract class SampleBase
    {
        public abstract Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings);

        public virtual void Update()
        {

        }

        public virtual void NotifyClosed()
        {

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

            var resBackgroundTexture = manipulator.AddTexture(sourceBackgroundTexture);
            manipulator.Add(new FullscreenTexture(resBackgroundTexture), bgLayer.Name);

            // Define textures and materials
            var resTileTexture = manipulator.AddResource(() => new StandardTextureResource(sourceTileTexture));
            var resTileMaterial = manipulator.AddResource(() => new SimpleColoredMaterialResource(resTileTexture));

            // Define floor geometry
            var floorType = new FloorGeometryFactory(new Vector2(4f, 4f), 0f)
            {
                BottomMaterial = resTileMaterial,
                DefaultFloorMaterial = resTileMaterial,
                SideMaterial = resTileMaterial
            };

            floorType.SetTilemap(25, 25);

            // Add floor to scene
            var resFloorGeometry = manipulator.AddResource(() => new GeometryResource(floorType));
            var floorObject = manipulator.AddMesh(resFloorGeometry, sceneLayer, resTileMaterial);
            floorObject.Tag1 = "Floor";
        }
    }
}