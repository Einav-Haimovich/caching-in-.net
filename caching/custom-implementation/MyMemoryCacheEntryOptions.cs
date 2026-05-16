using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMemoryCacheEntryOptions
    {
        public TimeSpan? Duration { get; set; } = null;

        public MyMemoryCacheEntryOptions()
        {
            
        }
    }
}
