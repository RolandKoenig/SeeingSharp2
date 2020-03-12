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
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class Grid3DGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid3DGeometryFactory" /> class.
        /// </summary>
        public Grid3DGeometryFactory()
        {
            this.GenerateGround = true;
            this.LineSmallDivider = 25f;
            this.LineBigDivider = 100f;
            this.TileWidth = 1f;
            this.TilesX = 10;
            this.TilesZ = 10;
            this.GroupTileCount = 5;
            this.BuildBackFaces = true;

            this.HighlightXzLines = false;
            this.ZLineHighlightColor = Color4.BlueColor;
            this.XLineHighlightColor = Color4.GreenColor;

            this.GroundColor = Color4.LightSteelBlue;
            this.LineColor = Color4.LightGray;
        }

        /// <summary>
        /// Builds the geometry
        /// </summary>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            // Calculate parameters
            var firstCoordinate = new Vector3(
                -(this.TilesX * this.TileWidth / 2f),
                0f,
                -(this.TilesZ * this.TileWidth / 2f));
            var tileWidthX = this.TileWidth;
            var tileWidthZ = this.TileWidth;

            var tileMiddleX = this.TilesX % 2 == 0 && this.HighlightXzLines ? this.TilesX / 2 : 1;
            var tileMiddleZ = this.TilesZ % 2 == 0 && this.HighlightXzLines ? this.TilesZ / 2 : 1;

            // Define line geometry
            var genSurfaceDefaultLine = result.CreateSurface();
            var genSurfaceGroupLine = result.CreateSurface();

            for (var actTileX = 0; actTileX < this.TilesX + 1; actTileX++)
            {
                var localStart = firstCoordinate + new Vector3(actTileX * tileWidthX, 0f, 0f);
                var localEnd = localStart + new Vector3(0f, 0f, tileWidthZ * this.TilesZ);

                var actLineColor = this.LineColor;
                var divider = actTileX % this.GroupTileCount == 0 ? this.LineSmallDivider : this.LineBigDivider;

                if (this.HighlightXzLines && actTileX == tileMiddleX)
                {
                    actLineColor = this.ZLineHighlightColor;
                    divider = this.LineSmallDivider;
                }

                var targetGeometry = actTileX % this.GroupTileCount == 0 ? genSurfaceGroupLine : genSurfaceDefaultLine;
                targetGeometry.BuildRect4V(
                    localStart - new Vector3(tileWidthX / divider, 0f, 0f),
                    localStart + new Vector3(tileWidthX / divider, 0f, 0f),
                    localEnd + new Vector3(tileWidthX / divider, 0f, 0f),
                    localEnd - new Vector3(tileWidthX / divider, 0f, 0f))
                    .SetVertexColor(actLineColor);

                if (this.BuildBackFaces)
                {
                    targetGeometry.BuildRect4V(
                        localEnd - new Vector3(tileWidthX / divider, 0f, 0f),
                        localEnd + new Vector3(tileWidthX / divider, 0f, 0f),
                        localStart + new Vector3(tileWidthX / divider, 0f, 0f),
                        localStart - new Vector3(tileWidthX / divider, 0f, 0f))
                        .SetVertexColor(actLineColor);
                }
            }

            for (var actTileZ = 0; actTileZ < this.TilesZ + 1; actTileZ++)
            {
                var localStart = firstCoordinate + new Vector3(0f, 0f, actTileZ * tileWidthZ);
                var localEnd = localStart + new Vector3(tileWidthX * this.TilesX, 0f, 0f);

                var actLineColor = this.LineColor;
                var divider = actTileZ % this.GroupTileCount == 0 ? this.LineSmallDivider : this.LineBigDivider;

                if (this.HighlightXzLines && actTileZ == tileMiddleZ)
                {
                    actLineColor = this.XLineHighlightColor;
                    divider = this.LineSmallDivider;
                }

                var targetGeometry = actTileZ % this.GroupTileCount == 0 ? genSurfaceGroupLine : genSurfaceDefaultLine;
                targetGeometry.BuildRect4V(
                    localStart + new Vector3(0f, 0f, tileWidthZ / divider),
                    localStart - new Vector3(0f, 0f, tileWidthZ / divider),
                    localEnd - new Vector3(0f, 0f, tileWidthZ / divider),
                    localEnd + new Vector3(0f, 0f, tileWidthZ / divider))
                    .SetVertexColor(actLineColor);

                if (this.BuildBackFaces)
                {
                    targetGeometry.BuildRect4V(
                        localEnd + new Vector3(0f, 0f, tileWidthZ / divider),
                        localEnd - new Vector3(0f, 0f, tileWidthZ / divider),
                        localStart - new Vector3(0f, 0f, tileWidthZ / divider),
                        localStart + new Vector3(0f, 0f, tileWidthZ / divider))
                        .SetVertexColor(actLineColor);
                }
            }
            if (genSurfaceDefaultLine.CountTriangles == 0) { result.RemoveSurface(genSurfaceDefaultLine); }
            if (genSurfaceGroupLine.CountTriangles == 0) { result.RemoveSurface(genSurfaceGroupLine); }

            // Return the generated geometry
            return result;
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

        public bool HighlightXzLines
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