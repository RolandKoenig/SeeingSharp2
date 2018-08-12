#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using SeeingSharp;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class PalletStackType : ObjectType
    {
        private float m_width;
        private float m_depth;
        private float m_palletHeight;
        private float m_smallFooterWidth;
        private float m_bigFooterWidth;
        private float m_boardHeight;
        private int m_palletCount;
        private Color4 m_palletColor;
        private Color4 m_palletCollor2;
        private NamedOrGenericKey m_palletMaterial;
        private NamedOrGenericKey m_palletMaterial2;

        public PalletStackType(NamedOrGenericKey palletMaterial, int palletCount)
            : this(palletMaterial, NamedOrGenericKey.Empty, palletCount, 0.8f, 1.2f, 0.144f, 0.10f, 0.145f, 0.022f)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletType" /> class.
        /// </summary>
        public PalletStackType()
            : this(NamedOrGenericKey.Empty, NamedOrGenericKey.Empty, 6)
        {

        }

        /// <summary>
        /// Creates a new pallet type with default size
        /// </summary>
        public PalletStackType(NamedOrGenericKey palletMaterial, NamedOrGenericKey contentMaterial, int palletCount)
            : this(palletMaterial, contentMaterial, palletCount, 0.8f, 1.2f, 0.144f, 0.10f, 0.145f, 0.022f)
        {

        }

        /// <summary>
        /// Creates a new pallet type with given size settings
        /// </summary>
        public PalletStackType(
            NamedOrGenericKey palletMaterial, NamedOrGenericKey contentMaterial, 
            int palletCount,
            float width, float depth, float palletHeight, float smallFooterWidth, float bigFooterWidth, float boardHeight)
        {
            m_width = width;
            m_depth = depth;
            m_palletHeight = palletHeight;
            m_bigFooterWidth = bigFooterWidth;
            m_smallFooterWidth = smallFooterWidth;
            m_boardHeight = boardHeight;

            m_palletCount = palletCount;
            if (m_palletCount <= 0) { m_palletCount = 1; }

            m_palletMaterial = palletMaterial;
            m_palletMaterial2 = contentMaterial;

            m_palletColor = Color4Ex.LightGray;
            m_palletColor.ChangeAlphaTo(0.2f);

            m_palletCollor2 = Color4Ex.Transparent;
        }

        /// <summary>
        /// Builds the structure needed for the pallet
        /// </summary>
        /// <param name="buildOptions">Some generic options for structure building</param>
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            VertexStructure result = new VertexStructure();
            VertexStructureSurface surface = result.CreateSurface();

            //Build pallet
            #region -----------------------------------------------------------
            if (buildOptions.IsHighDetail)
            {
                float middleFront = m_width / 2f;
                float middleSide = m_depth / 2f;
                float middleFrontBegin = middleFront - m_bigFooterWidth / 2f;
                float middleSideBegin = middleSide - m_bigFooterWidth / 2f;
                float lastBeginSmall = m_width - m_smallFooterWidth;
                float lastBeginBig = m_depth - m_bigFooterWidth;
                float footerHeight = m_palletHeight - m_boardHeight * 3f;
                float quarterFrontBegin = ((m_bigFooterWidth / 2f) + ((middleFront - (m_bigFooterWidth / 2f)) / 2f)) - (m_smallFooterWidth / 2f);// +(middleFront / 2f - m_smallFooterWidth / 2f);
                float threeQuarterFrontBegin = middleFront + (middleFront - quarterFrontBegin - m_smallFooterWidth);//(middleFront / 2f) * 3f - m_smallFooterWidth / 2f;

                for (int loop = 0; loop < m_palletCount; loop++)
                {
                    Color4 actColor = loop % 2 == 0 ? m_palletColor : m_palletCollor2;
                    float actYCoord = m_palletHeight * loop;

                    surface.Material = m_palletMaterial;

                    //Build 3 board on bottom
                    surface.BuildCube24V(new Vector3(0f, actYCoord, 0f), new Vector3(m_smallFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(middleFrontBegin, actYCoord, 0f), new Vector3(m_bigFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(lastBeginSmall, actYCoord, 0f), new Vector3(m_smallFooterWidth, m_boardHeight, m_depth), actColor);

                    //Build 9 footers
                    surface.BuildCubeSides16V(new Vector3(0f, m_boardHeight + actYCoord, 0f), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(0f, m_boardHeight + actYCoord, middleSideBegin), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(0f, m_boardHeight + actYCoord, lastBeginBig), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(middleFrontBegin, m_boardHeight + actYCoord, 0f), new Vector3(m_bigFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(middleFrontBegin, m_boardHeight + actYCoord, middleSideBegin), new Vector3(m_bigFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(middleFrontBegin, m_boardHeight + actYCoord, lastBeginBig), new Vector3(m_bigFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(lastBeginSmall, m_boardHeight + actYCoord, 0f), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(lastBeginSmall, m_boardHeight + actYCoord, middleSideBegin), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);
                    surface.BuildCubeSides16V(new Vector3(lastBeginSmall, m_boardHeight + actYCoord, lastBeginBig), new Vector3(m_smallFooterWidth, footerHeight, m_bigFooterWidth), actColor);

                    //Build boards above footers
                    surface.BuildCube24V(new Vector3(0f, m_boardHeight + footerHeight + actYCoord, 0f), new Vector3(m_width, m_boardHeight, m_bigFooterWidth), actColor);
                    surface.BuildCube24V(new Vector3(0f, m_boardHeight + footerHeight + actYCoord, middleSideBegin), new Vector3(m_width, m_boardHeight, m_bigFooterWidth), actColor);
                    surface.BuildCube24V(new Vector3(0f, m_boardHeight + footerHeight + actYCoord, lastBeginBig), new Vector3(m_width, m_boardHeight, m_bigFooterWidth), actColor);

                    //Build top boards
                    float localYPos = m_palletHeight - m_boardHeight;
                    surface.BuildCube24V(new Vector3(0f, localYPos + actYCoord, 0f), new Vector3(m_bigFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(middleFrontBegin, localYPos + actYCoord, 0f), new Vector3(m_bigFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(m_width - m_bigFooterWidth, localYPos + actYCoord, 0f), new Vector3(m_bigFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(quarterFrontBegin, localYPos + actYCoord, 0f), new Vector3(m_smallFooterWidth, m_boardHeight, m_depth), actColor);
                    surface.BuildCube24V(new Vector3(threeQuarterFrontBegin, localYPos + actYCoord, 0f), new Vector3(m_smallFooterWidth, m_boardHeight, m_depth), actColor);
                }
            }
            else
            {
                for (int loop = 0; loop < m_palletCount; loop++)
                {
                    Color4 actColor = loop % 2 == 0 ? m_palletColor : m_palletCollor2;
                    surface.BuildCube24V(
                        new Vector3(0f, m_palletHeight * loop, 0f),
                        new Vector3(m_width, m_palletHeight, m_depth),
                        actColor);
                }
            }
            #endregion -----------------------------------------------------------

            Matrix rotMatrix = Matrix.RotationY(EngineMath.RAD_90DEG);

            result.UpdateVerticesUsingRelocationBy(new Vector3(-m_width / 2f, 0f, -m_depth / 2f));
            result.CalculateTangentsAndBinormals();
            result.TransformVertices(rotMatrix);
            result.FitToCenteredCube(1f, FitToCuboidMode.Stretch, SpacialOriginLocation.LowerCenter);

            return result;
        }

        /// <summary>
        /// Gets or sets the width of the object
        /// </summary>
        public float Width
        {
            get { return m_width; }
            set
            {
                if (m_width != value)
                {
                    m_width = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the depth of the object
        /// </summary>
        public float Depth
        {
            get { return m_depth; }
            set
            {
                if (m_depth != value)
                {
                    m_depth = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of a pallet
        /// </summary>
        public float PalletHeight
        {
            get { return m_palletHeight; }
            set
            {
                if (m_palletHeight != value)
                {
                    m_palletHeight = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the small footer
        /// </summary>
        public float SmallFooterWidth
        {
            get { return m_smallFooterWidth; }
            set
            {
                if (m_smallFooterWidth != value)
                {
                    m_smallFooterWidth = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the big footer
        /// </summary>
        public float BigFooterWidth
        {
            get { return m_bigFooterWidth; }
            set
            {
                if (m_bigFooterWidth != value)
                {
                    m_bigFooterWidth = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pallet count.
        /// </summary>
        public int PalletCount
        {
            get { return m_palletCount; }
            set { m_palletCount = value; }
        }

        /// <summary>
        /// Gets or sets the height of a board
        /// </summary>
        public float BoardHeight
        {
            get { return m_boardHeight; }
            set
            {
                if (m_boardHeight != value)
                {
                    m_boardHeight = value;

                    //base.RefreshStructure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the material of the pallet.
        /// </summary>
        public NamedOrGenericKey PalletMaterial
        {
            get { return m_palletMaterial; }
            set { m_palletMaterial = value; }
        }

        /// <summary>
        /// Gets or sets the material of the content.
        /// </summary>
        public NamedOrGenericKey ContentMaterial
        {
            get { return m_palletMaterial2; }
            set { m_palletMaterial2 = value; }
        }

        public Color4 ContentColor
        {
            get { return m_palletCollor2; }
            set { m_palletCollor2 = value; }
        }

        public Color4 PalletColor
        {
            get { return m_palletColor; }
            set { m_palletColor = value; }
        }
    }
}