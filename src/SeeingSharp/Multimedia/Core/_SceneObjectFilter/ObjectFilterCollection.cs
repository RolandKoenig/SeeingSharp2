using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public class ObjectFilterCollection : IEnumerable<SceneObjectFilter>
    {
        private ViewInformation _view;

        private List<SceneObjectFilter> _objectFilters;

        internal bool ObjectFilterCollectionChanged;

        public int Count => _objectFilters.Count;

        public SceneObjectFilter this[int index] => _objectFilters[index];

        internal ObjectFilterCollection(ViewInformation view)
        {
            _view = view;
            _objectFilters = new List<SceneObjectFilter>();
        }

        internal void CopyTo(List<SceneObjectFilter> targetCollection)
        {
            targetCollection.AddRange(_objectFilters);
        }

        /// <summary>
        /// Adds the given <see cref="SceneObjectFilter"/> to this view.
        /// </summary>
        public void Add(SceneObjectFilter objectFilter)
        {
            objectFilter.EnsureNotNull(nameof(objectFilter));
            //if(objectFilter.ParentView != null){ throw new SeeingSharpException($"Unable to add given object filter: It is already assigned to a view!"); }

            //objectFilter.ParentView = _view;
            objectFilter.ConfigurationChanged = true;

            _objectFilters.Add(objectFilter);
            this.ObjectFilterCollectionChanged = true;
        }

        /// <summary>
        /// Removes the given <see cref="SceneObjectFilter"/> from this view.
        /// </summary>
        public bool Remove(SceneObjectFilter objectFilter)
        {
            objectFilter.EnsureNotNull(nameof(objectFilter));

            var result = _objectFilters.Remove(objectFilter);
            this.ObjectFilterCollectionChanged = true;

            //objectFilter.ParentView = null;

            return result;
        }

        public void Clear()
        {
            //for (var loop = 0; loop < _objectFilters.Count; loop++)
            //{
            //    var actObjectFilter = _objectFilters[loop];
            //    _objectFilters[loop] = null;

            //    //actObjectFilter.ParentView = null;
            //}
            _objectFilters.Clear();

            this.ObjectFilterCollectionChanged = true;
        }

        /// <inheritdoc />
        public IEnumerator<SceneObjectFilter> GetEnumerator()
        {
            return _objectFilters.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _objectFilters.GetEnumerator();
        }
    }
}
