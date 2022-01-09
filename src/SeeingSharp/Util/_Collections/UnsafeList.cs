using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    // Original code from .Net Core source
    // Changes:
    //  - Some naming changes to meet coding style of SeeingSharp
    //  - Access to the backing array through the BackingArray property

    /// <summary>
    /// A implementation of a list that allows accessing the internal array.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class UnsafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
    {
        private const int DEFAULT_CAPACITY = 4;

        private static readonly T[] s_emptyArray = new T[0];

        private T[] _backingArray;
        private int _size;
        private int _version;

        public int Capacity
        {
            get => _backingArray.Length;
            set
            {
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == _backingArray.Length)
                {
                    return;
                }

                if (value > 0)
                {
                    var array = new T[value];
                    if (_size > 0)
                    {
                        Array.Copy(_backingArray, 0, array, 0, _size);
                    }
                    _backingArray = array;
                }
                else
                {
                    _backingArray = s_emptyArray;
                }
            }
        }

        public int Count => _size;

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _backingArray[index];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                _backingArray[index] = value;
                _version++;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        public T?[] BackingArray => _backingArray;

        public UnsafeList()
        {
            _backingArray = s_emptyArray;
        }

        public UnsafeList(int capacity)
        {
            capacity.EnsurePositiveOrZero(nameof(capacity));

            if (capacity == 0)
            {
                _backingArray = s_emptyArray;
            }
            else
            {
                _backingArray = new T[capacity];
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
                            _backingArray = s_emptyArray;
                            return;
                        }
                        _backingArray = new T[count];
                        collection2.CopyTo(_backingArray, 0);
                        _size = count;
                        break;
                    }

                default:
                    {
                        _size = 0;
                        _backingArray = s_emptyArray;
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
            if (_size == _backingArray.Length)
            {
                this.ApplyCapacity(_size + 1);
            }

            _backingArray[_size++] = item;
            _version++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            this.InsertRange(_size, collection);
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(_backingArray, 0, _size);
                _size = 0;
            }
            _version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (var i = 0; i < _size; i++)
                {
                    if (_backingArray[i] == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            var @default = EqualityComparer<T>.Default;
            for (var j = 0; j < _size; j++)
            {
                if (@default.Equals(_backingArray[j], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_backingArray, 0, array, arrayIndex, _size);
        }

        public T Find(Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            for (var i = 0; i < _size; i++)
            {
                if (match(_backingArray[i]))
                {
                    return _backingArray[i];
                }
            }
            return default;
        }

        public UnsafeList<T> FindAll(Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            var customList = new UnsafeList<T>();
            for (var i = 0; i < _size; i++)
            {
                if (match(_backingArray[i]))
                {
                    customList.Add(_backingArray[i]);
                }
            }
            return customList;
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)_size)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (count < 0 || startIndex > _size - count)
            {
                throw new ArgumentOutOfRangeException();
            }
            match.EnsureNotNull(nameof(match));

            var num = startIndex + count;
            for (var i = startIndex; i < num; i++)
            {
                if (match(_backingArray[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            match.EnsureNotNull(nameof(match));

            if (_size == 0)
            {
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else if ((uint)startIndex >= (uint)_size)
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
                if (match(_backingArray[num2]))
                {
                    return num2;
                }
            }
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            action.EnsureNotNull(nameof(action));

            var version = _version;
            for (var i = 0; i < _size; i++)
            {
                if (version != _version)
                {
                    break;
                }
                action(_backingArray[i]);
            }
            if (version != _version)
            {
                throw new InvalidOperationException("Enumeration has changed during the foreach loop!");
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_backingArray, item, 0, _size);
        }

        public int IndexOf(T item, int index)
        {
            if (index > _size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return Array.IndexOf(_backingArray, item, index, _size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index > _size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || index > _size - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return Array.IndexOf(_backingArray, item, index, count);
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (_size == _backingArray.Length)
            {
                this.ApplyCapacity(_size + 1);
            }
            if (index < _size)
            {
                Array.Copy(_backingArray, index, _backingArray, index + 1, _size - index);
            }
            _backingArray[index] = item;
            _size++;
            _version++;
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            collection.EnsureNotNull(nameof(collection));

            if ((uint)index > (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (collection is ICollection<T> collection2)
            {
                var count = collection2.Count;
                if (count > 0)
                {
                    this.ApplyCapacity(_size + count);
                    if (index < _size)
                    {
                        Array.Copy(_backingArray, index, _backingArray, index + count, _size - index);
                    }
                    if (ReferenceEquals(this, collection2))
                    {
                        Array.Copy(_backingArray, 0, _backingArray, index, index);
                        Array.Copy(_backingArray, index + count, _backingArray, index * 2, _size - index);
                    }
                    else
                    {
                        var array = new T[count];
                        collection2.CopyTo(array, 0);
                        array.CopyTo(_backingArray, index);
                    }
                    _size += count;
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
            _version++;
        }

        public int LastIndexOf(T item)
        {
            if (_size == 0)
            {
                return -1;
            }
            return this.LastIndexOf(item, _size - 1, _size);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= _size)
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
            if (_size == 0)
            {
                return -1;
            }
            if (index >= _size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count > index + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return Array.LastIndexOf(_backingArray, item, index, count);
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
            for (i = 0; i < _size && !match(_backingArray[i]); i++)
            {
            }
            if (i >= _size)
            {
                return 0;
            }
            var j = i + 1;
            while (j < _size)
            {
                for (; j < _size && match(_backingArray[j]); j++)
                {
                }
                if (j < _size)
                {
                    _backingArray[i++] = _backingArray[j++];
                }
            }
            Array.Clear(_backingArray, i, _size - i);
            var result = _size - i;
            _size = i;
            _version++;
            return result;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _size--;
            if (index < _size)
            {
                Array.Copy(_backingArray, index + 1, _backingArray, index, _size - index);
            }
            _backingArray[_size] = default;
            _version++;
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
            if (_size - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > 0)
            {
                _size -= count;
                if (index < _size)
                {
                    Array.Copy(_backingArray, index + count, _backingArray, index, _size - index);
                }
                Array.Clear(_backingArray, _size, count);
                _version++;
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
            if (_size - index < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            Array.Sort(_backingArray, index, count, comparer);
            _version++;
        }

        public void Sort(Comparison<T> comparison)
        {
            comparison.EnsureNotNull(nameof(comparison));

            if (_size > 0)
            {
                IComparer<T> comparer = new FunctionComparer(comparison);
                Array.Sort(_backingArray, 0, _size, comparer);
            }
        }

        private void ApplyCapacity(int min)
        {
            if (_backingArray.Length < min)
            {
                var num = _backingArray.Length == 0 ? 4 : _backingArray.Length * 2;
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

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private UnsafeList<T> _mUnsafeList;
            private int _index;
            private int _version;
            private T _current;

            internal Enumerator(UnsafeList<T> mUnsafeList)
            {
                _mUnsafeList = mUnsafeList;
                _index = 0;
                _version = mUnsafeList._version;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var customList = _mUnsafeList;
                if (_version == customList._version && (uint)_index < (uint)customList._size)
                {
                    _current = customList._backingArray[_index];
                    _index++;
                    return true;
                }
                return this.MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _mUnsafeList._version)
                {
                    throw new InvalidOperationException("Collection was modified while enumerating");

                }
                _index = _mUnsafeList._size + 1;
                _current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _mUnsafeList._version)
                {
                    throw new InvalidOperationException("Collection was modified while enumerating");
                }
                _index = 0;
                _current = default;
            }

            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _mUnsafeList._size + 1)
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
            private Comparison<T> _comparison;

            public FunctionComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }
        }
    }
}
