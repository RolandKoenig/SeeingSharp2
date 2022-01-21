using System;
using System.Threading;

namespace SeeingSharp.Util
{
    // Original code from book "Pro .Net Memory Management" by Konrad Kokosa (sample sourcecode chapter 6)
    // Changes:
    //  - Only some namings to meet coding style of SeeingSharp
    //
    // Based on ObjectPool<T> from Roslyn source code (with comments reused):
    // https://github.com/dotnet/roslyn/blob/d4dab355b96955aca5b4b0ebf6282575fad78ba8/src/Dependencies/PooledObjects/ObjectPool%601.cs

    /// <summary>
    /// Helper class for reusing objects.
    /// 
    /// </summary>
    public class ConcurrentObjectPool<T> 
        where T : class
    {
        private readonly T?[] _items;
        private readonly Func<T> _generator;
        private T? _firstItem;

        public ConcurrentObjectPool(Func<T> generator, int size)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _items = new T[size - 1];
        }

        public T Rent()
        {
            // PERF: Examine the first element. If that fails, RentSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = _firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
            {
                inst = this.RentSlow();
            }
            return inst;
        }

        public void Return(T item)
        {
            if (_firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _firstItem = item;
            }
            else
            {
                this.ReturnSlow(item);
            }
        }

        private T RentSlow()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional. 
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                var inst = _items[i];
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref _items[i], null, inst))
                    {
                        return inst;
                    }
                }
            }
            return _generator();
        }

        private void ReturnSlow(T obj)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    _items[i] = obj;
                    break;
                }
            }
        }
    }
}
