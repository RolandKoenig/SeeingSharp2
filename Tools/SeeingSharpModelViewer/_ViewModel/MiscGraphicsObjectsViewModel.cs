﻿using GalaSoft.MvvmLight;
using SeeingSharp;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public class MiscGraphicsObjectsViewModel : ViewModelBase
    {
        // Scene
        private Scene m_scene;
        private SceneLayer m_bgLayer;

        // Configuration
        private int m_tilesPerSide;

        public MiscGraphicsObjectsViewModel(Scene scene)
        {
            scene.EnsureNotNull(nameof(scene));

            m_tilesPerSide = 16;

            m_scene = scene;
        }

        /// <summary>
        /// Initializes the scene for this model viewer.
        /// </summary>
        internal async Task InitializeAsync()
        {
            await m_scene.ManipulateSceneAsync((manipulator) =>
            {
                SceneLayer bgImageLayer = null;
                bool isBgImageCreated = false;
                if (manipulator.ContainsLayer(Constants.LAYER_BACKGROUND_FLAT))
                {
                    bgImageLayer = manipulator.GetLayer(Constants.LAYER_BACKGROUND_FLAT);
                    isBgImageCreated = true;
                }
                else
                {
                    bgImageLayer = manipulator.AddLayer(Constants.LAYER_BACKGROUND_FLAT);
                }

                SceneLayer bgLayer = manipulator.AddLayer(Constants.LAYER_BACKGROUND);
                manipulator.SetLayerOrderID(bgImageLayer, 0);
                manipulator.SetLayerOrderID(bgLayer, 1);
                manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 2);

                var keyPostprocess = manipulator.AddResource<FocusPostprocessEffectResource>(
                    () => new FocusPostprocessEffectResource(false));
                if (!manipulator.ContainsLayer(Constants.LAYER_HOVER))
                {
                    SceneLayer layerHover = manipulator.AddLayer(Constants.LAYER_HOVER);
                    layerHover.PostprocessEffectKey = keyPostprocess;
                    layerHover.ClearDepthBufferBeforeRendering = true;
                    manipulator.SetLayerOrderID(layerHover, 3);
                }

                // Store a reference to the background layer
                m_bgLayer = bgLayer;

                // Define background texture
                if (!isBgImageCreated)
                {
                    ResourceLink linkBackgroundTexture = new AssemblyResourceLink(
                        typeof(App),
                        "Assets.Textures.Background.dds");
                    NamedOrGenericKey resBackgroundTexture = manipulator.AddTexture(linkBackgroundTexture);
                    manipulator.Add(new FullscreenTexture(resBackgroundTexture), bgImageLayer.Name);
                }

                // Define ground
                var objTypeGrid = new Grid3DGeometryFactory();
                objTypeGrid.TilesX = m_tilesPerSide * 4;
                objTypeGrid.TilesZ = m_tilesPerSide * 4;
                objTypeGrid.HighlightXZLines = true;
                objTypeGrid.TileWidth = 0.25f;
                objTypeGrid.GroupTileCount = 4;
                objTypeGrid.GenerateGround = false;
                objTypeGrid.XLineHighlightColor = Color4Ex.GreenColor;
                objTypeGrid.ZLineHighlightColor = Color4Ex.BlueColor;

                NamedOrGenericKey resGridGeometry = manipulator.AddGeometry(objTypeGrid);
                manipulator.Add(new GenericObject(resGridGeometry), Constants.LAYER_BACKGROUND);

                var resSimpleMaterial = manipulator.AddSimpleColoredMaterial();

                TextGeometryOptions textXOptions = TextGeometryOptions.Default;
                textXOptions.SurfaceVertexColor = Color4Ex.GreenColor;
                textXOptions.MakeVolumetricText = false;
                textXOptions.FontSize = 30;
                var resTextXGeometry = manipulator.Add3DTextGeometry(
                    "X", textXOptions,
                    realignToCenter: true);
                var textXMesh = new Mesh(resTextXGeometry, resSimpleMaterial);
                textXMesh.Position = new SharpDX.Vector3((m_tilesPerSide / 2f) + 1f, 0, 0);
                manipulator.Add(textXMesh, Constants.LAYER_BACKGROUND);

                TextGeometryOptions textZOptions = TextGeometryOptions.Default;
                textZOptions.SurfaceVertexColor = Color4Ex.BlueColor;
                textZOptions.MakeVolumetricText = false;
                textZOptions.FontSize = 30;
                var resTextZGeometry = manipulator.Add3DTextGeometry(
                    "Z", textZOptions,
                    realignToCenter: true);
                var textZMesh = new Mesh(resTextZGeometry, resSimpleMaterial);
                textZMesh.Position = new SharpDX.Vector3(0f, 0f, (m_tilesPerSide / 2f) + 1f);
                manipulator.Add(textZMesh, Constants.LAYER_BACKGROUND);
            });
        }

        /// <summary>
        /// Reloads the background asynchronous.
        /// </summary>
        /// <returns></returns>
        internal async Task ReloadBackgroundAsync()
        {
            if (m_bgLayer != null)
            {
                await m_scene.ManipulateSceneAsync((manipulator) =>
                {
                    manipulator.RemoveLayer(m_bgLayer);
                });
            }

            await InitializeAsync();
        }

        public int TilesPerSide
        {
            get { return m_tilesPerSide; }
            set
            {
                if (m_tilesPerSide != value)
                {
                    m_tilesPerSide = value;
                    if (m_tilesPerSide < Constants.COUNT_TILES_MIN) { m_tilesPerSide = Constants.COUNT_TILES_MIN; }
                    if (m_tilesPerSide > Constants.COUNT_TILES_MAX) { m_tilesPerSide = Constants.COUNT_TILES_MAX; }

                    ReloadBackgroundAsync()
                        .FireAndForget();

                    RaisePropertyChanged(nameof(TilesPerSide));
                }
            }
        }

        public bool BackgroundVisible
        {
            get
            {
                if (m_bgLayer == null) { return false; }
                else { return m_bgLayer.IsRenderingEnabled; }
            }
            set
            {
                if (m_bgLayer == null) { return; }

                if (m_bgLayer.IsRenderingEnabled != value)
                {
                    m_bgLayer.IsRenderingEnabled = value;
                    RaisePropertyChanged(nameof(BackgroundVisible));
                }
            }
        }
    }
}