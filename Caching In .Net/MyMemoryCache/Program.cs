using MemoryCache;


var cache = new MyMemoryCache();


// SET
cache.Set("name", "John Doe", TimeSpan.FromMinutes(10));
cache.Set("age", 30);

// GET
var name = cache.Get("name");
var age = cache.Get("age");

Console.WriteLine($"Name: {name}, Age: {age}");

// REMOVE
cache.Remove("age");
var removedAge = cache.Get("age");
Console.WriteLine($"Removed Age: {removedAge}");

// SET NULL
cache.Set("nickname", null);
var nickname = cache.Get("nickname");
Console.WriteLine($"Nickname: {nickname ?? "null"}");