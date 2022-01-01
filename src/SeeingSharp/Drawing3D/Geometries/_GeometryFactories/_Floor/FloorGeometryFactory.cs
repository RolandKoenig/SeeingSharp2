using System;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Drawing3D.Primitives;

namespace SeeingSharp.Drawing3D.Geometries
{
    public class FloorGeometryFactory : GeometryFactory
    {
        public const float DEFAULT_HEIGHT = 0.1f;

        //Properties
        private List<FloorTile> _groundTiles;
        private Vector2 _tileSize;
        private int _tilesX;
        private int _tilesY;

        private Vector2 _totalSizeWithoutBorder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorGeometryFactory"/> class.
        /// </summary>
        public FloorGeometryFactory(Vector2 tileSize)
        {
            _groundTiles = new List<FloorTile>();
            _tileSize = tileSize;
        }

        /// <summary>
        /// Sets the tilemap.
        /// </summary>
        /// <param name="tileMap">The new tilemap to apply (use null for empty tiles).</param>
        public void SetTilemap(FloorTileInfo[,] tileMap)
        {
            //Get width and height of the tilemap
            var tilesX = tileMap.GetLength(0);
            var tilesY = tileMap.GetLength(1);
            if (tilesX <= 0)
            {
                throw new ArgumentException("Width of tilemap <= 0!", nameof(tileMap));
            }
            if (tilesY <= 0)
            {
                throw new ArgumentException("Height of tilemap <= 0!", nameof(tileMap));
            }
            _tilesX = tilesX;
            _tilesY = tilesY;

            //Update total size
            _totalSizeWithoutBorder = new Vector2(
                tilesX * _tileSize.X,
                tilesY * _tileSize.Y);

            //Generate all tiles
            _groundTiles.Clear();
            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY] != null)
                    {
                        var newTile = new FloorTile(loopX, loopY, tileMap[loopX, loopY]);
                        _groundTiles.Add(newTile);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a tilemap using the given width and height and sets all tiles to default resource.
        /// </summary>
        /// <param name="width">Width of the tilemap.</param>
        /// <param name="height">Height of the tilemap.</param>
        public void SetTilemap(int width, int height)
        {
            var tilemap = new bool[width, height];
            for (var loopX = 0; loopX < width; loopX++)
            {
                for (var loopY = 0; loopY < height; loopY++)
                {
                    tilemap[loopX, loopY] = true;
                }
            }
            this.SetTilemap(tilemap);
        }

        /// <summary>
        /// Sets the tilemap.
        /// </summary>
        /// <param name="tileMap">The new tilemap to apply (use null for empty tiles).</param>
        public void SetTilemap(bool[,] tileMap)
        {
            // Get width and height of the tilemap
            var tilesX = tileMap.GetLength(0);
            var tilesY = tileMap.GetLength(1);
            if (tilesX <= 0)
            {
                throw new ArgumentException("Width of tilemap <= 0!", nameof(tileMap));
            }
            if (tilesY <= 0)
            {
                throw new ArgumentException("Height of tilemap <= 0!", nameof(tileMap));
            }
            _tilesX = tilesX;
            _tilesY = tilesY;

            // Update total size
            _totalSizeWithoutBorder = new Vector2(
                tilesX * _tileSize.X,
                tilesY * _tileSize.Y);

            // Generate all tiles
            _groundTiles.Clear();

            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY])
                    {
                        var newTile = new FloorTile(loopX, loopY);
                        _groundTiles.Add(newTile);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the center coordinate of the tile at the given location.
        /// </summary>
        /// <param name="xPos">X position of the requested tile.</param>
        /// <param name="yPos">Y position of the requested tile.</param>
        public Vector3 GetTilePosition(int xPos, int yPos)
        {
            //Check parameters
            if (xPos < 0 || xPos >= _tilesX)
            {
                throw new ArgumentException("Invalid x position!", nameof(xPos));
            }
            if (yPos < 0 || yPos >= _tilesY)
            {
                throw new ArgumentException("Invalid y position!", nameof(yPos));
            }

            //Calculate half sizes
            var totalHalfSize = new Vector2(_totalSizeWithoutBorder.X / 2f, _totalSizeWithoutBorder.Y / 2f);
            var tileHalfSize = new Vector2(_tileSize.X / 2f, _tileSize.Y / 2f);

            //Get position of the tile
            return new Vector3(
                xPos * _tileSize.X - totalHalfSize.X + tileHalfSize.X,
                0f,
                yPos * _tileSize.Y - totalHalfSize.Y + tileHalfSize.Y);
        }

        /// <summary>
        /// Builds the geometry.
        /// </summary>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            // Calculate half vector of total ground size.
            var totalHalfSize = new Vector2(_totalSizeWithoutBorder.X / 2f, _totalSizeWithoutBorder.Y / 2f);

            // Build all tiles
            foreach (var actTile in _groundTiles)
            {
                // Get the material of the tile
                var actMaterialIndex = actTile.MaterialIndex;
                if (actMaterialIndex < 0)
                {
                    actMaterialIndex = 0;
                }

                // Get surface object
                GeometrySurface actSurface = null;
                while (result.CountSurfaces <= actMaterialIndex)
                {
                    result.CreateSurface();
                }
                actSurface = result.Surfaces[actMaterialIndex];

                // Get position of the tile
                var tilePosition = new Vector3(
                    actTile.XPos * _tileSize.X - totalHalfSize.X,
                    0f,
                    actTile.YPos * _tileSize.Y - totalHalfSize.Y);

                // AddObject tile information to current Geometry
                actSurface.BuildCubeTop(
                    new Vector3(tilePosition.X, 0, tilePosition.Z),
                    new Vector3(_tileSize.X, 0, _tileSize.Y));
            }

            // Return all generated Geometry
            return result;
        }
    }
}