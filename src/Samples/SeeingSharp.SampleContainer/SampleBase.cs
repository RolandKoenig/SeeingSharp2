using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Core;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer
{
    public abstract class SampleBase
    {
        public abstract Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings);

        public abstract Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop);

        public virtual Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            return Task.CompletedTask;
        }

        public virtual void Update()
        {

        }

        public virtual void OnSampleClosed()
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
            manipulator.SetLayerOrderId(bgLayer, 0);
            manipulator.SetLayerOrderId(Scene.DEFAULT_LAYER_NAME, 1);
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
            manipulator.AddMeshObject(resFloorGeometry, sceneLayer, resTileMaterial);
        }
    }
}