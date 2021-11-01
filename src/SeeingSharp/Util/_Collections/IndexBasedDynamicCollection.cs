using System;
using System.Collections;
using System.Collections.Generic;

namespace SeeingSharp.Util
{
    public class IndexBasedDynamicCollection<T> : IEnumerable<T>
        where T : class
    {
        private UnsafeList<T> _list;
        private int _listCount;
        private object _lockObject;
        private Dictionary<T, int> _objectIndices;

        /// <summary>
        /// Gets the element at the given index.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        public T this[int index]
        {
            get
            {
                var objectList = _list.BackingArray;
                if (index >= objectList.Length) { return null; }
                return objectList[index];
            }
        }

        /// <summary>
        /// Gets the first item or null if there is none.
        /// </summary>
        public T FirstOrDefaultItem
        {
            get
            {
                var objectList = _list.BackingArray;
                for (var loop = 0; loop < objectList.Length; loop++)
                {
                    if (objectList[loop] != null) { return objectList[loop]; }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the total count of items within this collection.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _objectIndices.Count;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexBasedDynamicCollection{T}" /> class.
        /// </summary>
        public IndexBasedDynamicCollection()
        {
            _list = new UnsafeList<T>(10);
            _listCount = 0;
            _objectIndices = new Dictionary<T, int>();
            _lockObject = new object();
        }

        /// <summary>
        /// Has this collection an object at the given index?
        /// </summary>
        /// <param name="index">The index to check.</param>
        public bool HasObjectAt(int index)
        {
            var objectList = _list.BackingArray;

            if (objectList.Length <= index) { return false; }
            if (index < 0) { return false; }

            return objectList[index] != null;
        }

        /// <summary>
        /// Adds an object to this collection and returns the index on which the item was placed.
        /// </summary>
        /// <param name="objectToAdd">The object to be added to the collection.</param>
        public int AddObject(T objectToAdd)
        {
            lock (_lockObject)
            {
                if (_objectIndices.ContainsKey(objectToAdd)) { throw new SeeingSharpException("This object is already added!"); }

                var lastValidIndex = -1;
                for (var loop = _list.Count - 1; loop >= 0; loop--)
                {
                    if (_list[loop] == null) { lastValidIndex = loop; }
                    else { break; }
                }

                if (lastValidIndex > -1)
                {
                    _list[lastValidIndex] = objectToAdd;
                    _objectIndices.Add(objectToAdd, lastValidIndex);
                    _listCount = _list.Count;
                    return lastValidIndex;
                }
                _list.Add(objectToAdd);
                _objectIndices.Add(objectToAdd, _list.Count - 1);
                _listCount = _list.Count;
                return _list.Count - 1;
            }
        }

        /// <summary>
        /// Adds an object to the given index location.
        /// </summary>
        /// <param name="objectToAdd">The object to be added.</param>
        /// <param name="index">The index on which this object should be added.</param>
        public T AddObject(T objectToAdd, int index)
        {
            this.AddObject(objectToAdd, index, true);
            return objectToAdd;
        }

        /// <summary>
        /// Adds an object to the given index location.
        /// </summary>
        /// <param name="objectToAdd">The object to be added.</param>
        /// <param name="index">The index on which this object should be added.</param>
        /// <param name="throwIfIndexAlreadyUsed">Throw an exception if the item does already exist? If false, the existing value will be overwritten.</param>
        public void AddObject(T objectToAdd, int index, bool throwIfIndexAlreadyUsed)
        {
            var currentObject = this[index];

            if (currentObject != null)
            {
                if (throwIfIndexAlreadyUsed)
                {
                    throw new SeeingSharpException("There is already an object at the given index!");
                }

                this.RemoveObject(currentObject);
            }

            // Perform all operations for adding this object
            lock (_lockObject)
            {
                if (_objectIndices.ContainsKey(objectToAdd))
                {
                    throw new SeeingSharpException("This object is already added!");
                }

                while (_list.Count <= index)
                {
                    _list.Add(null);
                }

                _list[index] = objectToAdd;
                _objectIndices[objectToAdd] = index;
                _listCount = _list.Count;
            }
        }

        /// <summary>
        /// Is the given item already added to this collection?
        /// </summary>
        /// <param name="objectToCheckFor">The object to check for.</param>
        public bool Contains(T objectToCheckFor)
        {
            lock (_lockObject)
            {
                return _objectIndices.ContainsKey(objectToCheckFor);
            }
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="objectToCheckFor">The object to check for.</param>
        public int IndexOf(T objectToCheckFor)
        {
            lock (_lockObject)
            {
                return _objectIndices[objectToCheckFor];
            }
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        public void RemoveObject(T objectToRemove)
        {
            lock (_lockObject)
            {
                if (_objectIndices.ContainsKey(objectToRemove))
                {
                    var objectIndex = _objectIndices[objectToRemove];

                    _objectIndices.Remove(objectToRemove);
                    _list[objectIndex] = null;
                    _listCount = _list.Count;
                }
            }
        }

        /// <summary>
        /// Removes the object at the given index.
        /// </summary>
        /// <param name="index">The index of the object to remove.</param>
        public void RemoveObject(int index)
        {
            var objectToRemove = _list[index];
            this.RemoveObject(objectToRemove);
        }

        /// <summary>
        /// Clears the complete collection.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _list.Clear();
                _objectIndices.Clear();
                _listCount = _list.Count;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var max = _list.Count;

            for (var loop = 0; loop < max; loop++)
            {
                if (_list.Count != max)
                {
                    throw new InvalidOperationException("Collection was modified!");
                }

                var actObject = _list[loop];

                if (actObject != null)
                {
                    yield return actObject;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
