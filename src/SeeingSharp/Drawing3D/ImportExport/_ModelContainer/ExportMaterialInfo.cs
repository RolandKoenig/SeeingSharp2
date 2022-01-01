using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    /// <summary>
    /// All needed information for exported material.
    /// </summary>
    public class ExportMaterialInfo
    {
        public NamedOrGenericKey Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportMaterialInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public ExportMaterialInfo(NamedOrGenericKey key)
        {
            this.Key = key;
        }
    }
}