namespace SeeingSharp.Core
{
    /// <summary>
    /// Helper structure which holds information about all currently
    /// attached components.
    /// </summary>
    internal struct SceneComponentInfo
    {
        public SceneComponentBase Component;
        public object Context;
        public ViewInformation? CorrespondingView;
    }

    /// <summary>
    /// Helper structure which holds information about a request
    /// for a component (attach, detach) which normally comes
    /// from the UI thread
    /// </summary>
    internal struct SceneComponentRequest
    {
        public SceneComponentRequestType RequestType;
        public SceneComponentBase Component;
        public ViewInformation? CorrespondingView;
    }

    /// <summary>
    /// The type of a component request.
    /// </summary>
    internal enum SceneComponentRequestType
    {
        Attach,
        Detach,
        DetachAll
    }
}
