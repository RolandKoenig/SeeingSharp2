using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
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
using System.Threading;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class UnsafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
    {
        private const int DEFAULT_CAPACITY = 4;

        private static readonly T[] s_emptyArray = new T[0];

        private T[] m_backingArray;
        private int m_size;
        private int m_version;

        public UnsafeList()
        {
            m_backingArray = s_emptyArray;
        }

        public UnsafeList(int capacity)
        {
            capacity.EnsurePositiveOrZero(nameof(capacity));

            if (capacity == 0)
            {
                m_backingArray = s_emptyArray;
            }
            else
            {
                m_backingArray = new T[capacity];
            }
        }

        public UnsafeList(IEnumerable<T> collection)
        {
            switch (collection)
            {
                case null:
                    throw new ArgumentNullException(nameof(collection));

                case ICollection<T> collection2:
                    {
                        var count = collection2.Count;
                        if (count == 0)
                        {
                            m_backingArray = s_emptyArray;
                            return;
                        }
                        m_backingArray = new T[count];
                        collection2.CopyTo(m_backingArray, 0);
                        m_size = count;
                        break;
                    }

                default:
                    {
                        m_size = 0;
                        m_backingArray = s_emptyArray;
                        foreach (var item in collection)
                        {
                            this.Add(item);
                        }
                        break;
                    }
            }
        }

        public void Add(T item)
        {
            if (m_size == m_backingArray.Length)
            {
                this.ApplyCapacity(m_size + 1);
            }

            m_backingArray[m_size++] = item;
            m_version++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            this.InsertRange(m_size, collection);
        }

        public void Clear()
        {
            if (m_size > 0)
            {
                Array.Clear(m_backingArray, 0, m_size);
                m_size = 0;
            }
            m_version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (var i = 0; i < m_size; i++)
                {
                    if (m_backingArray[i] == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            var @default = EqualityComparer<T>.Default;
            for (var j = 0; j < m_size; j++)
            {
                if (@default.Equals(m_backingArray[j], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(m_backingArray, 0, array, arrayIndex, m_size);
        }

        public T Find(Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            for (var i = 0; i < m_size; i++)
            {
                if (match(m_backingArray[i]))
                {
                    return m_backingArray[i];
                }
            }
            return default;
        }

        public UnsafeList<T> FindAll(Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            var customList = new UnsafeList<T>();
            for (var i = 0; i < m_size; i++)
            {
                if (match(m_backingArray[i]))
                {
                    customList.Add(m_backingArray[i]);
                }
            }
            return customList;
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)m_size)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || startIndex > m_size - count)
            {
                throw new ArgumentOutOfRangeException();
            }
            match.EnsureNotNull(nameof(match));

            var num = startIndex + count;
            for (var i = startIndex; i < num; i++)
            {
                if (match(m_backingArray[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            if (m_size == 0)
            {
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else if ((uint)startIndex >= (uint)m_size)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || startIndex - count + 1 < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            var num = startIndex - count;
            for (var num2 = startIndex; num2 > num; num2--)
            {
                if (match(m_backingArray[num2]))
                {
                    return num2;
                }
            }
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            action.EnsureNotNull(nameof(action));

            var version = m_version;
            for (var i = 0; i < m_size; i++)
            {
                if (version != m_version)
                {
                    break;
                }
                action(m_backingArray[i]);
            }
            if (version != m_version)
            {
                throw new InvalidOperationException($"Enumeration has changed during the foreach loop!");
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(m_backingArray, item, 0, m_size);
        }

        public int IndexOf(T item, int index)
        {
            if (index > m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return Array.IndexOf(m_backingArray, item, index, m_size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index > m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || index > m_size - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return Array.IndexOf(m_backingArray, item, index, count);
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (m_size == m_backingArray.Length)
            {
                this.ApplyCapacity(m_size + 1);
            }
            if (index < m_size)
            {
                Array.Copy(m_backingArray, index, m_backingArray, index + 1, m_size - index);
            }
            m_backingArray[index] = item;
            m_size++;
            m_version++;
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            collection.EnsureNotNull(nameof(collection));

            if ((uint)index > (uint)m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (collection is ICollection<T> collection2)
            {
                var count = collection2.Count;
                if (count > 0)
                {
                    this.ApplyCapacity(m_size + count);
                    if (index < m_size)
                    {
                        Array.Copy(m_backingArray, index, m_backingArray, index + count, m_size - index);
                    }
                    if (this == collection2)
                    {
                        Array.Copy(m_backingArray, 0, m_backingArray, index, index);
                        Array.Copy(m_backingArray, index + count, m_backingArray, index * 2, m_size - index);
                    }
                    else
                    {
                        var array = new T[count];
                        collection2.CopyTo(array, 0);
                        array.CopyTo(m_backingArray, index);
                    }
                    m_size += count;
                }
            }
            else
            {
                using (var enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Insert(index++, enumerator.Current);
                    }
                }
            }
            m_version++;
        }

        public int LastIndexOf(T item)
        {
            if (m_size == 0)
            {
                return -1;
            }
            return this.LastIndexOf(item, m_size - 1, m_size);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this.LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (this.Count != 0 && index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (this.Count != 0 && count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (m_size == 0)
            {
                return -1;
            }
            if (index >= m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count > index + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return Array.LastIndexOf(m_backingArray, item, index, count);
        }

        public bool Remove(T item)
        {
            var num = this.IndexOf(item);
            if (num >= 0)
            {
                this.RemoveAt(num);
                return true;
            }
            return false;
        }

        public int RemoveAll(Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            int i;
            for (i = 0; i < m_size && !match(m_backingArray[i]); i++)
            {
            }
            if (i >= m_size)
            {
                return 0;
            }
            var j = i + 1;
            while (j < m_size)
            {
                for (; j < m_size && match(m_backingArray[j]); j++)
                {
                }
                if (j < m_size)
                {
                    m_backingArray[i++] = m_backingArray[j++];
                }
            }
            Array.Clear(m_backingArray, i, m_size - i);
            var result = m_size - i;
            m_size = i;
            m_version++;
            return result;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)m_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            m_size--;
            if (index < m_size)
            {
                Array.Copy(m_backingArray, index + 1, m_backingArray, index, m_size - index);
            }
            m_backingArray[m_size] = default;
            m_version++;
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (m_size - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > 0)
            {
                var size = m_size;
                m_size -= count;
                if (index < m_size)
                {
                    Array.Copy(m_backingArray, index + count, m_backingArray, index, m_size - index);
                }
                Array.Clear(m_backingArray, m_size, count);
                m_version++;
            }
        }

        public void Sort()
        {
            this.Sort(0, this.Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            this.Sort(0, this.Count, comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (m_size - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            Array.Sort(m_backingArray, index, count, comparer);
            m_version++;
        }

        public void Sort(Comparison<T> comparison)
        {
            comparison.EnsureNotNull(nameof(comparison));

            if (m_size > 0)
            {
                IComparer<T> comparer = new FunctionComparer(comparison);
                Array.Sort(m_backingArray, 0, m_size, comparer);
            }
        }

        private void ApplyCapacity(int min)
        {
            if (m_backingArray.Length < min)
            {
                var num = m_backingArray.Length == 0 ? 4 : m_backingArray.Length * 2;
                if ((uint)num > 2146435071u)
                {
                    num = 2146435071;
                }
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int Capacity
        {
            get => m_backingArray.Length;
            set
            {
                if (value < m_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == m_backingArray.Length)
                {
                    return;
                }

                if (value > 0)
                {
                    var array = new T[value];
                    if (m_size > 0)
                    {
                        Array.Copy(m_backingArray, 0, array, 0, m_size);
                    }
                    m_backingArray = array;
                }
                else
                {
                    m_backingArray = s_emptyArray;
                }
            }
        }

        public int Count => m_size;

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)m_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return m_backingArray[index];
            }
            set
            {
                if ((uint)index >= (uint)m_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                m_backingArray[index] = value;
                m_version++;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        public T[] BackingArray => m_backingArray;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private UnsafeList<T> _mUnsafeList;
            private int m_index;
            private int m_version;
            private T m_current;

            internal Enumerator(UnsafeList<T> mUnsafeList)
            {
                _mUnsafeList = mUnsafeList;
                m_index = 0;
                m_version = mUnsafeList.m_version;
                m_current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var customList = _mUnsafeList;
                if (m_version == customList.m_version && (uint)m_index < (uint)customList.m_size)
                {
                    m_current = customList.m_backingArray[m_index];
                    m_index++;
                    return true;
                }
                return this.MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (m_version != _mUnsafeList.m_version)
                {
                    throw new InvalidOperationException($"Collection was modified while enumerating");

                }
                m_index = _mUnsafeList.m_size + 1;
                m_current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (m_version != _mUnsafeList.m_version)
                {
                    throw new InvalidOperationException($"Collection was modified while enumerating");
                }
                m_index = 0;
                m_current = default;
            }

            public T Current => m_current;

            object IEnumerator.Current
            {
                get
                {
                    if (m_index == 0 || m_index == _mUnsafeList.m_size + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.Current;
                }
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class FunctionComparer : IComparer<T>
        {
            private Comparison<T> m_comparison;

            public FunctionComparer(Comparison<T> comparison)
            {
                m_comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return m_comparison(x, y);
            }
        }
    }
}
