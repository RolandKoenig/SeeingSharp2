using System.Numerics;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Components
{
    public class FocusedObjectCameraComponent : FocusedCameraComponent
    {
        public SceneSpacialObject FocusedObject
        {
            get;
            set;
        }

        protected override Vector3 GetFocusedLocation()
        {
            var focusedObject = this.FocusedObject;

            if (focusedObject != null)
            {
                return focusedObject.Position;
            }
            return Vector3.Zero;
        }
    }
}