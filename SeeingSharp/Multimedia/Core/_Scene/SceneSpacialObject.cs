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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public abstract class SceneSpacialObject :
        SceneObject,
        IAnimatableObjectEulerRotation, IAnimatableObjectPosition, IAnimatableObjectQuaternion, IAnimatableObjectScaling,
        IAnimatableObjectOpacity, IAnimatableObjectAccentuation
    {
        // Resource keys
        private NamedOrGenericKey KEY_SCENE_RENDER_PARAMETERS = GraphicsCore.GetNextGenericResourceKey();

        // Spacial parameters
        private SpacialTransformationType m_transformationType;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private Quaternion m_rotationQuaternion;
        private Vector3 m_rotationForward;
        private Vector3 m_rotationUp;
        private Vector3 m_scaling;
        private Matrix4x4 m_transform;
        private Matrix4x4 m_customTransform;
        private SceneSpacialObject m_transformSourceObject;
        private bool m_transformParamsChanged;
        private bool m_forceTransformUpdateOnChildren;

        // Rendering parameters
        private Color4 m_color;
        private float m_accentuationFactor;
        private float m_opacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneSpacialObject"/> class.
        /// </summary>
        protected SceneSpacialObject()
        {
            this.RenderParameters = new IndexBasedDynamicCollection<ObjectRenderParameters>();

            m_rotationUp = Vector3.UnitY;
            m_rotationForward = Vector3.UnitZ;

            m_opacity = 1f;
            m_color = Color4.White;
            m_accentuationFactor = 0f;

            m_transformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
            m_position = Vector3.Zero;
            m_rotation = Vector3.Zero;
            m_scaling = new Vector3(1f, 1f, 1f);
            m_transform = Matrix4x4.Identity;
            m_rotationQuaternion = Quaternion.Identity;
            m_transformParamsChanged = true;
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

            m_position.X += dist * look.X;
            m_position.Y += dist * look.Y;
            m_position.Z += dist * look.Z;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object position.
        /// </summary>
        /// <param name="x">moving in x direction.</param>
        /// <param name="z">moving in z direction.</param>
        public void Move(float x, float z)
        {
            m_position.X += x;
            m_position.Z += z;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object up and down.
        /// </summary>
        public void UpDown(float points)
        {
            var up = this.Up;

            m_position.X = m_position.X + up.X * points;
            m_position.Y = m_position.Y + up.Y * points;
            m_position.Z = m_position.Z + up.Z * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Moves the object up and down.
        /// </summary>
        public void UpDownWithoutMoving(float points)
        {
            var up = this.Up;

            m_position.Y = m_position.Y + up.Y * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Straves the object.
        /// </summary>
        public void Strave(float points)
        {
            var right = this.Right;

            m_position.X = m_position.X + right.X * points;
            m_position.Y = m_position.Y + right.Y * points;
            m_position.Z = m_position.Z + right.Z * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Straves the object.
        /// </summary>
        public void StraveAtPlane(float points)
        {
            var right = this.Right;

            m_position.X = m_position.X + right.X * points;
            m_position.Z = m_position.Z + right.Z * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Gets the rotation matrix for this object.
        /// </summary>
        public Matrix4x4 GetRotationMatrix()
        {
            switch (m_transformationType)
            {
                case SpacialTransformationType.ScalingTranslationEulerAngles:
                case SpacialTransformationType.TranslationEulerAngles:
                    return Matrix4x4.CreateFromYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z);

                case SpacialTransformationType.ScalingTranslationQuaternion:
                case SpacialTransformationType.TranslationQuaternion:
                    return Matrix4x4.CreateFromQuaternion(m_rotationQuaternion);

                case SpacialTransformationType.ScalingTranslationDirection:
                case SpacialTransformationType.TranslationDirection:
                    return Matrix4x4Ex.CreateRotationDirection(m_rotationUp, m_rotationForward);

                case SpacialTransformationType.Translation:
                case SpacialTransformationType.None:
                case SpacialTransformationType.CustomTransform:
                    return Matrix4x4.Identity;

                case SpacialTransformationType.TakeFromOtherObject:
                    if (m_transformSourceObject != null) { return m_transformSourceObject.GetRotationMatrix(); }
                    else { return Matrix4x4.Identity; }

                default:
                    throw new SeeingSharpGraphicsException("Unknown transformation type: " + m_transformationType);
            }
        }

        /// <summary>
        /// Gets the scaling matrix for this object.
        /// </summary>
        public Matrix4x4 GetScalingMatrix()
        {
            return Matrix4x4.CreateScale(m_scaling);
        }

        /// <summary>
        /// Gets the translation matrix for this object.
        /// </summary>
        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(m_position);
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
            TransformationChanged =
                m_transformParamsChanged || updateState.ForceTransformUpdatesOnChildren;

            // Update local transform matrix if transform values have changed
            if (m_transformParamsChanged || updateState.ForceTransformUpdatesOnChildren)
            {
                m_transformParamsChanged = false;
                m_forceTransformUpdateOnChildren = this.HasChildren;
                doRecreateShaderParameters = true;

                // Calculate new transformation matrix
                switch (m_transformationType)
                {
                    case SpacialTransformationType.ScalingTranslationEulerAngles:
                        m_transform =
                            Matrix4x4.CreateScale(m_scaling) *
                            Matrix4x4.CreateFromYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationQuaternion:
                        m_transform =
                            Matrix4x4.CreateScale(m_scaling) *
                            Matrix4x4.CreateFromQuaternion(m_rotationQuaternion) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationDirection:
                        m_transform =
                            Matrix4x4.CreateScale(m_scaling) *
                            Matrix4x4Ex.CreateRotationDirection(m_rotationUp, m_rotationForward) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslation:
                        m_transform =
                            Matrix4x4.CreateScale(m_scaling) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationEulerAngles:
                        m_transform =
                            Matrix4x4.CreateFromYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationQuaternion:
                        m_transform =
                            Matrix4x4.CreateFromQuaternion(m_rotationQuaternion) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationDirection:
                        m_transform =
                            Matrix4x4Ex.CreateRotationDirection(m_rotationUp, m_rotationForward) *
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.Translation:
                        m_transform =
                            Matrix4x4.CreateTranslation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.CustomTransform:
                        m_transform =
                            m_customTransform *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TakeFromOtherObject:
                        if (m_transformSourceObject != null) { m_transform = m_transformSourceObject.m_transform; }
                        else { m_transform = updateState.World.Top; }
                        break;

                    case SpacialTransformationType.None:
                        m_transform = updateState.World.Top;
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
        protected override void UpdateChildrenInternal(SceneRelatedUpdateState updateState, List<SceneObject> children)
        {
            var prevForceState = updateState.ForceTransformUpdatesOnChildren;
            updateState.ForceTransformUpdatesOnChildren = prevForceState || m_forceTransformUpdateOnChildren;
            m_forceTransformUpdateOnChildren = false;
            try
            {
                var childCount = children.Count;

                for (var loop = 0; loop < childCount; loop++)
                {
                    // Forward current transform matrix to child objects
                    var currentWorld = updateState.World;
                    currentWorld.Push(ref m_transform);

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
                    KEY_SCENE_RENDER_PARAMETERS,
                    () => new ObjectRenderParameters());
                this.RenderParameters.AddObject(renderParameters, renderState.DeviceIndex);
            }

            if (renderParameters.NeedsRefresh)
            {
                // Create constant buffer structure
                var cbPerObject = new CBPerObject
                {
                    AccentuationFactor = m_accentuationFactor,
                    Color = m_color.ToVector4(),
                    Opacity = m_opacity,
                    World = Matrix4x4.Transpose(m_transform),
                    ObjectScaling = m_scaling,
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

        /// <summary>
        /// The accentuation factor.
        /// </summary>
        public float AccentuationFactor
        {
            get => m_accentuationFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(m_accentuationFactor, value))
                {
                    m_accentuationFactor = value;
                    this.TriggerRecreateOfParameters();
                }
            }
        }

        /// <summary>
        /// Gets or sets current rotation.
        /// </summary>
        public Vector3 RotationEuler
        {
            get => m_rotation;
            set
            {
                m_rotation = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the opacity of this object.
        /// </summary>
        public float Opacity
        {
            get => m_opacity;
            set
            {
                if (!EngineMath.EqualsWithTolerance(m_opacity, value))
                {
                    m_opacity = value;

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
            get => m_position;
            set
            {
                m_position = value;
                m_transformParamsChanged = true;
            }
        }

        public Quaternion RotationQuaternion
        {
            get => m_rotationQuaternion;
            set
            {
                m_rotationQuaternion = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets current scaling.
        /// </summary>
        public Vector3 Scaling
        {
            get => m_scaling;
            set
            {
                m_scaling = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the x coordinate of this object.
        /// </summary>
        public float XPos
        {
            get => m_position.X;
            set
            {
                m_position.X = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the y coordinate of this object.
        /// </summary>
        public float YPos
        {
            get => m_position.Y;
            set
            {
                m_position.Y = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the z coordinate of this object.
        /// </summary>
        public float ZPos
        {
            get => m_position.Z;
            set
            {
                m_position.Z = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the used rotation type.
        /// </summary>
        public SpacialTransformationType TransformationType
        {
            get => m_transformationType;
            set
            {
                m_transformationType = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the vector that points up (default: Y-Axis).
        /// This is relevant when rotation is calculated by direction.
        /// </summary>
        public Vector3 RotationUp
        {
            get => m_rotationUp;
            set
            {
                m_rotationUp = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the vector that points forward (default: Z-Axis).
        /// This is relevant when rotation is calculated by direction.
        /// </summary>
        public Vector3 RotationForward
        {
            get => m_rotationForward;
            set
            {
                m_rotationForward = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets the current maximum scale factor.
        /// </summary>
        public float MaxScaleFactor
        {
            get
            {
                switch (m_transformationType)
                {
                    case SpacialTransformationType.ScalingTranslation:
                    case SpacialTransformationType.ScalingTranslationEulerAngles:
                    case SpacialTransformationType.ScalingTranslationQuaternion:
                        return Math.Max(m_scaling.X, Math.Max(m_scaling.Y, m_scaling.Z));

                    default:
                        return 1f;
                }
            }
        }

        /// <summary>
        /// Gets a matrix that transforms local space to world space.
        /// </summary>
        public Matrix4x4 Transform => m_transform;

        /// <summary>
        /// Gets or sets the source of the transformation value when SpacialTransformationType.CustomTransform is set.
        /// </summary>
        public SceneSpacialObject TransformSourceObject
        {
            get => m_transformSourceObject;
            set
            {
                if (m_transformSourceObject != value)
                {
                    m_transformSourceObject = value;
                    m_transformParamsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a matrix which is to apply on SpacialTransformationType.CustomTransform.
        /// </summary>
        public Matrix4x4 CustomTransform
        {
            get => m_customTransform;
            set
            {
                m_customTransform = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Gets the vector that looks up.
        /// </summary>
        public Vector3 Up => Vector3.Transform(new Vector3(0f, 1f, 0f), m_transform);

        /// <summary>
        /// Gets the vector that looks into front.
        /// </summary>
        public Vector3 Look => Vector3.Transform(new Vector3(0f, 0f, 1f), m_transform);

        /// <summary>
        /// Gets the vector that looks to the right.
        /// </summary>
        public Vector3 Right => Vector3.Transform(new Vector3(1f, 0f, 0f), m_transform);

        /// <summary>
        /// The color of this object.
        /// </summary>
        public Color4 Color
        {
            get => m_color;
            set
            {
                if (m_color != value)
                {
                    m_color = value;
                    this.TriggerRecreateOfParameters();
                }
            }
        }

        /// <summary>
        /// Gets all local render parameters.
        /// </summary>
        internal IndexBasedDynamicCollection<ObjectRenderParameters> RenderParameters { get; }
    }
}