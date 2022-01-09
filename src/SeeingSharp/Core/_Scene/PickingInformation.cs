namespace SeeingSharp.Core
{
    public class PickingInformation
    {
        private float _distance;
        private SceneObject? _pickedObject;

        /// <summary>
        /// The picked object.
        /// </summary>
        public SceneObject? PickedObject => _pickedObject;

        /// <summary>
        /// Gets the distance to the picked object.
        /// </summary>
        public float Distance => _distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PickingInformation" /> class.
        /// </summary>
        public PickingInformation()
        {
            _pickedObject = null;
            _distance = float.NaN;
        }

        /// <summary>
        /// Notifies a pick for the given object with the given distance.
        /// </summary>
        /// <param name="pickedObject">The object that was picked.</param>
        /// <param name="distance">The distance from the origin to the picked point.</param>
        public void NotifyPick(SceneObject pickedObject, float distance)
        {
            if (float.IsNaN(_distance) ||
                distance < _distance)
            {
                _distance = distance;
                _pickedObject = pickedObject;
            }
        }
    }
}
