namespace SeeingSharp.Multimedia.Drawing3D
{
    public class FloorTileInfo
    {
        /// <summary>
        /// Gets the material used for this tile.
        /// </summary>
        public int MaterialIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTileInfo"/> class.
        /// </summary>
        /// <param name="material">The index of the material to set on this tile.</param>
        public FloorTileInfo(int material)
        {
            this.MaterialIndex = material;
        }
    }
}