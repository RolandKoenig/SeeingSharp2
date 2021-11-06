using SeeingSharp.Core;
using SeeingSharp.Core.Animations;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public class FullscreenTexture : SceneObject, IAnimatableObjectAccentuation, IAnimatableObjectOpacity
    {
        // Configuration
        private NamedOrGenericKey _resTexture;

        // Device dependent resources
        private IndexBasedDynamicCollection<TexturePainterHelper> _texturePainterHelpers;

        public float AccentuationFactor { get; set; }

        public float Opacity { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor.
        /// </summary>
        public float Scaling { get; set; }

        /// <summary>
        /// Gets or sets the alpha blend mode.
        /// </summary>
        public TexturePainterAlphaBlendMode AlphaBlendMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullscreenTexture"/> class.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public FullscreenTexture(NamedOrGenericKey texture)
        {
            _resTexture = texture;
            this.Scaling = 1f;
            this.Opacity = 1f;
            this.AccentuationFactor = 0f;
            this.AlphaBlendMode = TexturePainterAlphaBlendMode.AlphaBlend;

            _texturePainterHelpers = new IndexBasedDynamicCollection<TexturePainterHelper>();
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        public override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            var newHelper = new TexturePainterHelper(_resTexture);

            _texturePainterHelpers.AddObject(
                newHelper,
                device.DeviceIndex);

            newHelper.LoadResources(resourceDictionary);
        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        /// <param name="device"></param>
        public override bool IsLoaded(EngineDevice device)
        {
            return _texturePainterHelpers.HasObjectAt(device.DeviceIndex);
        }

        /// <summary>
        /// Unloads all resources of the object.
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            foreach (var actHelper in _texturePainterHelpers)
            {
                actHelper.UnloadResources();
            }

            _texturePainterHelpers.Clear();
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        protected override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            //Subscribe to render passes
            if (this.CountRenderPassSubscriptions(layerViewSubset) == 0)
            {
                this.SubscribeToPass(
                    RenderPassInfo.PASS_PLAIN_RENDER,
                    layerViewSubset, this.OnRenderPlain);
                this.SubscribeToPass(
                    RenderPassInfo.PASS_TRANSPARENT_RENDER,
                    layerViewSubset, this.OnRenderTransparent);
            }
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRenderPlain(RenderState renderState)
        {
            if (this.Opacity < 1f) { return; }

            // Get and configure helper object
            var actHelper = _texturePainterHelpers[renderState.DeviceIndex];
            actHelper.Scaling = this.Scaling;
            actHelper.Opacity = this.Opacity;
            actHelper.AccentuationFactor = this.AccentuationFactor;
            actHelper.AlphaBlendMode = this.AlphaBlendMode;

            // Render the object
            actHelper.RenderPlain(renderState);
        }

        /// <summary>
        /// Renders the object.
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        private void OnRenderTransparent(RenderState renderState)
        {
            if (this.Opacity < 1f)
            {
                // Get and configure helper object
                var actHelper = _texturePainterHelpers[renderState.DeviceIndex];
                actHelper.Scaling = this.Scaling;
                actHelper.Opacity = this.Opacity;
                actHelper.AccentuationFactor = this.AccentuationFactor;
                actHelper.AlphaBlendMode = this.AlphaBlendMode;

                // Render the object
                actHelper.RenderPlain(renderState);
            }
        }
    }
}