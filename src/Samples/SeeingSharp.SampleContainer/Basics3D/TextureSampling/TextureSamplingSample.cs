using System;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Components.Input;
using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Drawing3D;
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
            _castedSettings = (TextureSamplingSettings)settings;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override async Task OnReloadAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            await base.OnReloadAsync(mainRenderLoop, settings);

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                manipulator.Clear();

                // Create floor
                this.BuildStandardFloor(manipulator, Scene.DEFAULT_LAYER_NAME);

                // Create texture
                var resTexture = manipulator.AddResource(_ =>
                {
                    var textureFileLink = new AssemblyResourceLink(
                        typeof(TextureSamplingSample),
                        "PrimitiveTexture.png");
                    var textureResource = new StandardTextureResource(textureFileLink);

                    textureResource.Filter = _castedSettings!.Filter;
                    textureResource.ComparisonFunction = _castedSettings.ComparisonFunction;
                    textureResource.MaxAnisotropy = _castedSettings.MaxAnisotropy;
                    textureResource.MipLODBias = _castedSettings.MipLODBias;
                    textureResource.MinLOD = _castedSettings.MinLOD;
                    textureResource.MaxLOD = _castedSettings.MaxLOD;

                    return textureResource;
                });

                // Create resources
                var resGeometry = manipulator.AddGeometryResource(new CubeGeometryFactory());
                var resMaterial = manipulator.AddStandardMaterialResource(resTexture);

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
            private bool _isAnimated = true;
            private int _maxAnisotropy = 8;
            private int _mipLODBias = 0;
            private float _maxLOD = float.MaxValue;
            private float _minLOD = float.MinValue;

            [Category("Misc")]
            public bool IsAnimated
            {
                get => _isAnimated;
                set
                {
                    _isAnimated = value;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Texture sampling")]
            public SeeingSharpFilter Filter
            {
                get => _filter;
                set
                {
                    _filter = value;
                    base.RaiseRecreateRequest();
                }
            }

            [Category("Texture sampling")]
            public SeeingSharpComparisonFunction ComparisonFunction
            {
                get => _comparisonFunction;
                set
                {
                    _comparisonFunction = value;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Texture sampling")]
            public int MaxAnisotropy
            {
                get => _maxAnisotropy;
                set
                {
                    _maxAnisotropy = value;
                    this.RaiseRecreateRequest();
                }
            }


            [Category("Texture sampling")]
            public int MipLODBias
            {
                get => _mipLODBias;
                set
                {
                    _mipLODBias = value;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Texture sampling")]
            public float MinLOD
            {
                get => _minLOD;
                set
                {
                    _minLOD = value;
                    this.RaiseRecreateRequest();
                }
            }

            [Category("Texture sampling")]
            public float MaxLOD
            {
                get => _maxLOD;
                set
                {
                    _maxLOD = value;
                    this.RaiseRecreateRequest();
                }
            }
        }
    }
}
