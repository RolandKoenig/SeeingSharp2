#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class FloorType : ObjectType
    {
        public const float DEFAULT_HEIGHT = 0.1f;

        //Properties
        private float m_borderSize;

        private Vector2 m_totalSizeWithoutBorder;
        private Vector2 m_tileSize;
        private float m_height;
        private int m_tilesX;
        private int m_tilesY;
        private List<FloorTile> m_groundTiles;
        private List<BorderInformation> m_borders;

        //Material names
        private NamedOrGenericKey m_borderMaterial;
        private NamedOrGenericKey m_groundMaterial;
        private NamedOrGenericKey m_sideMaterial;
        private NamedOrGenericKey m_bottomMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorType"/> class.
        /// </summary>
        public FloorType(Vector2 tileSize, float borderSize)
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
            int tilesX = tileMap.GetLength(0);
            int tilesY = tileMap.GetLength(1);
            if (tilesX <= 0) { throw new ArgumentException("Width of tilemap <= 0!", "tileMap"); }
            if (tilesY <= 0) { throw new ArgumentException("Height of tilemap <= 0!", "tileMap"); }
            m_tilesX = tilesX;
            m_tilesY = tilesY;

            //Update total size
            m_totalSizeWithoutBorder = new Vector2(
                tilesX * m_tileSize.X,
                tilesY * m_tileSize.Y);

            //Generate all tiles
            m_groundTiles.Clear();
            for (int loopX = 0; loopX < tilesX; loopX++)
            {
                for (int loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY] != null)
                    {
                        FloorTile newTile = new FloorTile(loopX, loopY, tileMap[loopX, loopY]);
                        m_groundTiles.Add(newTile);
                    }
                }
            }

            //Generate all borders
            bool[,] boolTileMap = CreateBooleanMap(tileMap);
            GenerateBorders(boolTileMap, tilesX, tilesY);
        }

        /// <summary>
        /// Sets a tilemap using the given width and height and sets all tiles to default resource.
        /// </summary>
        /// <param name="width">Width of the tilemap.</param>
        /// <param name="height">Height of the tilemap.</param>
        public void SetTilemap(int width, int height)
        {
            bool[,] tilemap = new bool[width, height];
            for (int loopX = 0; loopX < width; loopX++)
            {
                for (int loopY = 0; loopY < height; loopY++)
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
            //Get width and height of the tilemap
            int tilesX = tileMap.GetLength(0);
            int tilesY = tileMap.GetLength(1);
            if (tilesX <= 0) { throw new ArgumentException("Width of tilemap <= 0!", "tileMap"); }
            if (tilesY <= 0) { throw new ArgumentException("Height of tilemap <= 0!", "tileMap"); }
            m_tilesX = tilesX;
            m_tilesY = tilesY;

            //Update total size
            m_totalSizeWithoutBorder = new Vector2(
                tilesX * m_tileSize.X,
                tilesY * m_tileSize.Y);

            //Generate all tiles
            m_groundTiles.Clear();
            for (int loopX = 0; loopX < tilesX; loopX++)
            {
                for (int loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY])
                    {
                        FloorTile newTile = new FloorTile(loopX, loopY);
                        m_groundTiles.Add(newTile);
                    }
                }
            }

            //Generate all borders
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
            if ((xPos < 0) || (xPos >= m_tilesX)) { throw new ArgumentException("Invalid x position!", "xPos"); }
            if ((yPos < 0) || (yPos >= m_tilesY)) { throw new ArgumentException("Invalid y position!", "yPos"); }

            //Calculate half sizes
            Vector2 totalHalfSize = new Vector2(m_totalSizeWithoutBorder.X / 2f, m_totalSizeWithoutBorder.Y / 2f);
            Vector2 tileHalfSize = new Vector2(m_tileSize.X / 2f, m_tileSize.Y / 2f);

            //Get position of the tile
            return new Vector3(
                (xPos * m_tileSize.X) - totalHalfSize.X + tileHalfSize.X,
                0f,
                (yPos * m_tileSize.Y) - totalHalfSize.Y + tileHalfSize.Y);
        }

        /// <summary>
        /// Builds the structure.
        /// </summary>
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            VertexStructure result = new VertexStructure();

            // Hold dictionary containg materials and corresponding structures
            Dictionary<NamedOrGenericKey, VertexStructureSurface> materialRelated = new Dictionary<NamedOrGenericKey, VertexStructureSurface>();

            // Build bottom structure
            VertexStructureSurface bottomSurface = result.CreateSurface();
            bottomSurface.Material = m_bottomMaterial;
            materialRelated[m_bottomMaterial] = bottomSurface;

            // Calculate half vector of total ground size.
            Vector2 totalHalfSize = new Vector2(m_totalSizeWithoutBorder.X / 2f, m_totalSizeWithoutBorder.Y / 2f);
            Vector2 tileHalfSize = new Vector2(m_tileSize.X / 2f, m_tileSize.Y / 2f);

            // Build all tiles
            foreach (FloorTile actTile in m_groundTiles)
            {
                // Get the material of the tile
                NamedOrGenericKey actMaterial = actTile.Material;
                if (actMaterial.IsEmpty) { actMaterial = m_groundMaterial; }

                // Get surface object
                VertexStructureSurface actSurface = null;
                if (materialRelated.ContainsKey(actMaterial)) { actSurface = materialRelated[actMaterial]; }
                else
                {
                    actSurface = result.CreateSurface();
                    actSurface.Material = actMaterial;
                    materialRelated[actMaterial] = actSurface;
                }

                // Get position of the tile
                Vector3 tilePosition = new Vector3(
                    (actTile.XPos * m_tileSize.X) - totalHalfSize.X,
                    0f,
                    (actTile.YPos * m_tileSize.Y) - totalHalfSize.Y);

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
            if (materialRelated.ContainsKey(m_borderMaterial)) { borderSurface = materialRelated[m_borderMaterial]; }
            else
            {
                borderSurface = result.CreateSurface();
                borderSurface.Material = m_borderMaterial;
                materialRelated[m_borderMaterial] = borderSurface;
            }
            foreach (BorderInformation actBorder in m_borders)
            {
                if (m_borderSize <= 0f)
                {
                    Vector3 tilePosition = new Vector3(
                        (actBorder.TileXPos * m_tileSize.X) - totalHalfSize.X,
                        0f,
                        (actBorder.TileYPos * m_tileSize.Y) - totalHalfSize.Y);

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
                else
                {
                    //Build complex borders
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
            int tilesX = tileMap.GetLength(0);
            int tilesY = tileMap.GetLength(1);

            //Convert tilemap
            bool[,] result = new bool[tilesX, tilesY];
            for (int loopX = 0; loopX < tilesX; loopX++)
            {
                for (int loopY = 0; loopY < tilesY; loopY++)
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
            for (int loopX = 0; loopX < tilesX; loopX++)
            {
                for (int loopY = 0; loopY < tilesY; loopY++)
                {
                    if (tileMap[loopX, loopY])
                    {
                        if ((loopY == 0) || (!tileMap[loopX, loopY - 1]))
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Bottom));
                        }
                        if ((loopX == 0) || (!tileMap[loopX - 1, loopY]))
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Left));
                        }
                        if ((loopY == tilesY - 1) || (!tileMap[loopX, loopY + 1]))
                        {
                            m_borders.Add(new BorderInformation(loopX, loopY, BorderLocation.Top));
                        }
                        if ((loopX == tilesX - 1) || (!tileMap[loopX + 1, loopY]))
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
        public NamedOrGenericKey BorderMaterial
        {
            get { return m_borderMaterial; }
            set
            {
                if (m_borderMaterial != value)
                {
                    m_borderMaterial = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ground material
        /// </summary>
        public NamedOrGenericKey DefaultFloorMaterial
        {
            get { return m_groundMaterial; }
            set
            {
                if (m_groundMaterial != value)
                {
                    m_groundMaterial = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets material for sides.
        /// </summary>
        public NamedOrGenericKey SideMaterial
        {
            get { return m_sideMaterial; }
            set
            {
                if (m_sideMaterial != value)
                {
                    m_sideMaterial = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the material for bottom.
        /// </summary>
        public NamedOrGenericKey BottomMaterial
        {
            get { return m_bottomMaterial; }
            set
            {
                if (m_bottomMaterial != value)
                {
                    m_bottomMaterial = value;
                }
            }
        }

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
            public int TileXPos;
            public int TileYPos;
            public BorderLocation Location;

            public BorderInformation(int xPos, int yPos, BorderLocation location)
            {
                this.TileXPos = xPos;
                this.TileYPos = yPos;
                this.Location = location;
            }
        }
    }
}