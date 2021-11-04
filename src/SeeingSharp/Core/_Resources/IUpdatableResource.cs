namespace SeeingSharp.Core
{
    public interface IUpdatableResource
    {
        /// <summary>
        /// Triggers internal update within the resource (e. g. Render to Texture).
        /// </summary>
        /// <param name="updateState">Current state of update process.</param>
        void Update(UpdateState updateState);
    }
}
