using System;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public abstract class SceneSpacialObject :
        SceneObject,
        IAnimatableObjectEulerRotation, IAnimatableObjectPosition, IAnimatableObjectQuaternion, IAnimatableObjectScaling,
        IAnimatableObjectOpacity, IAnimatableObjectAccentuation
    {
        // Resource keys
        private NamedOrGenericKey _keySceneRenderParameters = GraphicsCore.GetNextGenericResourceKey();

        // Spacial parameters
        private SpacialTransformationType _transformationType;
        private Vector3 _position;
        private Vector3 _rotation;
        private Quaternion _rotationQuaternion;
        private Vector3 _rotationForward;
        private Vector3 _rotationUp;
        private Vector3 _scaling;
        private Matrix4x4 _transform;
        private Matrix4x4 _customTransform;
        private SceneSpacialObject _transformSourceObject;
        private bool _transformParamsChanged;
        private bool _forceTransformUpdateOnChildren;

        // Parameters for object hosting
        private IEngineHostedSceneObject _hostedObject;
        private IEngineOpacityProvider _hostedObjectOpacity;
        private ObjectHostMode _hostedObjectMode;
        private bool _hostedObjectChanged;

        // Rendering parameters
        private Color4 _color;
        private float _accentuationFactor;
        private float _opacity;

        /// <summary>
        /// The accentuation factor.
        /// </summary>
        public float AccentuationFactor
        {
            get => _accentuationFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_accentuationFactor, value))
                {
                    _accentuationFactor = value;
                    this.TriggerRecreateOfParameters();
                }
            }
        }

        /// <summary>
        /// Gets or sets current rotation.
        /// </summary>
        public Vector3 RotationEuler
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the opacity of this object.
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_opacity, value))
                {
                    _opacity = value;

                    this.TriggerRecreateOfParameters();
                    this.OnOpacityChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets current position.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _transformParamsChanged = true;
            }
        }

        public Quaternion RotationQuaternion
        {
            get => _rotationQuaternion;
            set
            {
                _rotationQuaternion = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets current scaling.
        /// </summary>
        public Vector3 Scaling
        {
            get => _scaling;
            set
            {
                _scaling = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the x coordinate of this object.
        /// </summary>
        public float XPos
        {
            get => _position.X;
            set
            {
                _position.X = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the y coordinate of this object.
        /// </summary>
        public float YPos
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the z coordinate of this object.
        /// </summary>
        public float ZPos
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the used rotation type.
        /// </summary>
        public SpacialTransformationType TransformationType
        {
            get => _transformationType;
            set
            {
                _transformationType = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the vector that points up (default: Y-Axis).
        /// This is relevant when rotation is calculated by direction.
        /// </summary>
        public Vector3 RotationUp
        {
            get => _rotationUp;
            set
            {
                _rotationUp = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the vector that points forward (default: Z-Axis).
        /// This is relevant when rotation is calculated by direction.
        /// </summary>
        public Vector3 RotationForward
        {
            get => _rotationForward;
            set
            {
                _rotationForward = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets the current maximum scale factor.
        /// </summary>
        public float MaxScaleFactor
        {
            get
            {
                switch (_transformationType)
                {
                    case SpacialTransformationType.ScalingTranslation:
                    case SpacialTransformationType.ScalingTranslationEulerAngles:
                    case SpacialTransformationType.ScalingTranslationQuaternion:
                        return Math.Max(_scaling.X, Math.Max(_scaling.Y, _scaling.Z));

                    default:
                        return 1f;
                }
            }
        }

        /// <summary>
        /// Gets a matrix that transforms local space to world space.
        /// </summary>
        public Matrix4x4 Transform => _transform;

        /// <summary>
        /// Gets or sets the source of the transformation value when SpacialTransformationType.CustomTransform is set.
        /// </summary>
        public SceneSpacialObject TransformSourceObject
        {
            get => _transformSourceObject;
            set
            {
                if (_transformSourceObject != value)
                {
                    _transformSourceObject = value;
                    _transformParamsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a matrix which is to apply on SpacialTransformationType.CustomTransform.
        /// </summary>
        public Matrix4x4 CustomTransform
        {
            get => _customTransform;
            set
            {
                _customTransform = value;
                _transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets the vector that looks up.
        /// </summary>
        public Vector3 Up => Vector3.Transform(new Vector3(0f, 1f, 0f), _transform);

        /// <summary>
        /// Gets the vector that looks into front.
        /// </summary>
        public Vector3 Look => Vector3.Transform(new Vector3(0f, 0f, 1f), _transform);

        /// <summary>
        /// Gets the vector that looks to the right.
        /// </summary>
        public Vector3 Right => Vector3.Transform(new Vector3(1f, 0f, 0f), _transform);

        /// <summary>
        /// The color of this object.
        /// </summary>
        public Color4 Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    this.TriggerRecreateOfParameters();
                }
            }
        }

        /// <summary>
        /// Gets all local render parameters.
        /// </summary>
        internal IndexBasedDynamicCollection<ObjectRenderParameters> RenderParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneSpacialObject"/> class.
        /// </summary>
        protected SceneSpacialObject()
        {
            this.RenderParameters = new IndexBasedDynamicCollection<ObjectRenderParameters>();

            _rotationUp = Vector3.UnitY;
            _rotationForward = Vector3.UnitZ;

            _opacity = 1f;
            _color = Color4.White;
            _accentuationFactor = 0f;

            _transformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scaling = new Vector3(1f, 1f, 1f);
            _transform = Matrix4x4.Identity;
            _rotationQuaternion = Quaternion.Identity;
            _transformParamsChanged = true;
        }

        /// <summary>
        /// Sets an hosted object (or null, to reset previous).
        /// </summary>
        /// <param name="hostedObject">The hosted object to be set.</param>
        public void SetHostedObject(IEngineHostedSceneObject hostedObject)
        {
            this.SetHostedObject(hostedObject, ObjectHostMode.Default);
        }

        /// <summary>
        /// Sets an hosted object (or null, to reset previous).
        /// </summary>
        /// <param name="hostedObject">The hosted object to be set.</param>
        /// <param name="hostMode">Describes the specific host mode.</param>
        public void SetHostedObject(IEngineHostedSceneObject hostedObject, ObjectHostMode hostMode)
        {
            if (hostedObject != null)
            {
                _hostedObject = hostedObject;
                _hostedObjectOpacity = hostedObject as IEngineOpacityProvider;
                _hostedObjectChanged = true;
            }
            else
            {
                _hostedObject = null;
                _hostedObjectOpacity = null;

                // Set default parameters
                _transformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
                _position = Vector3.Zero;
                _rotation = Vector3.Zero;
                _scaling = new Vector3(1f, 1f, 1f);
                _transform = Matrix4x4.Identity;
                _rotationQuaternion = Quaternion.Identity;
                _transformParamsChanged = true;

                _color = Color4.White;
            }

            _hostedObjectMode = hostMode;
        }

        /// <summary>
        /// Resets object hosting functionality.
        /// </summary>
        public void ResetHostedObject()
        {
            this.SetHostedObject(null, ObjectHostMode.Default);
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="renderLoop">The RenderLoop object.</param>
        public BoundingBox TryGetBoundingBox(RenderLoop renderLoop)
        {
            return this.TryGetBoundingBox(renderLoop.ViewInformation);
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingBox.</param>
        public abstract BoundingBox TryGetBoundingBox(ViewInformation viewInfo);

        /// <summary>
        /// Tries to get the bounding sphere for the given render-loop.
        /// Returns BoundingSphere.Empty, if it is not available.
        /// </summary>
        /// <param name="viewInfo">The ViewInformation for which to get the BoundingSphere.</param>
        public abstract BoundingSphere TryGetBoundingSphere(ViewInformation viewInfo);

        /// <summary>
        /// Zooms the camera into or out along the actual target-vector.
        /// </summary>
        /// <param name="dist">The Distance you want to zoom.</param>
        public void MoveForward(float dist)
        {
            var look = this.Look;

            _position.X += dist * look.X;
            _position.Y += dist * look.Y;
            _position.Z += dist * look.Z;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object position.
        /// </summary>
        /// <param name="x">moving in x direction.</param>
        /// <param name="z">moving in z direction.</param>
        public void Move(float x, float z)
        {
            _position.X += x;
            _position.Z += z;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object up and down.
        /// </summary>
        public void UpDown(float points)
        {
            var up = this.Up;

            _position.X = _position.X + up.X * points;
            _position.Y = _position.Y + up.Y * points;
            _position.Z = _position.Z + up.Z * points;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object up and down.
        /// </summary>
        public void UpDownWithoutMoving(float points)
        {
            var up = this.Up;

            _position.Y = _position.Y + up.Y * points;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Straves the object.
        /// </summary>
        public void Strave(float points)
        {
            var right = this.Right;

            _position.X = _position.X + right.X * points;
            _position.Y = _position.Y + right.Y * points;
            _position.Z = _position.Z + right.Z * points;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Straves the object.
        /// </summary>
        public void StraveAtPlane(float points)
        {
            var right = this.Right;

            _position.X = _position.X + right.X * points;
            _position.Z = _position.Z + right.Z * points;

            _transformParamsChanged = true;
        }

        /// <summary>
        /// Gets the rotation matrix for this object.
        /// </summary>
        public Matrix4x4 GetRotationMatrix()
        {
            switch (_transformationType)
            {
                case SpacialTransformationType.ScalingTranslationEulerAngles:
                case SpacialTransformationType.TranslationEulerAngles:
                    return Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);

                case SpacialTransformationType.ScalingTranslationQuaternion:
                case SpacialTransformationType.TranslationQuaternion:
                    return Matrix4x4.CreateFromQuaternion(_rotationQuaternion);

                case SpacialTransformationType.ScalingTranslationDirection:
                case SpacialTransformationType.TranslationDirection:
                    return Matrix4x4Ex.CreateRotationDirection(_rotationUp, _rotationForward);

                case SpacialTransformationType.Translation:
                case SpacialTransformationType.None:
                case SpacialTransformationType.CustomTransform:
                    return Matrix4x4.Identity;

                case SpacialTransformationType.TakeFromOtherObject:
                    if (_transformSourceObject != null) { return _transformSourceObject.GetRotationMatrix(); }
                    else { return Matrix4x4.Identity; }

                default:
                    throw new SeeingSharpGraphicsException("Unknown transformation type: " + _transformationType);
            }
        }

        /// <summary>
        /// Gets the scaling matrix for this object.
        /// </summary>
        public Matrix4x4 GetScalingMatrix()
        {
            return Matrix4x4.CreateScale(_scaling);
        }

        /// <summary>
        /// Gets the translation matrix for this object.
        /// </summary>
        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(_position);
        }

        /// <summary>
        /// Triggers update of position/rotation/scaling data.
        /// </summary>
        /// <param name="forceRecreateOfParameters">Force upload of all object data to the graphics hardware?</param>
        public void NotifyStaticObjectChanged(bool forceRecreateOfParameters = false)
        {
            _hostedObjectChanged = true;
            if (forceRecreateOfParameters)
            {
                this.TriggerRecreateOfParameters();
            }
        }

        /// <summary>
        /// Unloads all resources
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            // Mark all local resources for unloading
            foreach (var actRenderParameters in this.RenderParameters)
            {
                actRenderParameters.MarkForUnloading();
            }
            this.RenderParameters.Clear();
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
            // Calculates local transform matrix (transforms local space to world space)
            var doRecreateShaderParameters = false;
            
            if (_hostedObject != null &&
                (!this.IsStatic || _hostedObjectChanged))
            {
                if (_hostedObjectChanged) { doRecreateShaderParameters = true; }
                _hostedObjectChanged = false;

                var hostedPosition = _hostedObject.Position;
                var hostedRotation = _hostedObject.Rotation;
                var hostedScaling = _hostedObject.Scaling;
                switch (_hostedObjectMode)
                {
                    // Gather transformation values
                    case ObjectHostMode.Default:
                        _transformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
                        if (hostedPosition != _position ||
                            hostedRotation != _rotation ||
                            hostedScaling != _scaling)
                        {
                            _position = hostedPosition;
                            _scaling = hostedScaling;
                            _rotation = hostedRotation;
                            _transformParamsChanged = true;
                            doRecreateShaderParameters = true;
                        }
                        break;

                    // Discard rotation data from hosted object
                    case ObjectHostMode.IgnoreRotation:
                        _rotation = Vector3.Zero;
                        _rotationQuaternion = Quaternion.Identity;
                        _transformationType = SpacialTransformationType.ScalingTranslation;
                        if (hostedPosition != _position ||
                            hostedScaling != _scaling)
                        {
                            _position = hostedPosition;
                            _scaling = hostedScaling;
                            _transformParamsChanged = true;
                            doRecreateShaderParameters = true;
                        }
                        break;

                    // Discard size data from hosted object
                    case ObjectHostMode.IgnoreScaling:
                        //_scaling = Vector3.One;
                        _transformationType = SpacialTransformationType.TranslationEulerAngles;
                        if (hostedPosition != _position ||
                            hostedRotation != _rotation)
                        {
                            _position = hostedPosition;
                            _rotation = hostedRotation;
                            _transformParamsChanged = true;
                            doRecreateShaderParameters = true;
                        }
                        break;

                    // Discard size and rotation data from hosted object
                    case ObjectHostMode.IgnoreRotationScaling:
                        _transformationType = SpacialTransformationType.TranslationEulerAngles;
                        if (hostedPosition != _position)
                        {
                            _position = hostedPosition;
                            _transformParamsChanged = true;
                            doRecreateShaderParameters = true;
                        }
                        break;
                }

                // Change opacity value if different
                var localHostedOpacity = _hostedObjectOpacity;
                var hostedOpacity = localHostedOpacity?.Opacity ?? _opacity;
                if (!EngineMath.EqualsWithTolerance(_opacity, hostedOpacity))
                {
                    _opacity = hostedOpacity;
                    doRecreateShaderParameters = true;
                    this.OnOpacityChanged();
                }

                // Gather additional values
                var currentDisplayColor = _hostedObject.DisplayColor;
                if (currentDisplayColor != _color)
                {
                    _color = currentDisplayColor;
                    doRecreateShaderParameters = true;
                }
            }

            // Remember current TransformationChanged flag)
            TransformationChanged =
                _transformParamsChanged || updateState.ForceTransformUpdatesOnChildren;

            // Update local transform matrix if transform values have changed
            if (_transformParamsChanged || 
                updateState.ForceTransformUpdatesOnChildren || 
                _transformationType == SpacialTransformationType.TakeFromOtherObject)  // <-- Special case: We don't know weather transform has changed there
            {
                _transformParamsChanged = false;
                _forceTransformUpdateOnChildren = this.HasChildren;
                doRecreateShaderParameters = true;

                // Calculate new transformation matrix
                switch (_transformationType)
                {
                    case SpacialTransformationType.ScalingTranslationEulerAngles:
                        _transform =
                            Matrix4x4.CreateScale(_scaling) *
                            Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationQuaternion:
                        _transform =
                            Matrix4x4.CreateScale(_scaling) *
                            Matrix4x4.CreateFromQuaternion(_rotationQuaternion) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationDirection:
                        _transform =
                            Matrix4x4.CreateScale(_scaling) *
                            Matrix4x4Ex.CreateRotationDirection(_rotationUp, _rotationForward) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslation:
                        _transform =
                            Matrix4x4.CreateScale(_scaling) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationEulerAngles:
                        _transform =
                            Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationQuaternion:
                        _transform =
                            Matrix4x4.CreateFromQuaternion(_rotationQuaternion) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationDirection:
                        _transform =
                            Matrix4x4Ex.CreateRotationDirection(_rotationUp, _rotationForward) *
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.Translation:
                        _transform =
                            Matrix4x4.CreateTranslation(_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.CustomTransform:
                        _transform =
                            _customTransform *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TakeFromOtherObject:
                        if (_transformSourceObject != null)
                        {
                            ref var takenTransform = ref _transformSourceObject._transform;
                            if (_transform != takenTransform)
                            {
                                _transform = takenTransform;
                            }
                            else
                            {
                                // Reset the flag to initially detected value
                                doRecreateShaderParameters = TransformationChanged;
                            }
                        }
                        else
                        {
                            ref var takenTransform = ref updateState.World.GetTopByRef();
                            if (_transform != takenTransform)
                            {
                                _transform = takenTransform;
                            }
                            else
                            {
                                // Reset the flag to initially detected value
                                doRecreateShaderParameters = TransformationChanged;
                            }
                        }

                        break;

                    case SpacialTransformationType.None:
                        _transform = updateState.World.Top;
                        break;
                }
            }

            // Trigger recreation of shader parameters
            if (doRecreateShaderParameters)
            {
                this.TriggerRecreateOfParameters();
            }
        }

        /// <summary>
        /// Updates all children of this object. Override this to change default behavior.
        /// </summary>
        /// <param name="updateState">The current update state.</param>
        /// <param name="children">The full list of children that should be updated.</param>
        protected override void UpdateChildrenInternal(SceneRelatedUpdateState updateState, UnsafeList<SceneObject> children)
        {
            var prevForceState = updateState.ForceTransformUpdatesOnChildren;
            updateState.ForceTransformUpdatesOnChildren = prevForceState || _forceTransformUpdateOnChildren;
            _forceTransformUpdateOnChildren = false;
            try
            {
                var childCount = children.Count;

                for (var loop = 0; loop < childCount; loop++)
                {
                    // Forward current transform matrix to child objects
                    var currentWorld = updateState.World;
                    currentWorld.Push(ref _transform);

                    try
                    {
                        children[loop].Update(updateState);
                    }
                    finally
                    {
                        currentWorld.Pop();
                    }
                }
            }
            finally
            {
                updateState.ForceTransformUpdatesOnChildren = prevForceState;
            }
        }

        /// <summary>
        /// Called when opacity has changed.
        /// </summary>
        protected virtual void OnOpacityChanged()
        {
        }

        /// <summary>
        /// Applies all current render parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void UpdateAndApplyRenderParameters(RenderState renderState)
        {
            // Get or create RenderParameters object on object level
            var renderParameters = this.RenderParameters[renderState.DeviceIndex];
            if (renderParameters == null)
            {
                renderParameters = renderState.CurrentResources.GetResourceAndEnsureLoaded(
                    _keySceneRenderParameters,
                    () => new ObjectRenderParameters());
                this.RenderParameters.AddObject(renderParameters, renderState.DeviceIndex);
            }

            if (renderParameters.NeedsRefresh)
            {
                // Create constant buffer structure
                var cbPerObject = new CBPerObject
                {
                    AccentuationFactor = _accentuationFactor,
                    Color = _color.ToVector4(),
                    Opacity = _opacity,
                    World = Matrix4x4.Transpose(_transform),
                    ObjectScaling = _scaling
                };

                // Update constant buffer
                renderParameters.UpdateValues(renderState, cbPerObject);

                renderParameters.NeedsRefresh = false;
            }

            renderParameters.Apply(renderState);
        }

        /// <summary>
        /// Triggers recreation of render parameters.
        /// </summary>
        private void TriggerRecreateOfParameters()
        {
            // Notify, that all render parameters need a refresh on next render
            foreach (var actRenderParameters in this.RenderParameters)
            {
                actRenderParameters.NeedsRefresh = true;
            }
        }
    }
}