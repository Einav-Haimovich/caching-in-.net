using System.Diagnostics;
using ZiggyCreatures.Caching.Fusion;

// CONFIG
var simulatedFactoryDelay = TimeSpan.FromSeconds(4);
var duration = TimeSpan.FromSeconds(5);
var factorySoftTimeout = TimeSpan.FromMilliseconds(100);
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

// STEP
Console.WriteLine("NO ENTRY, SOFT TIMEOUT:");
sw.Restart();
await cache.GetOrSetAsync(
	"foo",
	async _ =>
	{
		Console.WriteLine("> FACTORY STARTED");
		await Task.Delay(simulatedFactoryDelay);
		Console.WriteLine("> FACTORY ENDED");
		return 42;
	},
	options => options.SetFactoryTimeouts(factorySoftTimeout)
);
sw.Stop();
Console.WriteLine($"DONE ({sw.ElapsedMilliseconds} ms)");
Console.WriteLine();

for (int i = 1; i < 4; i++)
{
	// STEP
	Console.WriteLine($"VALID ENTRY, SOFT TIMEOUT ({i}):");
	sw.Restart();
	await cache.GetOrSetAsync(
		"foo",
		async _ =>
		{
			Console.WriteLine("> FACTORY STARTED");
			await Task.Delay(simulatedFactoryDelay);
			Console.WriteLine("> FACTORY ENDED");
			return 42;
		},
		options => options.SetFactoryTimeouts(factorySoftTimeout)
	);
	sw.Stop();
	Console.WriteLine($"DONE ({sw.ElapsedMilliseconds} ms)");
	Console.WriteLine();
}

// WAITING FOR EXPIRATION
Console.WriteLine("WAIT FOR THE EXPIRATION:");
await Task.Delay(duration);
Console.WriteLine("DONE");
Console.WriteLine();

// STEP
Console.WriteLine("EXPIRED ENTRY, SOFT TIMEOUT:");
sw.Restart();
await cache.GetOrSetAsync(
	"foo",
	async _ =>
	{
		Console.WriteLine("> FACTORY STARTED");
		await Task.Delay(simulatedFactoryDelay);
		Console.WriteLine("> FACTORY ENDED");
		return 42;
	},
	options => options.SetFactoryTimeouts(factorySoftTimeout)
);
sw.Stop();
Console.WriteLine($"DONE ({sw.ElapsedMilliseconds} ms)");
Console.WriteLine();

// WAITING FOR THE FACTORY
Console.WriteLine("WAIT FOR THE FACTORY (BACKGROUND):");
await Task.Delay(simulatedFactoryDelay);
Console.WriteLine("DONE");
Console.WriteLine();

// REMOVE
Console.WriteLine("REMOVE");
cache.Remove("foo");
Console.WriteLine();

// STEP
Console.WriteLine("NO ENTRY, HARD TIMEOUT:");
string? error = null;
sw.Restart();
try
{
	await cache.GetOrSetAsync(
		"foo",
		async _ =>
		{
			Console.WriteLine("> FACTORY STARTED");
			await Task.Delay(simulatedFactoryDelay);
			Console.WriteLine("> FACTORY ENDED");
			return 42;
		},
		options => options.SetFactoryTimeouts(factorySoftTimeout, factoryHardTimeout)
	);
}
catch (Exception exc)
{
	error = exc.Message;
}
sw.Stop();
Console.WriteLine($"DONE ({sw.ElapsedMilliseconds} ms)");
if (error is not null)
{
	Console.WriteLine($"! EXCEPTION: {error}");
}
Console.WriteLine();

// WAITING FOR THE FACTORY
Console.WriteLine("WAIT FOR THE FACTORY (BACKGROUND):");
await Task.Delay(simulatedFactoryDelay);
Console.WriteLine("DONE");
Console.WriteLine();

// REMOVE
Console.WriteLine("REMOVE");
cache.Remove("foo");
Console.WriteLine();

// STEP
Console.WriteLine("NO ENTRY, HARD TIMEOUT, FAIL-SAFE DEFAULT VALUE:");
error = null;
sw.Restart();
try
{
	await cache.GetOrSetAsync(
		"foo",
		async _ =>
		{
			Console.WriteLine("> FACTORY STARTED");
			await Task.Delay(simulatedFactoryDelay);
			Console.WriteLine("> FACTORY ENDED");
			return 42;
		},
		123,
		options => options.SetFactoryTimeouts(factorySoftTimeout, factoryHardTimeout)
	);
}
catch (Exception exc)
{
	error = exc.Message;
}
sw.Stop();
Console.WriteLine($"DONE ({sw.ElapsedMilliseconds} ms)");
if (error is not null)
{
	Console.WriteLine($"! EXCEPTION: {error}");
}
Console.WriteLine();

// WAITING FOR THE FACTORY
Console.WriteLine("WAIT FOR THE FACTORY (BACKGROUND):");
await Task.Delay(simulatedFactoryDelay);
Console.WriteLine("DONE");
Console.WriteLine();
