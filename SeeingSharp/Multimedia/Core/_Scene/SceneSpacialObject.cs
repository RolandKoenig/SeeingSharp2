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

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using Drawing3D;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public abstract class SceneSpacialObject :
        SceneObject,
        IAnimatableObjectEulerRotation, IAnimatableObjectPosition, IAnimatableObjectQuaternion, IAnimatableObjectScaling,
        IAnimatableObjectOpacity, IAnimatableObjectAccentuation
    {
        #region Resource keys
        private NamedOrGenericKey KEY_SCENE_RENDER_PARAMETERS = GraphicsCore.GetNextGenericResourceKey();
        #endregion Resource keys

        #region Resources for rendering

        #endregion Resources for rendering

        #region Spacial parameters
        private SpacialTransformationType m_transformationType;
        private Vector3 m_position;
        private Vector3 m_rotation;
        private Quaternion m_rotationQuaternion;
        private Vector3 m_rotationForward;
        private Vector3 m_rotationUp;
        private Vector3 m_scaling;
        private Matrix m_customTransform;
        private SceneSpacialObject m_transformSourceObject;
        private bool m_transformParamsChanged;
        private bool m_forceTransformUpdateOnChilds;
        #endregion Spacial parameters

        #region Rendering parameters
        private Color4 m_color;
        private float m_accentuationFactor;
        private float m_opacity;
        private float m_borderPart;
        private float m_borderMultiplyer;
        #endregion Rendering parameters

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneSpacialObject"/> class.
        /// </summary>
        public SceneSpacialObject()
        {
            RenderParameters = new IndexBasedDynamicCollection<ObjectRenderParameters>();

            m_rotationUp = Vector3.UnitY;
            m_rotationForward = Vector3.UnitZ;

            m_opacity = 1f;
            m_color = Color4.White;
            m_accentuationFactor = 0f;
            m_borderPart = 0.01f;
            m_borderMultiplyer = 0f;

            m_transformationType = SpacialTransformationType.ScalingTranslationEulerAngles;
            m_position = Vector3.Zero;
            m_rotation = Vector3.Zero;
            m_scaling = new Vector3(1f, 1f, 1f);
            Transform = Matrix.Identity;
            m_rotationQuaternion = Quaternion.Identity;
            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Enables a shader generated border.
        /// </summary>
        public void EnableShaderGeneratedBorder(float borderThicknes = 1f)
        {
            BorderMultiplyer = 50f;
            BorderPart = 0.01f * borderThicknes;
        }

        /// <summary>
        /// Disables shader generated border.
        /// </summary>
        public void DisableShaderGeneratedBorder()
        {
            BorderMultiplyer = 0f;
            BorderPart = 0f;
        }

        /// <summary>
        /// Tries to get the bounding box for the given render-loop.
        /// Returns BoundingBox.Empty if it is not available.
        /// </summary>
        /// <param name="renderLoop">The RenderLoop object.</param>
        public BoundingBox TryGetBoundingBox(RenderLoop renderLoop)
        {
            return TryGetBoundingBox(renderLoop.ViewInformation);
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
            var look = Look;

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
            var up = Up;

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
            var up = Up;

            m_position.Y = m_position.Y + up.Y * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Straves the object.
        /// </summary>
        public void Strave(float points)
        {
            var right = Right;

            m_position.X = m_position.X + right.X * points;
            m_position.Y = m_position.Y + right.Y * points;
            m_position.Z = m_position.Z + right.Z * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Streaves the object.
        /// </summary>
        public void StraveAtPlane(float points)
        {
            var right = Right;

            m_position.X = m_position.X + right.X * points;
            m_position.Z = m_position.Z + right.Z * points;

            m_transformParamsChanged = true;
        }

        /// <summary>
        /// Gets the rotation matrix for this object.
        /// </summary>
        public Matrix GetRotationMatrix()
        {
            switch (m_transformationType)
            {
                case SpacialTransformationType.ScalingTranslationEulerAngles:
                case SpacialTransformationType.TranslationEulerAngles:
                    return Matrix.RotationYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z);

                case SpacialTransformationType.ScalingTranslationQuaternion:
                case SpacialTransformationType.TranslationQuaternion:
                    return Matrix.RotationQuaternion(m_rotationQuaternion);

                case SpacialTransformationType.ScalingTranslationDirection:
                case SpacialTransformationType.TranslationDirection:
                    return MatrixEx.RotationDirection(m_rotationUp, m_rotationForward);

                case SpacialTransformationType.Translation:
                case SpacialTransformationType.None:
                case SpacialTransformationType.CustomTransform:
                    return Matrix.Identity;

                case SpacialTransformationType.TakeFromOtherObject:
                    if (m_transformSourceObject != null) { return m_transformSourceObject.GetRotationMatrix(); }
                    else { return Matrix.Identity; }

                default:
                    throw new SeeingSharpGraphicsException("Unknown transformation type: " + m_transformationType);
            }
        }

        /// <summary>
        /// Gets the scaling matrix for this object.
        /// </summary>
        public Matrix GetScalingMatrix()
        {
            return Matrix.Scaling(m_scaling);
        }

        /// <summary>
        /// Gets the translation matrix for this object.
        /// </summary>
        public Matrix GetTranslationMatrix()
        {
            return Matrix.Translation(m_position);
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected override void UpdateInternal(SceneRelatedUpdateState updateState)
        {
            // Calculates local transform matrix (transforms local space to world space)
            var doRecreateShaderParameters = false;
            TransormationChanged =
                m_transformParamsChanged || updateState.ForceTransformUpdatesOnChilds;

            // Update local transform matrix if transform values have changed
            if (m_transformParamsChanged || updateState.ForceTransformUpdatesOnChilds)
            {
                m_transformParamsChanged = false;
                m_forceTransformUpdateOnChilds = HasChilds;
                doRecreateShaderParameters = true;

                // Calculate new transformation matrix
                switch (m_transformationType)
                {
                    case SpacialTransformationType.ScalingTranslationEulerAngles:
                        Transform =
                            Matrix.Scaling(m_scaling) *
                            Matrix.RotationYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationQuaternion:
                        Transform =
                            Matrix.Scaling(m_scaling) *
                            Matrix.RotationQuaternion(m_rotationQuaternion) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslationDirection:
                        Transform =
                            Matrix.Scaling(m_scaling) *
                            MatrixEx.RotationDirection(m_rotationUp, m_rotationForward) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.ScalingTranslation:
                        Transform =
                            Matrix.Scaling(m_scaling) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationEulerAngles:
                        Transform =
                            Matrix.RotationYawPitchRoll(m_rotation.Y, m_rotation.X, m_rotation.Z) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationQuaternion:
                        Transform =
                            Matrix.RotationQuaternion(m_rotationQuaternion) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TranslationDirection:
                        Transform =
                            MatrixEx.RotationDirection(m_rotationUp, m_rotationForward) *
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.Translation:
                        Transform =
                            Matrix.Translation(m_position) *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.CustomTransform:
                        Transform =
                            m_customTransform *
                            updateState.World.Top;
                        break;

                    case SpacialTransformationType.TakeFromOtherObject:
                        if (m_transformSourceObject != null) { Transform = m_transformSourceObject.Transform; }
                        else { Transform = updateState.World.Top; }
                        break;

                    case SpacialTransformationType.None:
                        Transform = updateState.World.Top;
                        break;
                }
            }

            // Trigger recreation of shader parameters
            if (doRecreateShaderParameters)
            {
                TriggerRecreateOfParameters();
            }
        }

        /// <summary>
        /// Updates all children of this object. Override this to change default behavior.
        /// </summary>
        /// <param name="updateState">The current update state.</param>
        /// <param name="children">The full list of children that should be updated.</param>
        protected override void UpdateChildrenInternal(SceneRelatedUpdateState updateState, List<SceneObject> children)
        {
            var prevForceState = updateState.ForceTransformUpdatesOnChilds;
            updateState.ForceTransformUpdatesOnChilds = prevForceState || m_forceTransformUpdateOnChilds;
            m_forceTransformUpdateOnChilds = false;

            try
            {
                var childCount = children.Count;

                for (var loop = 0; loop < childCount; loop++)
                {
                    // Forward current transform matrix to child objects
                    var currentWorld = updateState.World;
                    currentWorld.Push(Transform);

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
                updateState.ForceTransformUpdatesOnChilds = prevForceState;
            }
        }

        /// <summary>
        /// Triggers recreation of render parameters.
        /// </summary>
        private void TriggerRecreateOfParameters()
        {
            // Notify, that all render parameters need a refresh on next render
            foreach (var actRenderParameters in RenderParameters)
            {
                actRenderParameters.NeedsRefresh = true;
            }
        }

        /// <summary>
        /// Unloads all resources
        /// </summary>
        public override void UnloadResources()
        {
            base.UnloadResources();

            // Mark all local resources for unloading
            foreach (var actRenderParameters in RenderParameters)
            {
                actRenderParameters.MarkForUnloading();
            }
            RenderParameters.Clear();
        }

        /// <summary>
        /// Applies all current render parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void UpdateAndApplyRenderParameters(RenderState renderState)
        {
            // Get or create RenderParamters object on object level
            var renderParameters = RenderParameters[renderState.DeviceIndex];

            if (renderParameters == null)
            {
                renderParameters = renderState.CurrentResources.GetResourceAndEnsureLoaded(
                    KEY_SCENE_RENDER_PARAMETERS,
                    () => new ObjectRenderParameters());
                RenderParameters.AddObject(renderParameters, renderState.DeviceIndex);
            }

            if (renderParameters.NeedsRefresh)
            {
                // Create constant buffer structure
                var cbPerObject = new CBPerObject
                {
                    AccentuationFactor = m_accentuationFactor,
                    Color = m_color.ToVector4(),
                    Opacity = m_opacity,
                    World = Matrix.Transpose(Transform),
                    BorderPart = m_borderPart,
                    BorderMultiplyer = m_borderMultiplyer,
                    ObjectScaling = m_scaling
                };

                // Vector3.TransformCoordinate(m_scaling, Matrix.RotationY(-m_rotation.Y));

                // Update constant buffer
                renderParameters.UpdateValues(renderState, cbPerObject);

                renderParameters.NeedsRefresh = false;
            }

            renderParameters.Apply(renderState);
        }

        /// <summary>
        /// Called when opacity has changed.
        /// </summary>
        protected virtual void OnOpacityChanged()
        {
        }

        /// <summary>
        /// Gets all local render parameters.
        /// </summary>
        internal IndexBasedDynamicCollection<ObjectRenderParameters> RenderParameters { get; }

        /// <summary>
        /// Gets current AnimationHandler object.
        /// </summary>
        public override AnimationHandler AnimationHandler => base.AnimationHandler;

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
        public Matrix Transform { get; private set; }

        /// <summary>
        /// Gets or sets the source of the transformation value when SpacialTransformationType.CustomTransform is set.
        /// </summary>
        public SceneSpacialObject TransformSourceObject
        {
            get => m_transformSourceObject;
            set
            {
                if (m_transformSourceObject == value)
                {
                    return;
                }

                m_transformSourceObject = value;
                m_transformParamsChanged = true;
            }
        }

        /// <summary>
        /// Getr or sets a matrix which is to apply on SpacialTransformationType.CustomTransform.
        /// </summary>
        public Matrix CustomTransform
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
        public Vector3 Up => Vector3.Transform(new Vector3(0f, 1f, 0f), Transform).ToXYZ();

        /// <summary>
        /// Gets the vector that looks into front.
        /// </summary>
        public Vector3 Look => Vector3.Transform(new Vector3(0f, 0f, 1f), Transform).ToXYZ();

        /// <summary>
        /// Gets the vector that looks to the right.
        /// </summary>
        public Vector3 Right => Vector3.Transform(new Vector3(1f, 0f, 0f), Transform).ToXYZ();

        /// <summary>
        /// The color of this object.
        /// </summary>
        public Color4 Color
        {
            get => m_color;
            set
            {
                if (m_color == value)
                {
                    return;
                }

                m_color = value;
                TriggerRecreateOfParameters();
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
                if (m_accentuationFactor == value)
                {
                    return;
                }

                m_accentuationFactor = value;
                TriggerRecreateOfParameters();
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
                if (m_opacity == value)
                {
                    return;
                }

                m_opacity = value;
                TriggerRecreateOfParameters();
                OnOpacityChanged();
            }
        }

        public float BorderPart
        {
            get => m_borderPart;
            set
            {
                if (m_borderPart == value)
                {
                    return;
                }

                m_borderPart = value;
                TriggerRecreateOfParameters();
            }
        }

        public float BorderMultiplyer
        {
            get => m_borderMultiplyer;
            set
            {
                if (m_borderMultiplyer == value)
                {
                    return;
                }

                m_borderMultiplyer = value;
                TriggerRecreateOfParameters();
            }
        }
    }
}