#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class Grid3DType : ObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grid3DType" /> class.
        /// </summary>
        public Grid3DType()
        {
            GenerateGround = true;
            LineSmallDevider = 25f;
            LineBigDevider = 100f;
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
        /// Builds the structures.
        /// </summary>
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            var result = new VertexStructure();

            // Calculate parameters
            var firstCoordinate = new Vector3(
                -((TilesX * TileWidth) / 2f),
                0f,
                -((TilesZ * TileWidth) / 2f));
            var tileWidthX = TileWidth;
            var tileWidthZ = TileWidth;
            var fieldWidth = tileWidthX * TilesX;
            var fieldDepth = tileWidthZ * TilesZ;
            var fieldWidthHalf = fieldWidth / 2f;
            var fieldDepthHalf = fieldDepth / 2f;

            var tileMiddleX = (TilesX % 2 == 0) && (HighlightXZLines) ? TilesX / 2 : 1;
            var tileMiddleZ = (TilesZ % 2 == 0) && (HighlightXZLines) ? TilesZ / 2 : 1;

            // Define lower ground structure
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

            // Define line structures
            var genStructureDefaultLine = result.CreateSurface();
            var genStructureGroupLine = result.CreateSurface();

            for (var actTileX = 0; actTileX < TilesX + 1; actTileX++)
            {
                var localStart = firstCoordinate + new Vector3(actTileX * tileWidthX, 0f, 0f);
                var localEnd = localStart + new Vector3(0f, 0f, tileWidthZ * TilesZ);

                var actLineColor = LineColor;
                var devider = actTileX % GroupTileCount == 0 ? LineSmallDevider : LineBigDevider;

                if (HighlightXZLines && (actTileX == tileMiddleX))
                {
                    actLineColor = ZLineHighlightColor;
                    devider = LineSmallDevider;
                }

                var targetStruture = actTileX % GroupTileCount == 0 ? genStructureGroupLine : genStructureDefaultLine;
                targetStruture.BuildRect4V(
                    localStart - new Vector3(tileWidthX / devider, 0f, 0f),
                    localStart + new Vector3(tileWidthX / devider, 0f, 0f),
                    localEnd + new Vector3(tileWidthX / devider, 0f, 0f),
                    localEnd - new Vector3(tileWidthX / devider, 0f, 0f),
                    actLineColor);

                if(BuildBackFaces)
                {
                    targetStruture.BuildRect4V(
                        localEnd - new Vector3(tileWidthX / devider, 0f, 0f),
                        localEnd + new Vector3(tileWidthX / devider, 0f, 0f),
                        localStart + new Vector3(tileWidthX / devider, 0f, 0f),
                        localStart - new Vector3(tileWidthX / devider, 0f, 0f),
                        actLineColor);
                }
            }

            for (var actTileZ = 0; actTileZ < TilesZ + 1; actTileZ++)
            {
                var localStart = firstCoordinate + new Vector3(0f, 0f, actTileZ * tileWidthZ);
                var localEnd = localStart + new Vector3(tileWidthX * TilesX, 0f, 0f);

                var actLineColor = LineColor;
                var devider = actTileZ % GroupTileCount == 0 ? LineSmallDevider : LineBigDevider;

                if (HighlightXZLines && (actTileZ == tileMiddleZ))
                {
                    actLineColor = XLineHighlightColor;
                    devider = LineSmallDevider;
                }

                var targetStruture = actTileZ % GroupTileCount == 0 ? genStructureGroupLine : genStructureDefaultLine;
                targetStruture.BuildRect4V(
                    localStart + new Vector3(0f, 0f, tileWidthZ / devider),
                    localStart - new Vector3(0f, 0f, tileWidthZ / devider),
                    localEnd - new Vector3(0f, 0f, tileWidthZ / devider),
                    localEnd + new Vector3(0f, 0f, tileWidthZ / devider),
                    actLineColor);

                if(BuildBackFaces)
                {
                    targetStruture.BuildRect4V(
                        localEnd + new Vector3(0f, 0f, tileWidthZ / devider),
                        localEnd - new Vector3(0f, 0f, tileWidthZ / devider),
                        localStart - new Vector3(0f, 0f, tileWidthZ / devider),
                        localStart + new Vector3(0f, 0f, tileWidthZ / devider),
                        actLineColor);
                }
            }
            genStructureDefaultLine.Material = LineMaterial;
            genStructureGroupLine.Material = LineMaterial;

            if (genStructureDefaultLine.CountTriangles == 0) { result.RemoveSurface(genStructureDefaultLine); }
            if (genStructureGroupLine.CountTriangles == 0) { result.RemoveSurface(genStructureGroupLine); }

            // Return all generated structures
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

        public float LineSmallDevider
        {
            get;
            set;
        }

        public float LineBigDevider
        {
            get;
            set;
        }
    }
}