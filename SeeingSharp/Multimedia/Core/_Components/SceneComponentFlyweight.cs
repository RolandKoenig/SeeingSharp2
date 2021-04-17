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
using System.Collections.Concurrent;
using System.Collections.Generic;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// This is mostly logic of the Scene class but extracted here
    /// following the Flyweight pattern.
    /// </summary>
    internal class SceneComponentFlyweight
    {
        // Main members
        private Scene _owner;
        private ConcurrentQueue<SceneComponentRequest> _componentRequests;
        private List<SceneComponentInfo> _attachedComponents;

        internal int CountAttached => _attachedComponents.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneComponentFlyweight"/> class.
        /// </summary>
        internal SceneComponentFlyweight(Scene owner)
        {
            _owner = owner;
            _componentRequests = new ConcurrentQueue<SceneComponentRequest>();
            _attachedComponents = new List<SceneComponentInfo>();
        }

        /// <summary>
        /// Attaches the given component to this scene.
        /// </summary>
        /// <param name="component">The component to be attached.</param>
        /// <param name="sourceView">The view which attaches the component.</param>
        internal void AttachComponent(SceneComponentBase component, ViewInformation sourceView)
        {
            component.EnsureNotNull(nameof(component));
            if (component.IsViewSpecific)
            {
                sourceView.EnsureNotNull(nameof(sourceView));
            }

            _componentRequests.Enqueue(new SceneComponentRequest
            {
                RequestType = SceneComponentRequestType.Attach,
                Component = component,
                CorrespondingView = component.IsViewSpecific ? sourceView : null
            });
        }

        /// <summary>
        /// Detaches the given component from this scene.
        /// </summary>
        /// <param name="component">The component to be detached.</param>
        /// <param name="sourceView">The view which attached the component initially.</param>
        internal void DetachComponent(SceneComponentBase component, ViewInformation sourceView)
        {
            component.EnsureNotNull(nameof(component));
            if (component.IsViewSpecific)
            {
                sourceView.EnsureNotNull(nameof(sourceView));
            }

            _componentRequests.Enqueue(new SceneComponentRequest
            {
                RequestType = SceneComponentRequestType.Detach,
                Component = component,
                CorrespondingView = component.IsViewSpecific ? sourceView : null
            });
        }

        /// <summary>
        /// Detaches all currently attached components.
        /// </summary>
        /// <param name="sourceView">The view from which we've to detach all components.</param>
        internal void DetachAllComponents(ViewInformation sourceView)
        {
            _componentRequests.Enqueue(new SceneComponentRequest
            {
                RequestType = SceneComponentRequestType.DetachAll,
                CorrespondingView = sourceView
            });
        }

        /// <summary>
        /// Internal method for updating all scene components.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        internal void UpdateSceneComponents(SceneRelatedUpdateState updateState)
        {
            // Update all components
            var attachedComponentsCount = _attachedComponents.Count;

            for (var loop = 0; loop < attachedComponentsCount; loop++)
            {
                _attachedComponents[loop].Component.UpdateInternal(
                    updateState,
                    _attachedComponents[loop].CorrespondingView,
                    _attachedComponents[loop].Context);
            }

            // Attach all components which are coming in
            while (_componentRequests.TryDequeue(out var actRequest))
            {
                SceneComponentInfo actComponent;
                int actComponentIndex;

                switch (actRequest.RequestType)
                {
                    case SceneComponentRequestType.Attach:
                        if (actRequest.Component == null)
                        {
                            continue;
                        }

                        if (this.TryGetAttachedComponent(
                            actRequest.Component, actRequest.CorrespondingView,
                            out actComponent, out actComponentIndex))
                        {
                            // We've already attached this component, so skip this request
                            continue;
                        }

                        // Trigger removing of all components with the same group like the new one
                        //  (new components replace old components with same group name)
                        if (!string.IsNullOrEmpty(actRequest.Component.ComponentGroup))
                        {
                            foreach (var actObsoleteComponent in this.GetExistingComponentsByGroup(
                                actRequest.Component.ComponentGroup,
                                actRequest.Component.IsViewSpecific ? actRequest.CorrespondingView : null))
                            {
                                _componentRequests.Enqueue(new SceneComponentRequest
                                {
                                    RequestType = SceneComponentRequestType.Detach,
                                    Component = actObsoleteComponent.Component,
                                    CorrespondingView = actObsoleteComponent.CorrespondingView
                                });
                            }
                        }

                        var actManipulator = new SceneManipulator(_owner);
                        actManipulator.IsValid = true;

                        try
                        {
                            var newRegisteredComponentInfo = new SceneComponentInfo
                            {
                                Component = actRequest.Component,
                                CorrespondingView = actRequest.CorrespondingView,
                                Context = actRequest.Component.AttachInternal(
                                    actManipulator, actRequest.CorrespondingView)
                            };

                            // Register the component on local list of attached ones
                            _attachedComponents.Add(newRegisteredComponentInfo);
                        }
                        finally
                        {
                            actManipulator.IsValid = false;
                        }
                        break;

                    case SceneComponentRequestType.Detach:
                        if (actRequest.Component == null) { continue; }
                        if (!this.TryGetAttachedComponent(
                            actRequest.Component, actRequest.CorrespondingView,
                            out actComponent, out actComponentIndex))
                        {
                            // We don't have any component that is like the requested one
                            continue;
                        }

                        actManipulator = new SceneManipulator(_owner);
                        actManipulator.IsValid = true;
                        try
                        {
                            actComponent.Component.DetachInternal(
                                actManipulator, actComponent.CorrespondingView, actComponent.Context);

                            // RemoveObject the component
                            _attachedComponents.RemoveAt(actComponentIndex);
                        }
                        finally
                        {
                            actManipulator.IsValid = false;
                        }
                        break;

                    case SceneComponentRequestType.DetachAll:
                        while (_attachedComponents.Count > 0)
                        {
                            actManipulator = new SceneManipulator(_owner)
                            {
                                IsValid = true
                            };

                            try
                            {
                                actComponent = _attachedComponents[0];
                                actComponent.Component.DetachInternal(
                                    actManipulator, actComponent.CorrespondingView, actComponent.Context);

                                // RemoveObject the component
                                _attachedComponents.RemoveAt(0);
                            }
                            finally
                            {
                                actManipulator.IsValid = false;
                            }
                        }
                        break;

                    default:
                        throw new SeeingSharpException($"Unknown {nameof(SceneComponentRequestType)}: {actRequest.RequestType}");
                }
            }
        }

        private IEnumerable<SceneComponentInfo> GetExistingComponentsByGroup(string groupName, ViewInformation correspondingView)
        {
            var attachedComponentCount = _attachedComponents.Count;
            for (var loop = 0; loop < attachedComponentCount; loop++)
            {
                if (_attachedComponents[loop].Component.ComponentGroup == groupName &&
                   _attachedComponents[loop].CorrespondingView == correspondingView)
                {
                    yield return _attachedComponents[loop];
                }
            }
        }

        /// <summary>
        /// Tries the get information about a currently attached component.
        /// </summary>
        private bool TryGetAttachedComponent(
            SceneComponentBase component, ViewInformation correspondingView,
            out SceneComponentInfo componentInfo, out int componentIndex)
        {
            var attachedComponentCount = _attachedComponents.Count;
            for (var loop = 0; loop < attachedComponentCount; loop++)
            {
                if (component.IsViewSpecific)
                {
                    if (component == _attachedComponents[loop].Component &&
                       correspondingView != null &&
                       correspondingView == _attachedComponents[loop].CorrespondingView)
                    {
                        componentInfo = _attachedComponents[loop];
                        componentIndex = loop;
                        return true;
                    }
                }
                else
                {
                    if (component == _attachedComponents[loop].Component)
                    {
                        componentInfo = _attachedComponents[loop];
                        componentIndex = loop;
                        return true;
                    }
                }
            }

            componentInfo = default;
            componentIndex = -1;
            return false;
        }
    }
}
