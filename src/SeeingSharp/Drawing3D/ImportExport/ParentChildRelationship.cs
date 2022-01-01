using SeeingSharp.Core;

namespace SeeingSharp.Drawing3D.ImportExport
{
    public struct ParentChildRelationship
    {
        public readonly SceneObject Parent;
        public readonly SceneObject Child;

        public ParentChildRelationship(SceneObject parent, SceneObject child)
        {
            Parent = parent;
            Child = child;
        }
    }
}
