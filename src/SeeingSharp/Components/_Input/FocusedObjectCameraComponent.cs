using System.Numerics;
using SeeingSharp.Core;

namespace SeeingSharp.Components
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