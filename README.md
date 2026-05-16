# Caching

Caching is one of the hardest problems in software — not because the data structure is complex, but because the failure modes are subtle. This repo works through the key patterns and failure modes one at a time: stampede, stale data, timeouts, eviction, and what happens when you build a cache from scratch.

---

## [custom-implementation](caching/custom-implementation/)

Build a memory cache from first principles: a `ConcurrentDictionary` as the backing store, a background thread scanning for expired entries, and the core get/set/remove contract.

_Learned: a cache is just a dictionary plus expiration metadata plus a cleanup loop — understanding this makes every higher-level caching API feel transparent instead of magical._

---

## [stampede-protection](caching/stampede-protection/)

Simulates 1,000 concurrent requests hitting an empty cache and counts how many database calls each implementation triggers. The difference between a naive approach and a stampede-protected one is not small.

_Learned: `IMemoryCache.GetOrCreateAsync` does not protect against stampedes — every concurrent caller gets its own factory invocation until one wins. Libraries that deduplicate factory calls ensure only one request hits the database, regardless of concurrency._

---

## [fail-safe](caching/fail-safe/)

Interactive demo: toggle the database off and watch the cache serve the last known value instead of throwing an error. Toggle it back on and normal refresh resumes.

_Learned: fail-safe is "stale is better than an error" — when the source is unavailable, serving an expired cached value is almost always preferable to propagating the failure to every caller._

---

## [eager-refresh](caching/eager-refresh/)

When a cached entry's remaining TTL drops below a threshold (60%), the next request silently triggers a background factory call and immediately returns the current value. No caller ever waits for the refresh.

_Learned: eager refresh decouples cache expiry from user-visible latency — callers always get an instant cache hit while the new value is fetched in the background._

---

## [factory-timeouts](caching/factory-timeouts/)

Two timeout levels: soft (100ms) lets the factory keep running in the background and returns stale data immediately; hard (500ms) aborts the factory call entirely.

_Learned: soft and hard timeouts serve fundamentally different purposes — soft prevents tail-latency spikes by accepting staleness, hard is a circuit-breaker for factory calls that are stuck or runaway._

---

## [eager-refresh-timeouts](caching/eager-refresh-timeouts/)

Combines eager refresh and factory timeouts: the 60% threshold triggers a background refresh, but that background call is also subject to a soft timeout. Shows how the two behaviors compose.

---

## [adaptive-ttl](caching/adaptive-ttl/)

The factory inspects what it's about to return and adjusts the entry's expiration accordingly — if the value is a "miss" (zero), skip caching entirely; otherwise cache for a duration derived from the value itself.

_Learned: adaptive TTL lets the factory control caching behavior — it knows far more about what it's returning than any fixed `AbsoluteExpirationRelativeToNow` ever can._

---

## [cache-invalidation](caching/cache-invalidation/)

Invalidates entries by exact key, by key prefix, and by clearing everything. Shows the gap between a minimal cache API (no native prefix support) and one designed for structured invalidation.

_Learned: key naming conventions are a caching design decision, not just an implementation detail — `user:{id}:profile` prefixes make bulk invalidation trivial if your cache library supports it._

---

## [cache-immutability](caching/cache-immutability/)

Mutates an object after storing it in cache, then shows that the cached value changed too — because the cache holds a reference, not a copy.

_Learned: memory caches store references, not copies — mutating the object you retrieved from the cache mutates the cache itself. Use immutable types or store serialized snapshots._

---

## [manual-eviction](caching/manual-eviction/)

Fills a memory cache with entries of varying priorities, then triggers compaction to 50% capacity. Shows which entries survive and why.

_Learned: memory cache eviction is not LRU by default — it uses the priority you assigned when inserting entries, so you need to think about eviction policy at write time, not just expiry._

---

## [micro-caching](caching/micro-caching/)

ASP.NET web API with `/nocache` and `/cache` endpoints plus k6 load test scripts. At 1,000 concurrent users with a 5-second TTL, the cached endpoint makes roughly 1 database call versus hundreds without caching.

_Learned: even a 1-second TTL can absorb enormous database load under high traffic — micro-caching doesn't need to be long-lived to be effective._

---

## [hybrid-stampede](caching/hybrid-stampede/)

Explores what happens when you try to implement `TryGetAsync` on a cache that has no native try-get API. Two workaround approaches are implemented and both have subtle problems: non-deterministic behavior and using exceptions for control flow.

---

## [stampede](caching/stampede/)

Work in progress.

---

## How to Run

Each folder is a standalone project. Run any of them with:

```bash
cd caching/<folder-name>
dotnet run
```

For the `micro-caching` web API, the k6 load tests are in `caching/micro-caching/k6/`:

```bash
k6 run caching/micro-caching/k6/cache.js
k6 run caching/micro-caching/k6/nocache.js
```

Open `caching/caching.slnx` in Visual Studio or Rider to browse all projects in one solution.
