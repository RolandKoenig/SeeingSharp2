using System.Threading;

namespace SeeingSharp.Util
{
    public class UniqueGenericKeyGenerator
    {
        private long _nextGenericKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueGenericKeyGenerator" /> class.
        /// </summary>
        public UniqueGenericKeyGenerator()
        {
            _nextGenericKey = long.MinValue;
        }

        /// <summary>
        /// Generates the next generic key.
        /// </summary>
        public NamedOrGenericKey GetNextGeneric()
        {
            var result = Interlocked.Increment(ref _nextGenericKey);
            result--;
            return new NamedOrGenericKey(result);
        }
    }
}
