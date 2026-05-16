using Microcaching;
using ZiggyCreatures.Caching.Fusion;

// SETUP
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFusionCache()
	.WithDefaultEntryOptions(options =>
	{
		// MICRO
		options.Duration = TimeSpan.FromSeconds(5);

		// MICRO
		//options.Duration = TimeSpan.FromSeconds(1);

		//// FAILSAFE + FACTORY TIMEOUT
		//options.IsFailSafeEnabled = true;
		//options.FactorySoftTimeout = TimeSpan.FromMilliseconds(100);
	});

// APP
var app = builder.Build();
app.UseHttpsRedirection();

// NO CACHE
app.MapGet("/nocache", async () =>
{
	return await FakeDatabase.GetThingAsync();
});

// CACHE
app.MapGet("/cache", async (IFusionCache cache) =>
{
	return await cache.GetOrSetAsync(
		"my-product",
		async _ => await FakeDatabase.GetThingAsync()
	);
});

// STATS
app.MapGet("/stats", () =>
{
	return $"{FakeDatabase.GetStats()} db calls";
});

// RESET
app.MapGet("/reset", () =>
{
	FakeDatabase.Reset();
	return "database calls counter reset to 0";
});

app.Run();