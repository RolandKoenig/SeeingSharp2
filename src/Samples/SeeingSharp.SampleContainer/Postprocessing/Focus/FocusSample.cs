using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.SampleContainer.Postprocessing.Focus
{
    [SampleDescription(
        "Focus", 2, nameof(Postprocessing),
        "PreviewImage.png",
        "https://github.com/RolandKoenig/SeeingSharp2/tree/master/Samples/SeeingSharp.SampleContainer/Postprocessing/Focus",
        typeof(FocusSampleSettings))]
    public class FocusSample : SampleBase
    {
        private FocusSampleSettings _sampleSettings;
        private Mesh _focusMesh;

        public override async Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings settings)
        {
            mainRenderLoop.EnsureNotNull(nameof(mainRenderLoop));

            _sampleSettings = (FocusSampleSettings) settings;

            await mainRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Create floor
                this.BuildStandardFloor(
                    manipulator, Scene.DEFAULT_LAYER_NAME);

                    var defaultLayer = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);

                    // Create the focus effect and attach it to a new 'Focus' layer
                    var keyPostprocess = manipulator.AddResource(
                        device => new FocusPostprocessEffectResource());
                    var focusLayer = manipulator.AddLayer("Focus");
                    focusLayer.PostprocessEffectKey = keyPostprocess;

                    var resGeometry = manipulator.AddResource(
                        device => new GeometryResource(new CubeGeometryFactory()));
                    var resMaterial = manipulator.AddStandardMaterialResource();

                    var frontMesh = manipulator.AddMeshObject(resGeometry, defaultLayer.Name, resMaterial);
                    frontMesh.Color = Color4.BlueColor;
                    frontMesh.Scaling = new Vector3(1f, 0.5f, 0.5f);
                    frontMesh.Position = new Vector3(1f, 2f, 1f);

                    var backMesh = manipulator.AddMeshObject(resGeometry, defaultLayer.Name, resMaterial);
                    backMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 3f, 0f);
                    backMesh.Scaling = new Vector3(2f, 2f, 2f);
                    backMesh.Color = Color4.RedColor;

                    // This mesh will render the focus effect itself (it is placed on the focus layer)
                    _focusMesh = manipulator.AddMeshObject(resGeometry, focusLayer.Name, resMaterial);
                    _focusMesh.TransformSourceObject = backMesh;
                    _focusMesh.TransformationType = SpacialTransformationType.TakeFromOtherObject;
                    _focusMesh.Color = Color4.RedColor;
            });
        }

        public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
        {
            var camera = mainOrChildRenderLoop.Camera;

            // ConfigureLoading camera
            camera.Position = new Vector3(4f, 4f, 4f);
            camera.Target = new Vector3(0f, 0.5f, 0f);
            camera.UpdateCamera();

            // Append camera behavior
            mainOrChildRenderLoop.SceneComponents.Add(new FreeMovingCameraComponent());

            // Add object filter for viewbox culling
            mainOrChildRenderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());

            return Task.FromResult<object>(null);
        }

        public override void Update()
        {
            base.Update();

            // Show/Hide the focus mesh depending on the user's configuration
            if (_sampleSettings.ShowFocusEffect)
            {
                _focusMesh.VisibilityTestMethod = VisibilityTestMethod.ByObjectFilters;
            }
            else
            {
                _focusMesh.VisibilityTestMethod = VisibilityTestMethod.ForceHidden;
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class FocusSampleSettings : SampleSettingsWith3D
        {
            [Category("Focus")]
            public bool ShowFocusEffect { get; set; } = true;
        }
    }
}
