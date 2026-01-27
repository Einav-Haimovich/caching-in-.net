using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMemoryCache
    {

        private Dictionary<string, MyMemoryCacheEntry> _entries;

        public MyMemoryCache()
        {
            _entries = new Dictionary<string, MyMemoryCacheEntry>();
        }

        public void Set(string key, object? value, TimeSpan? duration = null)
        {
            _entries[key] = new MyMemoryCacheEntry(
                value, 
                DateTimeOffset.UtcNow + (duration ?? TimeSpan.FromMinutes(5)));
        }

        public object? Get(string key)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                if (entry.AbsoluteExpiration > DateTimeOffset.UtcNow)
                {
                    value = entry.Value;
                    return true;
                }
                else
                {
                    _entries.Remove(key);
                }
            }
            value = default(object?);
            return false;
        }

        public void Remove(string key)
        {
            _entries.Remove(key);
        }
    }
}
