using System.Diagnostics;
using ZiggyCreatures.Caching.Fusion;

// CONFIG
var simulatedFactoryDelay = TimeSpan.FromSeconds(4);
var duration = TimeSpan.FromSeconds(5);
var eagerRefresh = 0.6f; // 60% OF 5 SEC -> 3 SEC

// INITIAL SETUP
var cache = new FusionCache(new FusionCacheOptions
{
	DefaultEntryOptions = {
		Duration = duration,
	}
});
var sw = new Stopwatch();

for (int i = 1; i <= 20; i++)
{
	// STEP
	Console.WriteLine($"OPERATION {i}: STARTED");
	sw.Restart();
	await cache.GetOrSetAsync(
		"foo",
		async _ =>
		{
			Console.WriteLine("> FACTORY STARTED");
			await Task.Delay(simulatedFactoryDelay);
			Console.WriteLine("> FACTORY ENDED");
			return 42;
		}
		, options => options.SetEagerRefresh(eagerRefresh)
	);
	sw.Stop();
	Console.WriteLine($"OPERATION {i}: ENDED ({sw.ElapsedMilliseconds} ms)");
	await Task.Delay(TimeSpan.FromSeconds(1));
}