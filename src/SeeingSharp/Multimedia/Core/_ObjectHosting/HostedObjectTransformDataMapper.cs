/*
    SeeingSharp and all applications distributed together with it. 
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
        private IEngineHostedSceneObject _original;

        private Func<Vector3, Vector3> _funcPositionMapper;
        private Func<Vector3, Vector3> _funcRotationMapper;
        private Func<Vector3, Vector3> _funcScalingMapper;
        private Func<AnimationHandler, AnimationHandler> _funcGetAnimationHandler;
        private Func<Color4, Color4> _funcGetDisplayColor;
        private IEngineOpacityProvider _opacityObject;

        public Vector3 Position => _funcPositionMapper(_original.Position);

        public Vector3 Rotation => _funcRotationMapper(_original.Rotation);

        public Vector3 Scaling => _funcScalingMapper(_original.Scaling);

        public AnimationHandler AnimationHandler
        {
            get
            {
                if (_funcGetAnimationHandler != null) { return _funcGetAnimationHandler(_original.AnimationHandler); }
                return _original.AnimationHandler;
            }
        }

        public Color4 DisplayColor
        {
            get
            {
                if (_funcGetDisplayColor != null) { return _funcGetDisplayColor(_original.DisplayColor); }
                return _original.DisplayColor;
            }
        }

        float IEngineOpacityProvider.Opacity => _opacityObject?.Opacity ?? 1f;

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
            _original = originalObject;
            _funcPositionMapper = funcPositionMapper;
            _funcRotationMapper = funcRotationMapper;
            _funcScalingMapper = funcScalingMapper;
            _funcGetAnimationHandler = null;
            _funcGetDisplayColor = null;
            _opacityObject = originalObject as IEngineOpacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Vector3, Vector3> funcPositionMapper,
            Func<Vector3, Vector3> funcRotationMapper,
            Func<Vector3, Vector3> funcScalingMapper,
            Func<Color4, Color4> funcColorMapper)
        {
            _original = originalObject;
            _funcPositionMapper = funcPositionMapper;
            _funcRotationMapper = funcRotationMapper;
            _funcScalingMapper = funcScalingMapper;
            _funcGetDisplayColor = funcColorMapper;
            _opacityObject = originalObject as IEngineOpacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Vector3, Vector3> funcPositionMapper,
            Func<Vector3, Vector3> funcRotationMapper,
            Func<Vector3, Vector3> funcScalingMapper,
            Func<Color4, Color4> funcColorMapper,
            IEngineOpacityProvider opacityProvider)
        {
            _original = originalObject;
            _funcPositionMapper = funcPositionMapper;
            _funcRotationMapper = funcRotationMapper;
            _funcScalingMapper = funcScalingMapper;
            _funcGetDisplayColor = funcColorMapper;
            _opacityObject = opacityProvider;
        }

        public HostedObjectTransformDataMapper(
            IEngineHostedSceneObject originalObject,
            Func<Color4, Color4> funcColorMapper)
            : this(
                originalObject,
                actPos => actPos,
                actRot => actRot,
                actScale => actScale,
                funcColorMapper)
        {
        }
    }
}