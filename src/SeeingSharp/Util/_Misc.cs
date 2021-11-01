namespace SeeingSharp.Util
{
    /// <summary>
    /// An interface which can return the disposed state of an object.
    /// </summary>
    public interface ICheckDisposed
    {
        /// <summary>
        /// Is this object already disposed?
        /// This property does not throw an <see cref="System.ObjectDisposedException"/>
        /// </summary>
        bool IsDisposed { get; }
    }
}