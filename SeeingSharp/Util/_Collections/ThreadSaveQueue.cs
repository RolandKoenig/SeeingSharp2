#region License information
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
#endregion
namespace SeeingSharp.Util
{
    #region using

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// A queue object that performs each action using methods of the Interlocked class and therefore does not need any locking mechanism.
    /// </summary>
    /// <typeparam name="T">The type of the objects within the list.</typeparam>
    public class ThreadSaveQueue<T>
    {
        private ConcurrentQueue<T> m_backingQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSaveQueue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSaveQueue()
        {
            m_backingQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Enqueue(T item)
        {
            m_backingQueue.Enqueue(item);
        }

        /// <summary>
        /// Enqueues all items within the given collection.
        /// </summary>
        /// <param name="items">The items to be enqueued.</param>
        public void Enqueue(IEnumerable<T> items)
        {
            foreach(var actItem in items)
            {
                m_backingQueue.Enqueue(actItem);
            }
        }

        /// <summary>
        /// Are there any elements in the queue?
        /// </summary>
        public bool HasAny()
        {
            return m_backingQueue.Count > 0;
        }

        /// <summary>
        /// Peeks the first item from the queue.
        /// This method does not remove the item.
        /// </summary>
        /// <param name="item">The read item.</param>
        public bool Peek(out T item)
        {
            return m_backingQueue.TryPeek(out item);
        }

        /// <summary>
        /// Peeks objects from the queue while checkPeekItem func returns true.
        /// </summary>
        /// <param name="checkPeekItem">The checking func.</param>
        public IEnumerable<T> PeekWhile(Func<T, bool> checkPeekItem)
        {
            IEnumerable<T> enumarableForPeek = m_backingQueue;

            // Peek all following items on which checkPeekItem returns true
            //         .. and stop on first "false"
            foreach (var loopItem in enumarableForPeek)
            {
                if (!checkPeekItem(loopItem))
                {
                    break;
                }

                yield return loopItem;
            }
        }

        /// <summary>
        /// Dequeues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Dequeue(out T item)
        {
            return m_backingQueue.TryDequeue(out item);
        }

        /// <summary>
        /// Dequeues items from the queue as long as checkDequeueItem func returns true.
        /// </summary>
        /// <param name="checkDequeueItem">The checking function.</param>
        public IEnumerable<T> DequeueWhile(Func<T, bool> checkDequeueItem)
        {
            // Dequeue and return items as long as checkDequeueItem returns true
            var actItem = default(T);
            while (m_backingQueue.TryPeek(out actItem) &&
                  checkDequeueItem(actItem) &&
                  m_backingQueue.TryDequeue(out actItem))
            {
                yield return actItem;
            }
        }

        /// <summary>
        /// This method first dequeues items as long checkDequeueItem func returns true. After that
        /// it peeks items as long as checkPeekItem func returns true.
        /// </summary>
        /// <param name="checkDequeueItem"></param>
        /// <param name="checkPeekItem"></param>
        public IEnumerable<T> DequeueAndPeekWhile(
            Func<T, bool> checkDequeueItem,
            Func<T, bool> checkPeekItem)
        {
            // Execute dequeuing first
            foreach(var actItem in DequeueWhile(checkDequeueItem))
            {
                yield return actItem;
            }

            // Execute peeking at last
            foreach(var actItem in PeekWhile(checkPeekItem))
            {
                yield return actItem;
            }
        }

        /// <summary>
        /// Deques all items within this queue and returns them using a List object.
        /// </summary>
        public List<T> DequeueAll()
        {
            List<T> result = new List<T>(m_backingQueue.Count);

            var actItem = default(T);

            while (this.Dequeue(out actItem))
            {
                result.Add(actItem);
            }

            return result;
        }

        /// <summary>
        /// Deques all items within this queue and puts the items to the given target list.
        /// </summary>
        public void DequeueAll(List<T> targetList)
        {
            var actItem = default(T);

            while (this.Dequeue(out actItem))
            {
                targetList.Add(actItem);
            }
        }

        /// <summary>
        /// Removes all items from the queue.
        /// </summary>
        public void Clear()
        {
            var actItem = default(T);
            while (this.Dequeue(out actItem)) { }
        }

        /// <summary>
        /// Gets the total count of items within the queue.
        /// </summary>
        public int Count
        {
            get { return m_backingQueue.Count; }
        }
    }
}