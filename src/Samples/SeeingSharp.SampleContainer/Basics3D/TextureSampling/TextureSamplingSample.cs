using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer.Basics3D.TextureSampling
{
    [SampleDescription(
        "TextureSampling", 11, nameof(Basics3D),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/src/Samples/SeeingSharp.SampleContainer/Basics3D/TextureSampling",
        typeof(TextureSamplingSettings))]
    public class TextureSamplingSample : SampleBase
    {
        private TextureSamplingSettings? _castedSettings;
        private Scene? _scene;

        /// <inheritdoc />
        public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            _scene = mainRenderLoop.Scene;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            _castedSettings = (TextureSamplingSettings)settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create texture and material
                var resTexture = manipulator.AddResource(_ =>
                {
                    var textureFileLink = new AssemblyResourceLink(
                        typeof(TextureSamplingSample),
                        "PrimitiveTexture.png");
                    var textureResource = new StandardTextureResource(textureFileLink);

                    textureResource.Filter = _castedSettings!.Filter;
                    textureResource.ComparisonFunction = _castedSettings.ComparisonFunction;
                    textureResource.AddressU = _castedSettings.AddressU;
                    textureResource.AddressV = _castedSettings.AddressV;
                    textureResource.MaxAnisotropy = _castedSettings.MaxAnisotropy;
                    textureResource.MipLODBias = _castedSettings.MipLODBias;
                    textureResource.MinLOD = _castedSettings.MinLOD;
                    textureResource.MaxLOD = _castedSettings.MaxLOD;

                    return textureResource;
                });
                var resMaterial = manipulator.AddStandardMaterialResource(resTexture);

                // Create geometry 
                var resGeometry = manipulator.AddGeometryResource(new CustomGeometryFactory(_ =>
                {
                    var result = new Geometry();

                    var surface = result.CreateSurface();
                    surface.EnableTextureTileMode(new Vector2(0.25f, 0.25f));
                    surface.BuildCube(1f, 1f, 1f);

                    return result;
                }));

                // Create cube object
                var cubeMesh = new Mesh(resGeometry, resMaterial);
                cubeMesh.Position = new Vector3(0f, 0.5f, 0f);
                if (_castedSettings!.IsAnimated)
                {
                    cubeMesh.BuildAnimationSequence()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_180DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .RotateEulerAnglesTo(new Vector3(0f, EngineMath.RAD_360DEG, 0f), TimeSpan.FromSeconds(2.0))
                        .WaitFinished()
                        .CallAction(() => cubeMesh.RotationEuler = Vector3.Zero)
                        .ApplyAndRewind();
                }
                manipulator.AddObject(cubeMesh);
            });
        }

        /// <inheritdoc />
        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(-2f, 2f, -2f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.CompletedTask;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class TextureSamplingSettings : SampleSettingsWith3D
        {
            private SeeingSharpFilter _filter = SeeingSharpFilter.Anisotropic;
            private SeeingSharpComparisonFunction _comparisonFunction = SeeingSharpComparisonFunction.Never;
            private SeeingSharpTextureAddressMode _addressU = SeeingSharpTextureAddressMode.Wrap;
            private SeeingSharpTextureAddressMode _addressV = SeeingSharpTextureAddressMode.Wrap;
            private bool _isAnimated = true;
            private int _maxAnisotropy = 8;
            private int _mipLODBias = 0;
            private float _maxLOD = float.MaxValue;
            private float _minLOD = float.MinValue;

            [Category("Misc")]
            public bool IsAnimated
            {
                get => _isAnimated;
                set => base.SetFieldRaisingRecreateRequest(ref _isAnimated, value);
            }

            [Category("Texture sampling")]
            public SeeingSharpFilter Filter
            {
                get => _filter;
                set => base.SetFieldRaisingRecreateRequest(ref _filter, value);
            }

            [Category("Texture sampling")]
            public SeeingSharpComparisonFunction ComparisonFunction
            {
                get => _comparisonFunction;
                set => base.SetFieldRaisingRecreateRequest(ref _comparisonFunction, value);
            }

            [Category("Texture sampling")]
            public SeeingSharpTextureAddressMode AddressU
            {
                get => _addressU;
                set => base.SetFieldRaisingRecreateRequest(ref _addressU, value);
            }

            [Category("Texture sampling")]
            public SeeingSharpTextureAddressMode AddressV
            {
                get => _addressV;
                set => base.SetFieldRaisingRecreateRequest(ref _addressV, value);
            }

            [Category("Texture sampling")]
            public int MaxAnisotropy
            {
                get => _maxAnisotropy;
                set => base.SetFieldRaisingRecreateRequest(ref _maxAnisotropy, value);
            }


            [Category("Texture sampling")]
            public int MipLODBias
            {
                get => _mipLODBias;
                set => base.SetFieldRaisingRecreateRequest(ref _mipLODBias, value);
            }

            [Category("Texture sampling")]
            public float MinLOD
            {
                get => _minLOD;
                set => base.SetFieldRaisingRecreateRequest(ref _minLOD, value);
            }

            [Category("Texture sampling")]
            public float MaxLOD
            {
                get => _maxLOD;
                set => base.SetFieldRaisingRecreateRequest(ref _maxLOD, value);
            }
        }
    }
}
