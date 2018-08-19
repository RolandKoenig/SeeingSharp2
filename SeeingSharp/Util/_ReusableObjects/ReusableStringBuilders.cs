using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace SeeingSharp.Util
{
    public class ReusableStringBuilders
    {
        private ConcurrentStack<StringBuilder> s_stringBuilders;

        static ReusableStringBuilders()
        {
            Current = new ReusableStringBuilders();
        }

        public ReusableStringBuilders()
        {
            s_stringBuilders = new ConcurrentStack<StringBuilder>();
        }

        public IDisposable UseStringBuilder(out StringBuilder stringBuilder, int requiredCapacity = 128)
        {
            stringBuilder = TakeStringBuilder(requiredCapacity);

            StringBuilder cachedStringBuilder = stringBuilder;
            return new DummyDisposable(() => ReregisterStringBuilder(cachedStringBuilder));
        }

        public StringBuilder TakeStringBuilder(int requiredCapacity = 128)
        {
            StringBuilder result;
            if(!s_stringBuilders.TryPop(out result))
            {
                result = new StringBuilder(requiredCapacity);
            }
            else
            {
                if(result.Capacity < requiredCapacity) { result.EnsureCapacity(requiredCapacity); }
            }
            return result;
        }

        public void ReregisterStringBuilder(StringBuilder stringBuilder)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            s_stringBuilders.Push(stringBuilder);
        }

        public void Clear()
        {
            s_stringBuilders.Clear();
        }

        public int Count
        {
            get { return s_stringBuilders.Count; }
        }

        public static ReusableStringBuilders Current
        {
            get;
            private set;
        }
    }
}
