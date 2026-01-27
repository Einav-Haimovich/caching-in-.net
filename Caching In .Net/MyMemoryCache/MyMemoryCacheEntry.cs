using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMemoryCacheEntry
    {


        public MyMemoryCacheEntry(object? value, DateTimeOffset absoluteExpiration)
        {
            AbsoluteExpiration = absoluteExpiration;
            Value = value;
        }

        public object? Value { get; }
        public DateTimeOffset AbsoluteExpiration { get; }
    }
}
