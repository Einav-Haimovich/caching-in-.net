namespace Microcaching;

public static class FakeDatabase
{
	static int _databaseCalls = 0;

	// QUERY
	public static async Task<string> GetThingAsync()
	{
		await Task.Delay(TimeSpan.FromSeconds(1));
		Interlocked.Increment(ref _databaseCalls);
		return "My Product";
	}

	// GET STATS
	public static int GetStats()
	{
		return _databaseCalls;
	}

	// RESET STATS
	public static void Reset()
	{
		Interlocked.Exchange(ref _databaseCalls, 0);
	}
}
