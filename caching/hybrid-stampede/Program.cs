using HybridStampedeBehavior;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

// SETUP DI
var services = new ServiceCollection();
services.AddHybridCache();
//services.AddFusionCache().AsHybridCache();

// GET THE CACHE
var sp = services.BuildServiceProvider();
var cache = sp.GetRequiredService<HybridCache>();

// TEST (SEQUENTIAL)
Console.WriteLine("SEQUENTIAL");
for (var i = 0; i < 10; i++)
{
	var foo = await cache.TryGetAsync<int>("foo");
	Console.WriteLine($"value = {foo.Value}, found = {foo.Found.ToString().ToUpper()}");
}

Console.WriteLine();

// TEST (PARALLEL)
Console.WriteLine("PARALLEL:");
await Parallel.ForAsync(0, 10, async (_, _) =>
{
	var foo = await cache.TryGetAsync<int>("foo");
	Console.WriteLine($"value = {foo.Value}, found = {foo.Found.ToString().ToUpper()}");
});