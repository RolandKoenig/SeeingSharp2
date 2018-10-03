using System;
using System.Collections.Generic;
using System.Text;
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

        public virtual void NotifyClosed()
        {

        }

        /// <summary>
        /// Builds a floor to the given scene.
        /// </summary>
        protected void BuildStandardFloor(SceneManipulator manipulator, string sceneLayer)
        {
            SceneLayer bgLayer = manipulator.AddLayer("BACKGROUND");
            manipulator.SetLayerOrderID(bgLayer, 0);
            manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 1);
            ResourceLink sourceBackgroundTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Background.dds");
            ResourceLink sourceTileTexture = new AssemblyResourceLink(
                typeof(SampleBase),
                "Assets.Floor.dds");

            var resBackgroundTexture = manipulator.AddTexture(sourceBackgroundTexture);
            manipulator.Add(new FullscreenTextureObject(resBackgroundTexture), bgLayer.Name);

            // Define textures and materials
            var resTileTexture = manipulator.AddResource(() => new StandardTextureResource(sourceTileTexture));
            var resTileMaterial = manipulator.AddResource(() => new SimpleColoredMaterialResource(resTileTexture));

            // Define floor geometry
            FloorType floorType = new FloorType(new Vector2(4f, 4f), 0f);
            floorType.BottomMaterial = resTileMaterial;
            floorType.DefaultFloorMaterial = resTileMaterial;
            floorType.SideMaterial = resTileMaterial;
            floorType.SetTilemap(25, 25);

            // Add floor to scene
            var resFloorGeometry = manipulator.AddResource((() => new GeometryResource(floorType)));
            var floorObject = manipulator.AddGeneric(resFloorGeometry, sceneLayer);
        }
    }
}
