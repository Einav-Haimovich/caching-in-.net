using ZiggyCreatures.Caching.Fusion;

// USER INPUTS
var isDatabaseUp = true;
var isFailSafeEnabled = true;

// CACHE CONFIG
var duration = TimeSpan.FromSeconds(10);
var failSafeThrottleDuration = TimeSpan.FromSeconds(4);
var failSafeMaxDuration = TimeSpan.FromSeconds(20);
var useFailSafeDefaultValue = false;

// DISPLAY CONFIG
var showDbCalls = false;

// STATUS
var isFailSafeActivated = false;
var isExpired = false;

// SETUP
var cache = new FusionCache(new FusionCacheOptions
{
	DefaultEntryOptions = {
		Duration = duration,
		IsFailSafeEnabled = isFailSafeEnabled,
		FailSafeThrottleDuration = failSafeThrottleDuration,
		FailSafeMaxDuration = failSafeMaxDuration,
	},
	EnableSyncEventHandlersExecution = true,
});

// YEAH, I KNOW, LEAKS... DOESNT MATTER HERE
cache.Events.FailSafeActivate += (sender, e) =>
{
	isFailSafeActivated = true;
	isExpired = true;
};

Console.Clear();
Console.WriteLine("Press enter to start...");
Console.ReadLine();

Console.Clear();
Console.WriteLine();

// GET + DISPLAY LOOP
_ = Task.Run(async () =>
{
	do
	{
		// HUD - BEGIN
		DisplayHud();

		var factorySuccess = false;
		try
		{
			// GET DATA
			cache.GetOrSet(
				"foo",
				_ =>
				{
					var value = GetDataFromDb();

					factorySuccess = true;
					isFailSafeActivated = false;
					isExpired = false;

					return value;
				},
				useFailSafeDefaultValue
					? MaybeValue<int>.FromValue(-1)
					: default
				, options => options.SetFailSafe(isFailSafeEnabled)
			);

			if (factorySuccess)
			{
				// NEW DURATION STARTED
				Console.WriteLine();
				Console.WriteLine();
				if (showDbCalls)
				{
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write("->");
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("DB");
				}

				// FRESH DATA
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("█");
			}
			else
			{
				// CACHED DATA

				if (isExpired)
				{
					// EXPIRED

					if (isFailSafeActivated)
					{
						// FAIL-SAFE ACTIVATED
						isFailSafeActivated = false;
						if (showDbCalls)
						{
							Console.BackgroundColor = ConsoleColor.Black;
							Console.ForegroundColor = ConsoleColor.Gray;
							Console.Write("->");
							Console.BackgroundColor = ConsoleColor.Black;
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write("DB");
						}
						Console.BackgroundColor = ConsoleColor.Black;
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.Write("|");
					}

					// STALE DATA
					Console.BackgroundColor = ConsoleColor.DarkGreen;
					Console.ForegroundColor = ConsoleColor.Black;
					Console.Write("∙");
				}
				else
				{
					// NON-STALE DATA
					Console.BackgroundColor = ConsoleColor.Black;
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("█");
				}
			}
		}
		catch
		{
			// ERROR
			if (showDbCalls)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write("->");
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("DB");
			}
			Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write("X");
		}

		// DELAY
		await Task.Delay(TimeSpan.FromSeconds(1));
	} while (true);
});

// INPUT LOOP
while (true)
{
	var key = Console.ReadKey(true);

	if (key.KeyChar == 'd')
	{
		// DATABASE: TOGGLE UP/DOWN
		isDatabaseUp = !isDatabaseUp;
	}
	else if (key.KeyChar == 'f')
	{
		// FAIL-SAFE: TOGGLE
		isFailSafeEnabled = !isFailSafeEnabled;
	}
	else if (key.KeyChar == 'r')
	{
		// REMOVE
		cache.Remove("foo");
	}
	else if (key.KeyChar == 'x')
	{
		// EXPIRE
		cache.Expire("foo");
	}
}

// DATABASE
int GetDataFromDb()
{
	if (isDatabaseUp)
	{
		return Random.Shared.Next(100);
	}

	throw new Exception("DATABASE ERROR");
}

// HUB
void DisplayHud()
{
	var pos = Console.GetCursorPosition();
	Console.SetCursorPosition(0, 0);

	// DATABASE
	Console.BackgroundColor = ConsoleColor.Black;
	Console.ForegroundColor = ConsoleColor.Gray;
	Console.Write("DATABASE: ");
	if (isDatabaseUp)
	{
		Console.BackgroundColor = ConsoleColor.Green;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write(" OK ");
	}
	else
	{
		Console.BackgroundColor = ConsoleColor.Red;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write(" KO ");
	}

	// FAIL-SAFE
	Console.BackgroundColor = ConsoleColor.Black;
	Console.ForegroundColor = ConsoleColor.Gray;
	Console.Write(" - FAILSAFE: ");
	if (isFailSafeEnabled)
	{
		Console.BackgroundColor = ConsoleColor.Green;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write(" ON  ");
	}
	else
	{
		Console.BackgroundColor = ConsoleColor.Red;
		Console.ForegroundColor = ConsoleColor.Black;
		Console.Write(" OFF ");
	}

	Console.SetCursorPosition(pos.Left, pos.Top);
}