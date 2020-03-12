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
using SeeingSharp.Checking;
using System;

namespace SeeingSharp.Util
{
    /// <summary>
    /// This is a simple RingBuffer implementation for Seeing#,
    /// originally created for the Input event system.
    /// 
    /// More info on cyclic buffers: https://en.wikipedia.org/wiki/Circular_buffer
    /// </summary>
    public class RingBuffer<T>
    {
        // all buffer properties
        private T[] _buffer;
        private int _itemStart;
        private int _itemLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class.
        /// </summary>
        /// <param name="maxItemCount">The maximum count of items.</param>
        public RingBuffer(int maxItemCount)
        {
            maxItemCount.EnsureInRange(2, int.MaxValue, nameof(maxItemCount));

            _buffer = new T[maxItemCount];
            _itemStart = 0;
            _itemLength = 0;
        }

        /// <summary>
        /// Adds a new item to the buffer and overrides existing items
        /// if the count of items reached the maximum.
        /// </summary>
        /// <param name="newItem">The new item to be added.</param>
        public void Add(T newItem)
        {
            if (_itemLength < _buffer.Length)
            {
                _buffer[(_itemStart + _itemLength) % _buffer.Length] = newItem;
                _itemLength++;
            }
            else
            {
                _itemStart = (_itemStart + 1) % _buffer.Length;

                var nextIndex = _itemStart - 1;
                if (nextIndex < 0) { nextIndex = _buffer.Length - 1; }
                _buffer[nextIndex] = newItem;
            }
        }

        /// <summary>
        /// Adds a new item and returns the reference to it.
        /// </summary>
        public ref T AddByRef()
        {
            if (_itemLength < _buffer.Length)
            {
                _itemLength++;
                return ref _buffer[(_itemStart + (_itemLength - 1)) % _buffer.Length];
            }
            else
            {
                _itemStart = (_itemStart + 1) % _buffer.Length;

                var nextIndex = _itemStart - 1;
                if (nextIndex < 0) { nextIndex = _buffer.Length - 1; }
                return ref _buffer[nextIndex];
            }
        }

        /// <summary>
        /// Gets the item at the given index by ref.
        /// </summary>
        /// <param name="index">The index to get the item from.</param>
        public ref T GetByRef(int index)
        {
            if (index < 0){ throw new IndexOutOfRangeException(); }
            if (index >= _itemLength) { throw new IndexOutOfRangeException(); }

            return ref _buffer[(_itemStart + index) % _buffer.Length];
        }

        /// <summary>
        /// Removes the first entry.
        /// </summary>
        public void RemoveFirst()
        {
            if (_itemLength == 0)
            {
                throw new IndexOutOfRangeException();
            }

            _itemStart++;
            _itemLength--;
            if (_itemStart == _buffer.Length)
            {
                _itemStart = 0;
            }
        }

        /// <summary>
        /// Clears this collection.
        /// </summary>
        public void Clear()
        {
            _itemLength = 0;
            _itemStart = 0;
        }

        /// <summary>
        /// Gets the object at the specified index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                ref var result = ref this.GetByRef(index);
                return result;
            }
        }

        /// <summary>
        /// Gets the total count of items.
        /// </summary>
        public int Count => _itemLength;

        /// <summary>
        /// Gets the maximum capacity of the buffer.
        /// </summary>
        public int MaxCapacity => _buffer.Length;
    }
}