﻿/*
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

using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class Grid3DGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid3DGeometryFactory" /> class.
        /// </summary>
        public Grid3DGeometryFactory()
        {
            GenerateGround = true;
            LineSmallDivider = 25f;
            LineBigDivider = 100f;
            TileWidth = 1f;
            TilesX = 10;
            TilesZ = 10;
            GroupTileCount = 5;
            BuildBackFaces = true;

            HighlightXZLines = false;
            ZLineHighlightColor = Color4Ex.BlueColor;
            XLineHighlightColor = Color4Ex.GreenColor;

            GroundColor = Color4Ex.LightSteelBlue;
            LineColor = Color4Ex.LightGray;
        }

        /// <summary>
        /// Builds the geometry
        /// </summary>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            // Calculate parameters
            var firstCoordinate = new Vector3(
                -(TilesX * TileWidth / 2f),
                0f,
                -(TilesZ * TileWidth / 2f));
            var tileWidthX = TileWidth;
            var tileWidthZ = TileWidth;
            var fieldWidth = tileWidthX * TilesX;
            var fieldDepth = tileWidthZ * TilesZ;
            var fieldWidthHalf = fieldWidth / 2f;
            var fieldDepthHalf = fieldDepth / 2f;

            var tileMiddleX = TilesX % 2 == 0 && HighlightXZLines ? TilesX / 2 : 1;
            var tileMiddleZ = TilesZ % 2 == 0 && HighlightXZLines ? TilesZ / 2 : 1;

            // Define lower ground geometry
            if (GenerateGround)
            {
                var lowerGround = result.CreateSurface();
                lowerGround.EnableTextureTileMode(new Vector2(TileWidth, TileWidth));
                lowerGround.BuildRect4V(
                    new Vector3(-fieldWidthHalf, -0.01f, -fieldDepthHalf),
                    new Vector3(fieldWidthHalf, -0.01f, -fieldDepthHalf),
                    new Vector3(fieldWidthHalf, -0.01f, fieldDepthHalf),
                    new Vector3(-fieldWidthHalf, -0.01f, fieldDepthHalf),
                    new Vector3(0f, 1f, 0f),
                    GroundColor);
                lowerGround.Material = GroundMaterial;
            }

            // Define line geometry
            var genSurfaceDefaultLine = result.CreateSurface();
            var genSurfaceGroupLine = result.CreateSurface();

            for (var actTileX = 0; actTileX < TilesX + 1; actTileX++)
            {
                var localStart = firstCoordinate + new Vector3(actTileX * tileWidthX, 0f, 0f);
                var localEnd = localStart + new Vector3(0f, 0f, tileWidthZ * TilesZ);

                var actLineColor = LineColor;
                var divider = actTileX % GroupTileCount == 0 ? LineSmallDivider : LineBigDivider;

                if (HighlightXZLines && actTileX == tileMiddleX)
                {
                    actLineColor = ZLineHighlightColor;
                    divider = LineSmallDivider;
                }

                var targetGeometry = actTileX % GroupTileCount == 0 ? genSurfaceGroupLine : genSurfaceDefaultLine;
                targetGeometry.BuildRect4V(
                    localStart - new Vector3(tileWidthX / divider, 0f, 0f),
                    localStart + new Vector3(tileWidthX / divider, 0f, 0f),
                    localEnd + new Vector3(tileWidthX / divider, 0f, 0f),
                    localEnd - new Vector3(tileWidthX / divider, 0f, 0f),
                    actLineColor);

                if(BuildBackFaces)
                {
                    targetGeometry.BuildRect4V(
                        localEnd - new Vector3(tileWidthX / divider, 0f, 0f),
                        localEnd + new Vector3(tileWidthX / divider, 0f, 0f),
                        localStart + new Vector3(tileWidthX / divider, 0f, 0f),
                        localStart - new Vector3(tileWidthX / divider, 0f, 0f),
                        actLineColor);
                }
            }

            for (var actTileZ = 0; actTileZ < TilesZ + 1; actTileZ++)
            {
                var localStart = firstCoordinate + new Vector3(0f, 0f, actTileZ * tileWidthZ);
                var localEnd = localStart + new Vector3(tileWidthX * TilesX, 0f, 0f);

                var actLineColor = LineColor;
                var divider = actTileZ % GroupTileCount == 0 ? LineSmallDivider : LineBigDivider;

                if (HighlightXZLines && actTileZ == tileMiddleZ)
                {
                    actLineColor = XLineHighlightColor;
                    divider = LineSmallDivider;
                }

                var targetGeometry = actTileZ % GroupTileCount == 0 ? genSurfaceGroupLine : genSurfaceDefaultLine;
                targetGeometry.BuildRect4V(
                    localStart + new Vector3(0f, 0f, tileWidthZ / divider),
                    localStart - new Vector3(0f, 0f, tileWidthZ / divider),
                    localEnd - new Vector3(0f, 0f, tileWidthZ / divider),
                    localEnd + new Vector3(0f, 0f, tileWidthZ / divider),
                    actLineColor);

                if(BuildBackFaces)
                {
                    targetGeometry.BuildRect4V(
                        localEnd + new Vector3(0f, 0f, tileWidthZ / divider),
                        localEnd - new Vector3(0f, 0f, tileWidthZ / divider),
                        localStart - new Vector3(0f, 0f, tileWidthZ / divider),
                        localStart + new Vector3(0f, 0f, tileWidthZ / divider),
                        actLineColor);
                }
            }
            genSurfaceDefaultLine.Material = LineMaterial;
            genSurfaceGroupLine.Material = LineMaterial;
            if (genSurfaceDefaultLine.CountTriangles == 0) { result.RemoveSurface(genSurfaceDefaultLine); }
            if (genSurfaceGroupLine.CountTriangles == 0) { result.RemoveSurface(genSurfaceGroupLine); }

            // Return the generated geometry
            return result;
        }

        public NamedOrGenericKey LineMaterial
        {
            get;
            set;
        }

        public NamedOrGenericKey GroupLineMaterial
        {
            get;
            set;
        }

        public NamedOrGenericKey GroundMaterial
        {
            get;
            set;
        }

        public int GroupTileCount
        {
            get;
            set;
        }

        public Color4 LineColor
        {
            get;
            set;
        }

        public Color4 GroundColor
        {
            get;
            set;
        }

        public bool HighlightXZLines
        {
            get;
            set;
        }

        public Color4 XLineHighlightColor
        {
            get;
            set;
        }

        public Color4 ZLineHighlightColor
        {
            get;
            set;
        }

        public bool BuildBackFaces
        {
            get;
            set;
        }

        public float TileWidth
        {
            get;
            set;
        }

        public int TilesX
        {
            get;
            set;
        }

        public int TilesZ
        {
            get;
            set;
        }

        public bool GenerateGround
        {
            get;
            set;
        }

        public float LineSmallDivider
        {
            get;
            set;
        }

        public float LineBigDivider
        {
            get;
            set;
        }
    }
}