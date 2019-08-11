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

using System;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.SampleContainer.Basics3D._10_MultiMaterial
{
    [SampleDescription(
        "Multi Material", 10, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Basics3D/_10_MultiMaterial",
        typeof(SampleSettingsWith3D))]
    public class MultiMaterialSample : SampleBase
    {
        /// <summary>
        /// Called when the sample has to startup.
        /// </summary>
        public override async Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings settings)
        {
            targetRenderLoop.EnsureNotNull(nameof(targetRenderLoop));

            // Build dummy scene
            var scene = targetRenderLoop.Scene;
            var camera = targetRenderLoop.Camera;

            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                // Define texture and material resource
                var resTexture = manipulator.AddTexture(
                    new AssemblyResourceLink(this.GetType(),
                        "SimpleTexture.png"));
                var resMaterials = new NamedOrGenericKey[]
                {
                    manipulator.AddSimpleColoredMaterial(materialDiffuseColor: Color4.White, useVertexColors:false),
                    manipulator.AddSimpleColoredMaterial(resTexture),
                    manipulator.AddSimpleColoredMaterial(materialDiffuseColor:Color4Ex.CornflowerBlue, useVertexColors:false)
                };

                // Create cube geometry resource
                var resGeometry = manipulator.AddGeometry(
                    new CustomGeometryFactory(BuildCustomGeometry));

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resMaterials);
                cubeMesh.Color = Color4Ex.GreenColor;
                cubeMesh.Position = new Vector3(0f, 0.5f, 0f);
                cubeMesh.EnableShaderGeneratedBorder();
                cubeMesh.BuildAnimationSequence()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                    .WaitFinished()
                    .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                    .ApplyAndRewind();
                manipulator.Add(cubeMesh);
            });

            // Configure camera
            camera.Position = new Vector3(3f, 3f, 3f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            targetRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());
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
                Color4Ex.White);
            surface1.BuildCubeSides16V(
                new Vector3(-1f, 0f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4Ex.White);

            var surface2 = result.CreateSurface();
            surface2.BuildCubeSides16V(
                new Vector3(-1f, 1f, -1f), 
                new Vector3(2f, 1f, 2f),
                Color4Ex.White);

            var surface3 = result.CreateSurface();
            surface3.BuildCubeSides16V(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4Ex.White);
            surface3.BuildCubeTop4V(
                new Vector3(-1f, 2f, -1f),
                new Vector3(2f, 1f, 2f),
                Color4Ex.White);

            return result;
        }
    }
}