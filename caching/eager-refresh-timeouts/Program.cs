using System.Diagnostics;
using ZiggyCreatures.Caching.Fusion;

// CONFIG
var simulatedFactoryDelay = TimeSpan.FromSeconds(4);
var duration = TimeSpan.FromSeconds(5);
var eagerRefresh = 0.6f; // 60% OF 5 SEC -> 3 SEC
var factorySoftTimeout = TimeSpan.FromMilliseconds(100);
//var factorySoftTimeout = TimeSpan.Zero;
var factoryHardTimeout = TimeSpan.FromMilliseconds(500);

// INITIAL SETUP
var cache = new FusionCache(new FusionCacheOptions
{
	DefaultEntryOptions = {
		Duration = duration,
		// REMEMBER THIS
		IsFailSafeEnabled = true,
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
		, options => options
			.SetEagerRefresh(eagerRefresh)
			.SetFactoryTimeouts(factorySoftTimeout)
	);
	sw.Stop();
	Console.WriteLine($"OPERATION {i}: ENDED ({sw.ElapsedMilliseconds} ms)");
	await Task.Delay(TimeSpan.FromSeconds(1));
}