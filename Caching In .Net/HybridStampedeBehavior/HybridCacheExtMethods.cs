using Microsoft.Extensions.Caching.Hybrid;

namespace HybridStampedeBehavior;

public static class HybridCacheExtMethods
{
	private static readonly HybridCacheEntryOptions _TryGetAsync_EntryOptions = new()
	{
		Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
	};

	// PROBLEMS:
	// - NON DETERMINISTIC
	// - WRONG BEHAVIOUR WITH L2 TO L1 COPY
	public static async ValueTask<(bool Found, T? Value)> TryGetAsync<T>(this HybridCache cache, string key)
	{
		var found = true;
		var value = await cache.GetOrCreateAsync(
			key,
			_ =>
			{
				found = false;
				return ValueTask.FromResult(default(T));
			},
			_TryGetAsync_EntryOptions
		);

		return (found, value);
	}

	// PROBLEMS:
	// - USING EXCEPTIONS FOR CONTROL FLOW
	// - PERFORMANCE
	// - WRONG BEHAVIOUR WITH L2 TO L1 COPY
	// - LOGGING/OBSERVABILITY BLOAT
	public static async ValueTask<(bool Found, T? Value)> TryGetAsync2<T>(this HybridCache cache, string key)
	{
		var found = true;
		T? value = default;
		try
		{
			value = await cache.GetOrCreateAsync<T>(
				key,
				_ =>
				{
					throw new Exception("CACHE MISS");
				},
				_TryGetAsync_EntryOptions
			);
		}
		catch (Exception)
		{
			found = false;
		}

		return (found, value);
	}
}