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
using System.Collections;
using System.Collections.Generic;

namespace SeeingSharp.Util
{
    public class IndexBasedDynamicCollection<T> : IEnumerable<T>
        where T : class
    {
        private List<T> m_list;
        private int m_listCount;
        private object m_lockObject;
        private Dictionary<T, int> m_objectIndices;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexBasedDynamicCollection{T}" /> class.
        /// </summary>
        public IndexBasedDynamicCollection()
        {
            m_list = new List<T>(10);
            m_listCount = 0;
            m_objectIndices = new Dictionary<T, int>();
            m_lockObject = new object();
        }

        /// <summary>
        /// Has this collection an object at the given index?
        /// </summary>
        /// <param name="index">The index to check.</param>
        public bool HasObjectAt(int index)
        {
            var objectList = m_list;

            if (m_listCount <= index) { return false; }
            if (index < 0) { return false; }

            return objectList[index] != null;
        }

        /// <summary>
        /// Adds an object to this collection and returns the index on which the item was placed.
        /// </summary>
        /// <param name="objectToAdd">The object to be added to the collection.</param>
        public int AddObject(T objectToAdd)
        {
            lock (m_lockObject)
            {
                if (m_objectIndices.ContainsKey(objectToAdd)) { throw new SeeingSharpException("This object is already added!"); }

                var lastValidIndex = -1;
                for (var loop = m_list.Count - 1; loop >= 0; loop--)
                {
                    if (m_list[loop] == null) { lastValidIndex = loop; }
                    else { break; }
                }

                if (lastValidIndex > -1)
                {
                    m_list[lastValidIndex] = objectToAdd;
                    m_objectIndices.Add(objectToAdd, lastValidIndex);
                    m_listCount = m_list.Count;
                    return lastValidIndex;
                }
                m_list.Add(objectToAdd);
                m_objectIndices.Add(objectToAdd, m_list.Count - 1);
                m_listCount = m_list.Count;
                return m_list.Count - 1;
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
            lock (m_lockObject)
            {
                if (m_objectIndices.ContainsKey(objectToAdd))
                {
                    throw new SeeingSharpException("This object is already added!");
                }

                while (m_list.Count <= index)
                {
                    m_list.Add(null);
                }

                m_list[index] = objectToAdd;
                m_objectIndices[objectToAdd] = index;
                m_listCount = m_list.Count;
            }
        }

        /// <summary>
        /// Is the given item already added to this collection?
        /// </summary>
        /// <param name="objectToCheckFor">The object to check for.</param>
        public bool Contains(T objectToCheckFor)
        {
            lock (m_lockObject)
            {
                return m_objectIndices.ContainsKey(objectToCheckFor);
            }
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="objectToCheckFor">The object to check for.</param>
        public int IndexOf(T objectToCheckFor)
        {
            lock (m_lockObject)
            {
                return m_objectIndices[objectToCheckFor];
            }
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        public void RemoveObject(T objectToRemove)
        {
            lock (m_lockObject)
            {
                if (m_objectIndices.ContainsKey(objectToRemove))
                {
                    var objectIndex = m_objectIndices[objectToRemove];

                    m_objectIndices.Remove(objectToRemove);
                    m_list[objectIndex] = null;
                    m_listCount = m_list.Count;
                }
            }
        }

        /// <summary>
        /// Removes the object at the given index.
        /// </summary>
        /// <param name="index">The index of the object to remove.</param>
        public void RemoveObject(int index)
        {
            var objectToRemove = m_list[index];
            this.RemoveObject(objectToRemove);
        }

        /// <summary>
        /// Clears the complete collection.
        /// </summary>
        public void Clear()
        {
            lock (m_lockObject)
            {
                m_list.Clear();
                m_objectIndices.Clear();
                m_listCount = m_list.Count;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var max = m_list.Count;

            for (var loop = 0; loop < max; loop++)
            {
                if (m_list.Count != max)
                {
                    throw new InvalidOperationException("Collection was modified!");
                }

                var actObject = m_list[loop];

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

        /// <summary>
        /// Gets the element at the given index.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        public T this[int index]
        {
            get
            {
                var objectList = m_list;
                if (index >= m_listCount) { return null; }
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
                var objectList = m_list;
                for (var loop = 0; loop < objectList.Count; loop++)
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
                lock (m_lockObject)
                {
                    return m_objectIndices.Count;
                }
            }
        }
    }
}
