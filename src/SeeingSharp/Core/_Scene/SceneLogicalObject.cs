namespace SeeingSharp.Core
{
    public abstract class SceneLogicalObject : SceneObject
    {
        public sealed override void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary)
        {
            // No resources, so nothing to be done
        }

        public sealed override bool IsLoaded(EngineDevice device)
        {
            // No resources, so nothing to be done
            return true;
        }

        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
        }

        protected sealed override void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            // No resources, so nothing to be done
        }
    }
}
