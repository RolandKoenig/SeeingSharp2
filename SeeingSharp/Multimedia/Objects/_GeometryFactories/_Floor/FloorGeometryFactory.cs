/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Multimedia.Objects
{
    public class FloorGeometryFactory : GeometryFactory
    {
        public const float DEFAULT_HEIGHT = 0.1f;

        //Properties
        private List<FloorTile> m_groundTiles;
        private Vector2 m_tileSize;
        private int m_tilesX;
        private int m_tilesY;

        private Vector2 m_totalSizeWithoutBorder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorGeometryFactory"/> class.
        /// </summary>
        public FloorGeometryFactory(Vector2 tileSize)
        {
            m_groundTiles = new List<FloorTile>();
            m_tileSize = tileSize;
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
            m_tilesX = tilesX;
            m_tilesY = tilesY;

            //Update total size
            m_totalSizeWithoutBorder = new Vector2(
                tilesX * m_tileSize.X,
                tilesY * m_tileSize.Y);

            //Generate all tiles
            m_groundTiles.Clear();
            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY] != null)
                    {
                        var newTile = new FloorTile(loopX, loopY, tileMap[loopX, loopY]);
                        m_groundTiles.Add(newTile);
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
            m_tilesX = tilesX;
            m_tilesY = tilesY;

            // Update total size
            m_totalSizeWithoutBorder = new Vector2(
                tilesX * m_tileSize.X,
                tilesY * m_tileSize.Y);

            // Generate all tiles
            m_groundTiles.Clear();

            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY])
                    {
                        var newTile = new FloorTile(loopX, loopY);
                        m_groundTiles.Add(newTile);
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
            if (xPos < 0 || xPos >= m_tilesX)
            {
                throw new ArgumentException("Invalid x position!", nameof(xPos));
            }
            if (yPos < 0 || yPos >= m_tilesY)
            {
                throw new ArgumentException("Invalid y position!", nameof(yPos));
            }

            //Calculate half sizes
            var totalHalfSize = new Vector2(m_totalSizeWithoutBorder.X / 2f, m_totalSizeWithoutBorder.Y / 2f);
            var tileHalfSize = new Vector2(m_tileSize.X / 2f, m_tileSize.Y / 2f);

            //Get position of the tile
            return new Vector3(
                xPos * m_tileSize.X - totalHalfSize.X + tileHalfSize.X,
                0f,
                yPos * m_tileSize.Y - totalHalfSize.Y + tileHalfSize.Y);
        }

        /// <summary>
        /// Builds the geometry.
        /// </summary>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            // Calculate half vector of total ground size.
            var totalHalfSize = new Vector2(m_totalSizeWithoutBorder.X / 2f, m_totalSizeWithoutBorder.Y / 2f);

            // Build all tiles
            foreach (var actTile in m_groundTiles)
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
                    actTile.XPos * m_tileSize.X - totalHalfSize.X,
                    0f,
                    actTile.YPos * m_tileSize.Y - totalHalfSize.Y);

                // AddObject tile information to current Geometry
                actSurface.BuildCubeTop4V(
                    new Vector3(tilePosition.X, 0, tilePosition.Z),
                    new Vector3(m_tileSize.X, 0, m_tileSize.Y));
            }

            // Return all generated Geometry
            return result;
        }
    }
}