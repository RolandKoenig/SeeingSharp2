using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    /// <summary>
    /// All needed information for exported geometry.
    /// </summary>
    public class ExportGeometryInfo
    {
        public NamedOrGenericKey Key { get; }

        public GeometryFactory ObjectType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportGeometryInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objType">Type of the object.</param>
        public ExportGeometryInfo(NamedOrGenericKey key, GeometryFactory objType)
        {
            this.Key = key;
            this.ObjectType = objType;
        }
    }
}