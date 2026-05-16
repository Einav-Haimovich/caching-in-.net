using Microsoft.Extensions.Caching.Memory;

// SETUP
var cache = new MemoryCache(new MemoryCacheOptions());

// PROCESSING
Console.WriteLine("ADD 10 ENTRIES");

// ADD 3 ENTRIES WITH PRIORITY = LOW
cache.Set("entry1", "LOW", new MemoryCacheEntryOptions { Priority = CacheItemPriority.Low });
cache.Set("entry2", "LOW", new MemoryCacheEntryOptions { Priority = CacheItemPriority.Low });
cache.Set("entry3", "LOW", new MemoryCacheEntryOptions { Priority = CacheItemPriority.Low });

// ADD 3 ENTRIES WITH PRIORITY = NORMAL (DEFAULT, NOT SPECIFIED)
cache.Set("entry4", "NORMAL");
cache.Set("entry5", "NORMAL");
cache.Set("entry6", "NORMAL");

// ADD 3 ENTRIES WITH PRIORITY = HIGH
cache.Set("entry7", "HIGH", new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
cache.Set("entry8", "HIGH", new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
cache.Set("entry9", "HIGH", new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });

// ADD 1 ENTRY WITH PRIORITY = NEVER REMOVE
cache.Set("entry10", "NEVER REMOVE", new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });

DebugEntries();

// ACCESS THE FIRST ENTRY -> UPDATE ITS LAST ACCESS
_ = cache.Get<string?>("entry1");

Console.WriteLine("COMPACT (20%)");
cache.Compact(0.2);
DebugEntries();

Console.WriteLine("COMPACT (50%)");
cache.Compact(0.5);
DebugEntries();

Console.WriteLine("COMPACT (100%)");
cache.Compact(1);
DebugEntries();


void DebugEntries()
{
	var count = cache.Count;

	Console.WriteLine();
	Console.WriteLine($"DEBUG (COUNT: {count})");
	for (int i = 1; i <= 10; i++)
	{
		var value = cache.Get<string?>($"entry{i}");
		Console.WriteLine($"- entry{i}: {value ?? "/"}");
	}
	Console.WriteLine("----------------------------------------");
}