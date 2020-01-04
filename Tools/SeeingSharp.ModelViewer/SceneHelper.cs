using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public static class SceneHelper
    {
        public static async Task ResetScene(RenderLoop targetRenderLoop)
        {
            await targetRenderLoop.Scene.ManipulateSceneAsync(manipulator =>
            {
                // Clear the scene first
                manipulator.Clear();

                // Build new background layer with fullscreen texture
                var bgLayer = manipulator.AddLayer("BACKGROUND");
                manipulator.SetLayerOrderID(bgLayer, 0);
                manipulator.SetLayerOrderID(Scene.DEFAULT_LAYER_NAME, 1);

                var sourceBackgroundTexture = new AssemblyResourceLink(
                    typeof(SceneHelper),
                    "Assets.Background.dds");
                var resBackgroundTexture = manipulator.AddTextureResource(sourceBackgroundTexture);
                manipulator.AddObject(new FullscreenTexture(resBackgroundTexture), bgLayer.Name);
            });
        }
    }
}
