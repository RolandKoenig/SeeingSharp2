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
using System.Collections.Concurrent;
using SeeingSharp.Checking;
using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// This is mostly logic of the Scene class but extracted here
    /// following the Flyweight pattern.
    /// </summary>
    internal class SceneComponentFlyweight
    {
        // Main members
        private Scene m_owner;
        private ConcurrentQueue<SceneComponentRequest> m_componentRequests;
        private List<SceneComponentInfo> m_attachedComponents;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneComponentFlyweight"/> class.
        /// </summary>
        internal SceneComponentFlyweight(Scene owner)
        {
            m_owner = owner;
            m_componentRequests = new ConcurrentQueue<SceneComponentRequest>();
            m_attachedComponents = new List<SceneComponentInfo>();
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

            m_componentRequests.Enqueue(new SceneComponentRequest
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

            m_componentRequests.Enqueue(new SceneComponentRequest
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
            m_componentRequests.Enqueue(new SceneComponentRequest
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
            var attachedComponentsCount = m_attachedComponents.Count;

            for (var loop = 0; loop < attachedComponentsCount; loop++)
            {
                m_attachedComponents[loop].Component.UpdateInternal(
                    updateState,
                    m_attachedComponents[loop].CorrespondingView,
                    m_attachedComponents[loop].Context);
            }

            // Attach all components which are coming in
            while (m_componentRequests.TryDequeue(out var actRequest))
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
                                m_componentRequests.Enqueue(new SceneComponentRequest
                                {
                                    RequestType = SceneComponentRequestType.Detach,
                                    Component = actObsoleteComponent.Component,
                                    CorrespondingView = actObsoleteComponent.CorrespondingView
                                });
                            }
                        }

                        var actManipulator = new SceneManipulator(m_owner);
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
                            m_attachedComponents.Add(newRegisteredComponentInfo);
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

                        actManipulator = new SceneManipulator(m_owner);
                        actManipulator.IsValid = true;
                        try
                        {
                            actComponent.Component.DetachInternal(
                                actManipulator, actComponent.CorrespondingView, actComponent.Context);

                            // RemoveObject the component
                            m_attachedComponents.RemoveAt(actComponentIndex);
                        }
                        finally
                        {
                            actManipulator.IsValid = false;
                        }
                        break;

                    case SceneComponentRequestType.DetachAll:
                        while (m_attachedComponents.Count > 0)
                        {
                            actManipulator = new SceneManipulator(m_owner)
                            {
                                IsValid = true
                            };

                            try
                            {
                                actComponent = m_attachedComponents[0];
                                actComponent.Component.DetachInternal(
                                    actManipulator, actComponent.CorrespondingView, actComponent.Context);

                                // RemoveObject the component
                                m_attachedComponents.RemoveAt(0);
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
            var attachedComponentCount = m_attachedComponents.Count;
            for (var loop = 0; loop < attachedComponentCount; loop++)
            {
                if (m_attachedComponents[loop].Component.ComponentGroup == groupName &&
                   m_attachedComponents[loop].CorrespondingView == correspondingView)
                {
                    yield return m_attachedComponents[loop];
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
            var attachedComponentCount = m_attachedComponents.Count;
            for (var loop = 0; loop < attachedComponentCount; loop++)
            {
                if (component.IsViewSpecific)
                {
                    if (component == m_attachedComponents[loop].Component &&
                       correspondingView != null &&
                       correspondingView == m_attachedComponents[loop].CorrespondingView)
                    {
                        componentInfo = m_attachedComponents[loop];
                        componentIndex = loop;
                        return true;
                    }
                }
                else
                {
                    if (component == m_attachedComponents[loop].Component)
                    {
                        componentInfo = m_attachedComponents[loop];
                        componentIndex = loop;
                        return true;
                    }
                }
            }

            componentInfo = default;
            componentIndex = -1;
            return false;
        }

        internal int CountAttached => m_attachedComponents.Count;
    }
}
