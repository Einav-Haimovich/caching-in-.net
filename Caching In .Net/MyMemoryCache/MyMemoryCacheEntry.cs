using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMemoryCacheEntry
    {
        public object? Value { get; }
        public DateTimeOffset AbsoluteExpiration { get; }
        public bool IsExpired
        {
            get
            {
                return DateTimeOffset.UtcNow >= AbsoluteExpiration;
            }
        }

        public MyMemoryCacheEntry(object? value, DateTimeOffset absoluteExpiration)
        {
            AbsoluteExpiration = absoluteExpiration;
            Value = value;
        }
    }
}
