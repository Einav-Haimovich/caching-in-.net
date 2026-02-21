using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using ZiggyCreatures.Caching.Fusion;

int databaseCallsCount = 0;

async Task<string> GetDataFromDb()
{
	Interlocked.Increment(ref databaseCallsCount);

	await Task.Delay(TimeSpan.FromSeconds(1));

	return "Hi from XYZ";
}

// SETUP DI
var services = new ServiceCollection();

// MEMORY
services.AddMemoryCache();
// FUSION
services.AddFusionCache();
// HYBRID
services.AddHybridCache();
//// FUSION HYBRID ADAPTER
//services.AddFusionCache().AsHybridCache();

var serviceProvider = services.BuildServiceProvider();

var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
var fusionCache = serviceProvider.GetRequiredService<IFusionCache>();
var hybridCache = serviceProvider.GetRequiredService<HybridCache>();

// SIMULATE CACHE STAMPEDE
var tasks = new ConcurrentBag<Task>();
var requestsCount = 1_000;

// MEMORY
Parallel.For(0, requestsCount, _ =>
{
	var task = memoryCache.GetOrCreateAsync<string>(
		"foo",
		async _ => await GetDataFromDb()
	);
	tasks.Add(task);
});

//// FUSION
//Parallel.For(0, requestsCount, _ =>
//{
//	var task = fusionCache.GetOrSetAsync<string>(
//		"foo",
//		async _ => await GetDataFromDb()
//	);
//	tasks.Add(task.AsTask());
//});

//// HYBRID (ALSO, FUSION HYBRID ADAPTER)
//Parallel.For(0, requestsCount, _ =>
//{
//	var task = hybridCache.GetOrCreateAsync<string>(
//		"foo",
//		async _ => await GetDataFromDb()
//	);
//	tasks.Add(task.AsTask());
//});

//// FUSION HYBRID ADAPTER (MIXED)
//Parallel.For(0, requestsCount, _ =>
//{
//	var task1 = fusionCache.GetOrSetAsync<string>(
//		"foo",
//		async _ => await GetDataFromDb()
//	);
//	tasks.Add(task1.AsTask());

//	var task2 = hybridCache.GetOrCreateAsync<string>(
//		"foo",
//		async _ => await GetDataFromDb()
//	);
//	tasks.Add(task2.AsTask());
//});

// AWAIT
await Task.WhenAll(tasks);

// RESULTS
Console.WriteLine($"{databaseCallsCount} DB CALL(S) OVER {requestsCount} REQUESTS");
