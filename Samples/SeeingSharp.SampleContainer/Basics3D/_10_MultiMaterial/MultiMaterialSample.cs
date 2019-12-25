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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace SeeingSharp.SampleContainer.Basics3D._10_MultiMaterial
{
    [SampleDescription(
        "Multi Material", 10, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_10_MultiMaterial",
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
                var resMaterials = new NamedOrGenericKey[]
                {
                    manipulator.AddStandardMaterialResource(materialDiffuseColor: Color4.White, useVertexColors:false),
                    manipulator.AddStandardMaterialResource(resTexture),
                    manipulator.AddStandardMaterialResource(materialDiffuseColor:Color4.CornflowerBlue, useVertexColors:false)
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
                    actCubeMeshInner.EnableShaderGeneratedBorder();
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

            // Configure camera
            camera.Position = new Vector3(-7f, 10f, -10f);
            camera.Target = new Vector3(0f, 1.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Builds a custom geometry for this sample.
        /// </summary>
        private static Geometry BuildCustomGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var surface1 = result.CreateSurface();
            surface1.BuildCubeBottom4V(
                new Vector3(-1f, 0f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4.White);
            surface1.BuildCubeSides16V(
                new Vector3(-1f, 0f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4.White);

            var surface2 = result.CreateSurface();
            surface2.BuildCubeSides16V(
                new Vector3(-1f, 1f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4.White);

            var surface3 = result.CreateSurface();
            surface3.BuildCubeSides16V(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4.White);
            surface3.BuildCubeTop4V(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4.White);

            return result;
        }
    }
}