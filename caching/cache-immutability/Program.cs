using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

// SETUP DI
var services = new ServiceCollection();

// MEMORY
services.AddMemoryCache();

// FUSION
services.AddFusionCache();
//services.AddFusionCache()
//	.WithSystemTextJsonSerializer()
//	.WithDefaultEntryOptions(options =>
//	{
//		options.EnableAutoClone = true;
//	});

// HYBRID
services.AddHybridCache();

// GET CACHES
var serviceProvider = services.BuildServiceProvider();

var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
var fusionCache = serviceProvider.GetRequiredService<IFusionCache>();
var hybridCache = serviceProvider.GetRequiredService<HybridCache>();


// CREATE
Console.WriteLine($"JOHN 1: CREATE");
Console.WriteLine();
var john1 = new Person
{
	FirstName = "John",
	LastName = "Doe"
};

Console.WriteLine($"DEBUG:");
Console.WriteLine($"- JOHN 1: {john1}");
Console.WriteLine();

// SET IN CACHE
Console.WriteLine($"JOHN 1: SET IN CACHE");
Console.WriteLine();
memoryCache.Set("john", john1);
//fusionCache.Set("john", john1);
//await hybridCache.SetAsync("john", john1);

// GET FROM CACHE
Console.WriteLine($"JOHN 2: GET FROM CACHE");
Console.WriteLine();
var john2 = memoryCache.Get<Person>("john")!;
//var john2 = fusionCache.GetOrDefault<Person>("john")!;
//var john2 = await hybridCache.GetOrCreateAsync<Person>(
//	"john",
//	async ct => null!
//)!;

Console.WriteLine($"DEBUG:");
Console.WriteLine($"- JOHN 1: {john1}");
Console.WriteLine($"- JOHN 2: {john2}");
Console.WriteLine();

// CHANGE NAME
Console.WriteLine("JOHN 2: CHANGE NAME");
Console.WriteLine();
john2.FirstName = "Johnny";

Console.WriteLine("DEBUG");
Console.WriteLine($"JOHN 1: {john1}");
Console.WriteLine($"JOHN 2: {john2}");
Console.WriteLine();


public record class Person
{
	public required string FirstName { get; set; }
	public required string LastName { get; set; }
}