using System.Numerics;

namespace SeeingSharp.Components.Input
{
    public class FocusedPointCameraComponent : FocusedCameraComponent
    {
        private Vector3 _focusedLocation;

        public float FocusedPointX
        {
            get => _focusedLocation.X;
            set => _focusedLocation.X = value;
        }

        public float FocusedPointY
        {
            get => _focusedLocation.Y;
            set => _focusedLocation.Y = value;
        }

        public float FocusedPointZ
        {
            get => _focusedLocation.Z;
            set => _focusedLocation.Z = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedPointCameraComponent"/> class.
        /// </summary>
        public FocusedPointCameraComponent()
        {
            _focusedLocation = Vector3.Zero;
        }

        public FocusedPointCameraComponent(Vector3 focusedLocation)
        {
            _focusedLocation = focusedLocation;
        }

        protected override Vector3 GetFocusedLocation()
        {
            return _focusedLocation;
        }
    }
}
