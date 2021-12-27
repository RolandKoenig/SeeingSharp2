using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D.MultiMaterial
{
    [SampleDescription(
        "Multi Material", 8, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Basics3D/MultiMaterial",
        typeof(SampleSettingsWith3D))]
    public class MultiMaterialSample : SampleBase
    {
        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define texture and material resource
                var resTexture = manipulator.AddTextureResource(
                    new AssemblyResourceLink(this.GetType(),
                        "SimpleTexture.png"));
                var resMaterials = new[]
                {
                    manipulator.AddStandardMaterialResource(
                        materialDiffuseColor: Color4.White, 
                        useVertexColors:false, 
                        enableShaderGeneratedBorder: true),
                    manipulator.AddStandardMaterialResource(resTexture),
                    manipulator.AddStandardMaterialResource(
                        materialDiffuseColor:Color4.CornflowerBlue, 
                        useVertexColors:false, 
                        enableShaderGeneratedBorder: true)
                };

                // Create cube geometry resource
                var resGeometry = manipulator.AddGeometryResource(
                    new CustomGeometryFactory(BuildCustomGeometry));

                // Create cube meshes
                var meshes = new List<Mesh>(3);
                meshes.Add(new Mesh(resGeometry, resMaterials[1])
                {
                    Position = new Vector3(-3f, 0.5f, 0f)
                });
                meshes.Add(new Mesh(resGeometry, resMaterials)
                {
                    Position = new Vector3(0, 0.5f, 0f)
                });
                meshes.Add(new Mesh(resGeometry, resMaterials[0], resMaterials[1])
                {
                    Position = new Vector3(3f, 0.5f, 0f)
                });
                foreach (var actCubeMesh in meshes)
                {
                    var actCubeMeshInner = actCubeMesh;
                    actCubeMeshInner.BuildAnimationSequence()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .CallAction(() => actCubeMeshInner.RotationEuler = Vector3.Zero)
                        .ApplyAndRewind();
                    manipulator.AddObject(actCubeMeshInner);
                }

            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(-7f, 10f, -10f);
            camera.Target = new Vector3(0f, 1.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Builds a custom geometry for this sample.
        /// </summary>
        private static Geometry BuildCustomGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var surface1 = result.CreateSurface();
            surface1.BuildCubeBottom(
                new Vector3(-1f, 0f, -1f),
                new Vector3(2f, 1f, 2f))
                .SetVertexColor(Color4.White);
            surface1.BuildCubeSides(
                new Vector3(-1f, 0f, -1f),
                new Vector3(2f, 1f, 2f))
                .SetVertexColor(Color4.White);

            var surface2 = result.CreateSurface();
            surface2.BuildCubeSides(
                new Vector3(-1f, 1f, -1f),
                new Vector3(2f, 1f, 2f))
                .SetVertexColor(Color4.White);

            var surface3 = result.CreateSurface();
            surface3.BuildCubeSides(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f))
                .SetVertexColor(Color4.White);
            surface3.BuildCubeTop(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f))
                .SetVertexColor(Color4.White);

            return result;
        }
    }
}