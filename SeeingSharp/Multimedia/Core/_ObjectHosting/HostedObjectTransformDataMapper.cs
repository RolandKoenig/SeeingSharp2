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
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public class HostedObjectTransformDataMapper : IEngineHostedSceneObject, IEngineOpacityProvider
    {
        private IEngineHostedSceneObject m_original;

        private Func<Vector3, Vector3> m_funcPositionMapper;
        private Func<Vector3, Vector3> m_funcRotationMapper;
        private Func<Vector3, Vector3> m_funcScalingMapper;
        private Func<AnimationHandler, AnimationHandler> m_funcGetAnimationHandler;
        private Func<Color4, Color4> m_funcGetDisplayColor;
        private IEngineOpacityProvider m_opacityObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedObjectTransformDataMapper"/> class.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <param name="funcPositionMapper">The function position mapper.</param>
        /// <param name="funcRotationMapper">The function rotation mapper.</param>
        /// <param name="funcScalingMapper">The function scaling mapper.</param>
        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Vector3, Vector3> funcPositionMapper,
            Func<Vector3, Vector3> funcRotationMapper,
            Func<Vector3, Vector3> funcScalingMapper)
        {
            m_original = originalObject;
            m_funcPositionMapper = funcPositionMapper;
            m_funcRotationMapper = funcRotationMapper;
            m_funcScalingMapper = funcScalingMapper;
            m_funcGetAnimationHandler = null;
            m_funcGetDisplayColor = null;
            m_opacityObject = originalObject as IEngineOpacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Vector3, Vector3> funcPositionMapper,
            Func<Vector3, Vector3> funcRotationMapper,
            Func<Vector3, Vector3> funcScalingMapper,
            Func<Color4, Color4> funcColorMapper)
        {
            m_original = originalObject;
            m_funcPositionMapper = funcPositionMapper;
            m_funcRotationMapper = funcRotationMapper;
            m_funcScalingMapper = funcScalingMapper;
            m_funcGetDisplayColor = funcColorMapper;
            m_opacityObject = originalObject as IEngineOpacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Vector3, Vector3> funcPositionMapper,
            Func<Vector3, Vector3> funcRotationMapper,
            Func<Vector3, Vector3> funcScalingMapper,
            Func<Color4, Color4> funcColorMapper,
            IEngineOpacityProvider opacityProvider)
        {
            m_original = originalObject;
            m_funcPositionMapper = funcPositionMapper;
            m_funcRotationMapper = funcRotationMapper;
            m_funcScalingMapper = funcScalingMapper;
            m_funcGetDisplayColor = funcColorMapper;
            m_opacityObject = opacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Color4, Color4> funcColorMapper)
            : this(
                originalObject,
                (actPos) => actPos,
                (actRot) => actRot,
                (actScale) => actScale,
                funcColorMapper)
        {
        }

        public Vector3 Position
        {
            get { return m_funcPositionMapper(m_original.Position); }
        }

        public Vector3 Rotation
        {
            get { return m_funcRotationMapper(m_original.Rotation); }
        }

        public Vector3 Scaling
        {
            get { return m_funcScalingMapper(m_original.Scaling); }
        }

        public AnimationHandler AnimationHandler
        {
            get
            {
                if (m_funcGetAnimationHandler != null) { return m_funcGetAnimationHandler(m_original.AnimationHandler); }
                else { return m_original.AnimationHandler; }
            }
        }

        public Color4 DisplayColor
        {
            get
            {
                if (m_funcGetDisplayColor != null) { return m_funcGetDisplayColor(m_original.DisplayColor); }
                else { return m_original.DisplayColor; }
            }
        }

        float IEngineOpacityProvider.Opacity
        {
            get
            {
                return m_opacityObject?.Opacity ?? 1f;
            }
        }
    }
}