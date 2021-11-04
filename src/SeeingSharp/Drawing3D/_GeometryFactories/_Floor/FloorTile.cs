namespace SeeingSharp.Drawing3D
{
    public class FloorTile
    {
        private FloorTileInfo _tileInfo;

        /// <summary>
        /// Gets the material used by this tile.
        /// </summary>
        public int MaterialIndex => _tileInfo.MaterialIndex;

        /// <summary>
        /// Gets the x-position of the tile.
        /// </summary>
        public int XPos { get; }

        /// <summary>
        /// Gets the y-position of the tile.
        /// </summary>
        public int YPos { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTile"/> class.
        /// </summary>
        /// <param name="xPos">The x pos.</param>
        /// <param name="yPos">The y pos.</param>
        internal FloorTile(int xPos, int yPos)
        {
            this.XPos = xPos;
            this.YPos = yPos;
            _tileInfo = new FloorTileInfo(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTile"/> class.
        /// </summary>
        /// <param name="xPos">The x pos.</param>
        /// <param name="yPos">The y pos.</param>
        /// <param name="tileInfo">Gets some generic information about the tile.</param>
        internal FloorTile(int xPos, int yPos, FloorTileInfo tileInfo)
        {
            this.XPos = xPos;
            this.YPos = yPos;
            _tileInfo = tileInfo;
        }
    }
}