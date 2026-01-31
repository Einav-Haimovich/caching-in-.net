using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMermoryCacheOptions
    {
        public TimeSpan DefaultDuration { get; set; }
        public TimeSpan ExporationScanFrequency { get; set; }

        public MyMermoryCacheOptions()
        {
            DefaultDuration = TimeSpan.FromMinutes(5);
            ExporationScanFrequency = TimeSpan.FromMinutes(1);
        }
    }
}
