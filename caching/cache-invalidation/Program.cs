using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

// SETUP
var mc = new MemoryCache(new MemoryCacheOptions());
var fc1 = new FusionCache(new FusionCacheOptions());
var fc2a = new FusionCache(
	new FusionCacheOptions { CacheKeyPrefix = "FC2A-" },
	mc
);
var fc2b = new FusionCache(
	new FusionCacheOptions { CacheKeyPrefix = "FC2B-" },
	mc
);
var fc3a = new FusionCache(
	// NO CACHE KEY PREFIX
	new FusionCacheOptions(),
	mc
);
var fc3b = new FusionCache(
	// NO CACHE KEY PREFIX
	new FusionCacheOptions(),
	mc
);

// PROCESSING (MEMORYCACHE)
Console.WriteLine("----------------------------------------");
Console.WriteLine("MEMORYCACHE");
Console.WriteLine("----------------------------------------");
Console.WriteLine("ADDING 4 ENTRIES");
for (int i = 1; i <= 4; i++)
{
	mc.Set($"entry{i}", $"MC {i}");
	mc.Set($"entry{i}", $"MC {i}");
	mc.Set($"entry{i}", $"MC {i}");
	mc.Set($"entry{i}", $"MC {i}");
}
DebugMemoryCacheEntries(mc);

Console.WriteLine("CLEAR");
mc.Clear();
DebugMemoryCacheEntries(mc);

// PROCESSING (FUSIONCACHE)
Console.WriteLine("----------------------------------------");
Console.WriteLine("FUSIONCACHE");
Console.WriteLine("----------------------------------------");
Console.WriteLine("ADDING 4 ENTRIES");
for (int i = 1; i <= 4; i++)
{
	fc1.Set($"entry{i}", $"FC1 {i}");
	fc1.Set($"entry{i}", $"FC1 {i}");
	fc1.Set($"entry{i}", $"FC1 {i}");
	fc1.Set($"entry{i}", $"FC1 {i}");
}
DebugFusionCacheEntries(fc1);

Console.WriteLine("CLEAR");
fc1.Clear();
DebugFusionCacheEntries(fc1);

// PROCESSING (FUSIONCACHE X2)
Console.WriteLine("----------------------------------------");
Console.WriteLine("FUSIONCACHE (2 INSTANCES, SHARED L1, PREFIX)");
Console.WriteLine("----------------------------------------");
Console.WriteLine("ADDING 4 ENTRIES");
for (int i = 1; i <= 4; i++)
{
	fc2a.Set($"entry{i}", $"FC2A {i}");
	fc2a.Set($"entry{i}", $"FC2A {i}");
	fc2a.Set($"entry{i}", $"FC2A {i}");
	fc2a.Set($"entry{i}", $"FC2A {i}");

	fc2b.Set($"entry{i}", $"FC2B {i}");
	fc2b.Set($"entry{i}", $"FC2B {i}");
	fc2b.Set($"entry{i}", $"FC2B {i}");
	fc2b.Set($"entry{i}", $"FC2B {i}");
}
DebugFusionCacheEntries(fc2a);
DebugFusionCacheEntries(fc2b);

Console.WriteLine("CLEAR (FC2A)");
fc2a.Clear();
DebugFusionCacheEntries(fc2a);
DebugFusionCacheEntries(fc2b);

Console.WriteLine("CLEAR (FC2B)");
fc2b.Clear();
DebugFusionCacheEntries(fc2a);
DebugFusionCacheEntries(fc2b);

// PROCESSING (FUSIONCACHE X2)
Console.WriteLine("----------------------------------------");
Console.WriteLine("FUSIONCACHE (2 INSTANCES, SHARED L1, NO PREFIX)");
Console.WriteLine("----------------------------------------");
Console.WriteLine("ADDING 4 ENTRIES");
for (int i = 1; i <= 4; i++)
{
	fc3a.Set($"entry{i}", $"FC3A {i}");
	fc3a.Set($"entry{i}", $"FC3A {i}");
	fc3a.Set($"entry{i}", $"FC3A {i}");
	fc3a.Set($"entry{i}", $"FC3A {i}");

	fc3b.Set($"entry{i}", $"FC3B {i}");
	fc3b.Set($"entry{i}", $"FC3B {i}");
	fc3b.Set($"entry{i}", $"FC3B {i}");
	fc3b.Set($"entry{i}", $"FC3B {i}");
}
DebugFusionCacheEntries(fc3a);
DebugFusionCacheEntries(fc3b);

Console.WriteLine("CLEAR (FC3A)");
fc3a.Clear();
DebugFusionCacheEntries(fc3a);
DebugFusionCacheEntries(fc3b);

Console.WriteLine("CLEAR (FC3B)");
fc3b.Clear();
DebugFusionCacheEntries(fc3a);
DebugFusionCacheEntries(fc3b);

Console.WriteLine();
Console.WriteLine("FIN");


static void DebugMemoryCacheEntries(MemoryCache cache)
{
	Console.WriteLine();
	Console.WriteLine($"DEBUG (MEMORYCACHE)");
	for (int i = 1; i <= 4; i++)
	{
		var value = cache.Get<string?>($"entry{i}");
		Console.WriteLine($"- entry{i}: {(value ?? " / ")}");
	}
	Console.WriteLine("----------------------------------------");
}

static void DebugFusionCacheEntries(FusionCache cache)
{
	Console.WriteLine();
	Console.WriteLine($"DEBUG (FUSIONCACHE)");
	for (int i = 1; i <= 4; i++)
	{
		var value = cache.GetOrDefault<string?>($"entry{i}");
		Console.WriteLine($"- entry{i}: {(value ?? " / ")}");
	}
	Console.WriteLine("----------------------------------------");
}