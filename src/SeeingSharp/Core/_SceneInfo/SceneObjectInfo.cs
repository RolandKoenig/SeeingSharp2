using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.Core
{
    public class SceneObjectInfo
    {
        public SceneObject OriginalObject { get; }

        public SceneObjectInfoType Type { get; }

        /// <summary>
        /// Gets or sets the name of this node.
        /// The name ist just meta information and has no relevance for SeeingSharp.
        /// </summary>
        public string Name => this.OriginalObject.Name;

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag1 => this.OriginalObject.Tag1;

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag2 => this.OriginalObject.Tag2;

        /// <summary>
        /// Queries for all children of this object.
        /// </summary>
        public IEnumerable<SceneObjectInfo> Children
        {
            get
            {
                foreach (var actChildObject in this.OriginalObject.GetAllChildren(false))
                {
                    yield return new SceneObjectInfo(actChildObject);
                }
            }
        }

        public bool IsAssociatedToScene => this.OriginalObject.Scene != null;

        public Scene AssociatedScene => this.OriginalObject.Scene;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObjectInfo"/> class.
        /// </summary>
        /// <param name="obj">The object for which to build this info object.</param>
        public SceneObjectInfo(SceneObject obj)
        {
            obj.EnsureNotNull(nameof(obj));

            this.OriginalObject = obj;

            // Set the type of this object
            this.Type = SceneObjectInfoType.Other;
            var clrType = obj.GetType();

            if (clrType == typeof(Mesh))
            {
                this.Type = SceneObjectInfoType.Mesh;
            }
            else if (clrType == typeof(ScenePivotObject))
            {
                this.Type = SceneObjectInfoType.Pivot;
            }
            else if (clrType == typeof(FullscreenTexture))
            {
                this.Type = SceneObjectInfoType.FullscreenTexture;
            }
            else if (clrType == typeof(Skybox))
            {
                this.Type = SceneObjectInfoType.FullscreenTexture;
            }
            else if(clrType == typeof(WireObject))
            {
                this.Type = SceneObjectInfoType.WireObject;
            }
        }

        public override string ToString()
        {
            return $"Type:{this.Type}, #Children:{this.OriginalObject.CountChildren}";
        }
    }
}
