using Microsoft.Extensions.Caching.Memory;
using ZiggyCreatures.Caching.Fusion;

Console.WriteLine("Press Enter to start");
Console.ReadLine();

// MEMORYCACHE
var memoryCache = new MemoryCache(new MemoryCacheOptions());

do
{
	var value = memoryCache.GetOrCreate<int>(
		"foo",
		entry =>
		{
			Console.WriteLine("FACTORY RUNNING...");

			var randomValue = Random.Shared.Next(5);

			// ADAPT EXPIRATION
			if (randomValue != 0)
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(randomValue);
			}

			return randomValue;
		},
		new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(7) }
	);
	Console.WriteLine($"VALUE: {value}");
	await Task.Delay(TimeSpan.FromSeconds(1));
} while (true);

//// FUSIONCACHE
//var fusionCache = new FusionCache(new FusionCacheOptions());

//do
//{
//	var value = fusionCache.GetOrSet<int>(
//		"foo",
//		(ctx, ct) =>
//		{
//			Console.WriteLine("FACTORY RUNNING...");

//			var randomValue = Random.Shared.Next(5);

//			// ADAPT EXPIRATION
//			if (randomValue != 0)
//			{
//				ctx.Options.Duration = TimeSpan.FromSeconds(randomValue);
//			}

//			return randomValue;
//		},
//		options => options.SetDuration(TimeSpan.FromSeconds(7))
//	);
//	Console.WriteLine($"VALUE: {value}");
//	await Task.Delay(TimeSpan.FromSeconds(1));
//} while (true);
