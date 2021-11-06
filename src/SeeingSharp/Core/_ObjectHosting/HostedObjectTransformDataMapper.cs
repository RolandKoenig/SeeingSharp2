using System;
using System.Numerics;
using SeeingSharp.Core.Animations;

namespace SeeingSharp.Core
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