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
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class FloorGeometryFactory : GeometryFactory
    {
        public const float DEFAULT_HEIGHT = 0.1f;

        //Material names
        private List<BorderInformation> m_borders;

        //Properties
        private float m_borderSize;
        private List<FloorTile> m_groundTiles;
        private float m_height;
        private Vector2 m_tileSize;
        private int m_tilesX;
        private int m_tilesY;

        private Vector2 m_totalSizeWithoutBorder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorGeometryFactory"/> class.
        /// </summary>
        public FloorGeometryFactory(Vector2 tileSize, float borderSize)
        {
            m_height = DEFAULT_HEIGHT;
            m_groundTiles = new List<FloorTile>();
            m_borders = new List<BorderInformation>();
            m_tileSize = tileSize;
            m_borderSize = borderSize;
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
            if (tilesX <= 0) { throw new ArgumentException("Width of tilemap <= 0!", nameof(tileMap)); }
            if (tilesY <= 0) { throw new ArgumentException("Height of tilemap <= 0!", nameof(tileMap)); }
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

            //Generate all borders
            var boolTileMap = CreateBooleanMap(tileMap);
            GenerateBorders(boolTileMap, tilesX, tilesY);
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
            SetTilemap(tilemap);
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
            if (tilesX <= 0) { throw new ArgumentException("Width of tilemap <= 0!", nameof(tileMap)); }
            if (tilesY <= 0) { throw new ArgumentException("Height of tilemap <= 0!", nameof(tileMap)); }
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

            // Generate all borders
            GenerateBorders(tileMap, tilesX, tilesY);
        }

        /// <summary>
        /// Gets the center coordinate of the tile at the given location.
        /// </summary>
        /// <param name="xPos">X position of the requested tile.</param>
        /// <param name="yPos">Y position of the requested tile.</param>
        public Vector3 GetTilePosition(int xPos, int yPos)
        {
            //Check parameters
            if (xPos < 0 || xPos >= m_tilesX) { throw new ArgumentException("Invalid x position!", nameof(xPos)); }
            if (yPos < 0 || yPos >= m_tilesY) { throw new ArgumentException("Invalid y position!", nameof(yPos)); }

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
        /// Builds the structure.
        /// </summary>
        public override VertexStructure BuildStructure(GeometryBuildOptions buildOptions)
        {
            var result = new VertexStructure();

            // Hold dictionary containing materials and corresponding structures
            var materialRelated = new Dictionary<NamedOrGenericKey, VertexStructureSurface>();

            // Build bottom structure
            var bottomSurface = result.CreateSurface();
            bottomSurface.Material = BottomMaterial;
            materialRelated[BottomMaterial] = bottomSurface;

            // Calculate half vector of total ground size.
            var totalHalfSize = new Vector2(m_totalSizeWithoutBorder.X / 2f, m_totalSizeWithoutBorder.Y / 2f);
            var tileHalfSize = new Vector2(m_tileSize.X / 2f, m_tileSize.Y / 2f);

            // Build all tiles
            foreach (var actTile in m_groundTiles)
            {
                // Get the material of the tile
                var actMaterial = actTile.Material;

                if (actMaterial.IsEmpty)
                {
                    actMaterial = DefaultFloorMaterial;
                }

                // Get surface object
                VertexStructureSurface actSurface = null;

                if (materialRelated.ContainsKey(actMaterial))
                {
                    actSurface = materialRelated[actMaterial];
                }
                else
                {
                    actSurface = result.CreateSurface();
                    actSurface.Material = actMaterial;
                    materialRelated[actMaterial] = actSurface;
                }

                // Get position of the tile
                var tilePosition = new Vector3(
                    actTile.XPos * m_tileSize.X - totalHalfSize.X,
                    0f,
                    actTile.YPos * m_tileSize.Y - totalHalfSize.Y);

                // Add tile information to current VertexStructures
                actSurface.BuildCubeTop4V(
                    new Vector3(tilePosition.X, -m_height, tilePosition.Z),
                    new Vector3(m_tileSize.X, m_height, m_tileSize.Y),
                    Color4.White);
                bottomSurface.BuildCubeBottom4V(
                    new Vector3(tilePosition.X, -m_height, tilePosition.Z),
                    new Vector3(m_tileSize.X, m_height, m_tileSize.Y),
                    Color4.White);
            }

            // Build all borders
            VertexStructureSurface borderSurface = null;
            if (materialRelated.ContainsKey(BorderMaterial)) { borderSurface = materialRelated[BorderMaterial]; }
            else
            {
                borderSurface = result.CreateSurface();
                borderSurface.Material = BorderMaterial;
                materialRelated[BorderMaterial] = borderSurface;
            }
            foreach (var actBorder in m_borders)
            {
                if (m_borderSize <= 0f)
                {
                    var tilePosition = new Vector3(
                        actBorder.TileXPos * m_tileSize.X - totalHalfSize.X,
                        0f,
                        actBorder.TileYPos * m_tileSize.Y - totalHalfSize.Y);

                    //Build simple borders
                    switch (actBorder.Location)
                    {
                        case BorderLocation.Left:
                            borderSurface.BuildRect4V(
                                new Vector3(tilePosition.X, -m_height, tilePosition.Z),
                                new Vector3(tilePosition.X, 0f, tilePosition.Z),
                                new Vector3(tilePosition.X, 0f, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X, -m_height, tilePosition.Z + m_tileSize.Y));
                            break;

                        case BorderLocation.Top:
                            borderSurface.BuildRect4V(
                                new Vector3(tilePosition.X, -m_height, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X, 0f, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X + m_tileSize.X, 0f, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X + m_tileSize.X, -m_height, tilePosition.Z + m_tileSize.Y));
                            break;

                        case BorderLocation.Right:
                            borderSurface.BuildRect4V(
                                new Vector3(tilePosition.X + m_tileSize.X, -m_height, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X + m_tileSize.X, 0f, tilePosition.Z + m_tileSize.Y),
                                new Vector3(tilePosition.X + m_tileSize.X, 0f, tilePosition.Z),
                                new Vector3(tilePosition.X + m_tileSize.X, -m_height, tilePosition.Z));
                            break;

                        case BorderLocation.Bottom:
                            borderSurface.BuildRect4V(
                                new Vector3(tilePosition.X + m_tileSize.X, -m_height, tilePosition.Z),
                                new Vector3(tilePosition.X + m_tileSize.X, 0f, tilePosition.Z),
                                new Vector3(tilePosition.X, 0f, tilePosition.Z),
                                new Vector3(tilePosition.X, -m_height, tilePosition.Z));
                            break;
                    }
                }
            }

            //Return all generated VertexStructures
            return result;
        }

        /// <summary>
        /// Creates a boolean map out of given tilemap.
        /// </summary>
        /// <param name="tileMap">The tilemap to convert.</param>
        private bool[,] CreateBooleanMap(FloorTileInfo[,] tileMap)
        {
            var tilesX = tileMap.GetLength(0);
            var tilesY = tileMap.GetLength(1);

            //Convert tilemap
            var result = new bool[tilesX, tilesY];
            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    result[loopX, loopY] = tileMap[loopX, loopY] != null;
                }
            }

            return result;
        }

        /// <summary>
        /// Generates borders for the ground.
        /// </summary>
        /// <param name="tileMap">The tilemap.</param>
        /// <param name="tilesX">Tiles in x direction.</param>
        /// <param name="tilesY">Tiles in y direction.</param>
        private void GenerateBorders(bool[,] tileMap, int tilesX, int tilesY)
        {
            m_borders.Clear();
            for (var loopX = 0; loopX < tilesX; loopX++)
            {
                for (var loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY])
                    {
                        if (loopY == 0 || !tileMap[loopX, loopY - 1])
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Bottom));
                        }
                        if (loopX == 0 || !tileMap[loopX - 1, loopY])
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Left));
                        }
                        if (loopY == tilesY - 1 || !tileMap[loopX, loopY + 1])
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Top));
                        }
                        if (loopX == tilesX - 1 || !tileMap[loopX + 1, loopY])
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Right));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the border material
        /// </summary>
        public NamedOrGenericKey BorderMaterial { get; set; }

        /// <summary>
        /// Gets or sets the ground material
        /// </summary>
        public NamedOrGenericKey DefaultFloorMaterial { get; set; }

        /// <summary>
        /// Gets or sets material for sides.
        /// </summary>
        public NamedOrGenericKey SideMaterial { get; set; }

        /// <summary>
        /// Gets or sets the material for bottom.
        /// </summary>
        public NamedOrGenericKey BottomMaterial { get; set; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private enum BorderLocation
        {
            Left,
            LeftTop,
            LeftBottom,
            Top,
            Right,
            RightTop,
            RightBottom,
            Bottom
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class BorderInformation
        {
            public BorderInformation(int xPos, int yPos, BorderLocation location)
            {
                TileXPos = xPos;
                TileYPos = yPos;
                Location = location;
            }

            public BorderLocation Location;
            public int TileXPos;
            public int TileYPos;
        }
    }
}