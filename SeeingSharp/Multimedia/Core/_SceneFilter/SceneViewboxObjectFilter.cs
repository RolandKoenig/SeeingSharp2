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
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneViewboxObjectFilter : SceneObjectFilter
    {
        #region Values for viewbox clipping
        private ViewInformation m_viewInfo;
        private BoundingFrustum m_boundingFrustum;
        #endregion

        #region Values for y-filter
        private bool m_enableYFilter;
        private float m_yFilterMin;
        private float m_yFilterMax;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneViewboxObjectFilter"/> class.
        /// </summary>
        public SceneViewboxObjectFilter()
        {
            // Default configuration of the y-filter
            m_enableYFilter = false;
            m_yFilterMin = 0f;
            m_yFilterMax = 10f;
        }

        /// <summary>
        /// Sets current environment data.
        /// </summary>
        /// <param name="layerToFilter">The SceneLayer that gets filtered.</param>
        /// <param name="viewInformation">The information object of the corresponding view.</param>
        public override void SetEnvironmentData(SceneLayer layerToFilter, ViewInformation viewInformation)
        {
            m_viewInfo = viewInformation;
            m_boundingFrustum = viewInformation.CameraBoundingFrustum;
        }

        /// <summary>
        /// Checks for visibility of the given object.
        /// </summary>
        /// <param name="input">The object to be checked..</param>
        /// <param name="viewInfo">The view on which to check for visibility.</param>
        public override bool IsObjectVisible(SceneObject input, ViewInformation viewInfo)
        {
            if (m_viewInfo == null) { return true; }

            // Perform viewbox clipping
            if (!input.IsInBoundingFrustum(m_viewInfo, ref m_boundingFrustum)) { return false; }

            // Handle Y-Filter
            if ((m_enableYFilter) &&
                (m_yFilterMin != m_yFilterMax) &&
                (m_yFilterMax > m_yFilterMin) &&
                (m_yFilterMax - m_yFilterMin > 0.1f))
            {
                SceneSpacialObject spacialObject = input as SceneSpacialObject;
                if (spacialObject != null)
                {
                    // Get the bounding box of the object
                    BoundingBox boundingBox = spacialObject.TryGetBoundingBox(viewInfo);
                    if (boundingBox.IsEmpty()) { boundingBox = new BoundingBox(spacialObject.Position, spacialObject.Position + new Vector3(0.1f, 0.1f, 0.1f)); }

                    // Perform some checks based on the bounding box
                    if (boundingBox.GetUpperA().Y < m_yFilterMin) { return false; }
                    if (boundingBox.GetLowerA().Y > m_yFilterMax) { return false; }
                }
            }

            // Object is visible
            return true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the y-filter is enabled.
        /// </summary>
        public bool EnableYFilter
        {
            get { return m_enableYFilter; }
            set
            {
                if (m_enableYFilter != value)
                {
                    m_enableYFilter = value;
                    RaiseFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the y-filter.
        /// </summary>
        public float YFilterMin
        {
            get { return m_yFilterMin; }
            set
            {
                if (m_yFilterMin != value)
                {
                    m_yFilterMin = value;
                    RaiseFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the y-filter.
        /// </summary>
        public float YFilterMax
        {
            get { return m_yFilterMax; }
            set
            {
                if (m_yFilterMax != value)
                {
                    m_yFilterMax = value;
                    RaiseFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Should this filter be updated on each frame?
        /// </summary>
        public override bool UpdateEachFrame
        {
            get { return true; }
        }
    }
}
