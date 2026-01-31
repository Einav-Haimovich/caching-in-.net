using MemoryCache;


var cache = new MyMemoryCache();
var memoryCacheEntryOpts = new MyMemoryCacheEntryOptions
{
    Duration = TimeSpan.FromMinutes(2)
};


// SET
cache.Set("name", "John Doe", memoryCacheEntryOpts);
cache.Set("age", 30);

// GET
var name = cache.Get<string>("name");
var age = cache.Get<int?>("age");

Console.WriteLine($"Name: {name}, Age: {age}");

// REMOVE
cache.Remove("age");
var removedAge = cache.Get<int?>("age");
Console.WriteLine($"Removed Age: {removedAge}");

// SET NULL
cache.Set<object>("nickname", null);
var nickname = cache.Get<string>("nickname");
Console.WriteLine($"Nickname: {nickname ?? "null"}");