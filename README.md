# Caching

A hands-on exploration of caching in .NET — from first principles through production-grade techniques. Covers in-memory, distributed, and hybrid caching using `IMemoryCache`, `FusionCache`, and `HybridCache`, with working implementations of stampede protection, fail-safe, eager refresh, factory timeouts, and adaptive TTL.

---

### 1. Caching

Caching is the practice of storing a computed or fetched result so it can be served faster on subsequent requests. The case for caching comes down to latency tiers: reading from L1/L2 CPU cache takes nanoseconds, reading from a distributed cache takes milliseconds, and hitting a relational database can take hundreds of milliseconds. A cache sits closer to the reader and absorbs repeated reads so the slower source is consulted only when necessary.

**What I learned:**
- Caching is memoization applied at a systems level — a trade-off between staleness tolerance and read speed, not a free performance win.
- Caches appear at every layer of the stack: CPU caches, DNS resolvers, HTTP/CDN edge caches, and application-level key-value stores all follow the same hit/miss principle.
- The decision to cache is three questions: what to cache (read-heavy data that changes infrequently), when to cache (after the hot path is identified), and how to cache (TTL, eviction policy, invalidation strategy).
- Caching introduces staleness, additional infrastructure, and invalidation complexity — it is a tool for a specific problem, not a universal fix.

---

### 2. A Cache Is A Cache Is A Cache

Before touching a library, it is worth understanding the universal data model shared by every cache. A cache maps string keys to entries; an entry is not just a value but a value plus metadata (expiration times, priority, size). The vocabulary — hit, miss, hit ratio, TTL, refresh cycle — is stable across all implementations.

**What I learned:**
- A cache entry is a value plus metadata; the metadata drives eviction. Treating an entry as a plain value is the source of most cache management bugs.
- Eviction has three distinct causes: invalidation (explicit removal by code), expiration (time-based: absolute, relative, or sliding), and eviction policies (capacity-based, e.g., LRU). Conflating them leads to wrong mental models.
- Absolute expiration is a fixed point in time; relative expiration counts from write time; sliding expiration resets on each read and suits session-like data.
- The factory pattern (`GetOrCreate`) is the correct primitive — it merges a cache read with a conditional DB call in one atomic-looking operation. Manual get-then-set cannot coordinate concurrent callers.
- Hit ratio and refresh cycle are the two numbers that indicate cache health. A low hit ratio usually means keys are too granular or TTLs are too short.
- A cache is not a database: it has no durability guarantee and entries can disappear at any time.

---

### 3. Anatomy Of A Cache

Building a memory cache from scratch — starting with a plain `Dictionary` and ending with something structurally identical to `Microsoft.Extensions.Caching.Memory` — reveals exactly what the abstraction is doing and why each piece exists.

**What I learned:**
- A naive `Dictionary<string, object>` is the kernel of any in-memory cache; everything else is bookkeeping around it.
- Expiration requires storing metadata alongside the value; the entry wraps both, so the cache can consult the expiry time on every access without the caller knowing.
- `ConcurrentDictionary` makes reads and single-key writes thread-safe, but compound operations (read-check-write) still need external locking to be atomic.
- Passive eviction (check expiry on access) is free but leaves expired entries in memory; active eviction (background timer loop) reclaims memory at the cost of a background thread and a `CancellationTokenSource` for clean shutdown.
- Implementing `IDisposable` is the correct signal that a cache owns background resources that must be released.
- Generics unlock type-safe return values without the cache needing to know the stored type; the caller sees `T` rather than `object`.
- The resulting implementation maps directly onto `MemoryCacheEntryOptions.AbsoluteExpirationRelativeToNow` and swaps in as a drop-in replacement for `IMemoryCache`.

Maps to: `caching/custom-implementation/`

---

### 4. Cache Types

There is a meaningful structural difference between the three main cache kinds: in-process memory, distributed (remote), and hybrid. The difference is not just where data lives but what guarantees each kind provides and what problems it cannot solve alone.

**What I learned:**
- A memory cache is an in-process dictionary with eviction — fast, zero serialization overhead, but invisible to other app instances and lost on restart. The term "memory cache" refers to in-process storage, not RAM as opposed to disk.
- A distributed cache is a remote key-value store (Redis, Memcached) — survives restarts and is shared across nodes, but requires serialization, adds network latency, and offers no stampede protection by default.
- A hybrid (multi-level) cache layers L1 memory over L2 distributed: reads hit the fast local tier first and fall through to the distributed tier on miss, inheriting stampede protection and fail-safe from the L1 implementation.
- For most application services, hybrid is the right default. Pure memory caches suit micro-components or single-instance services where shared state and resilience are not required.

---

### 5. The Caches We'll Use

Three concrete implementations shape the course: `IMemoryCache` (built into .NET), `FusionCache` (open-source, by the course author), and `HybridCache` (new in .NET 9). Each has a distinct feature set and meaningful gaps.

**What I learned:**
- `IMemoryCache` is the standard in-process cache in .NET. It does not protect against cache stampedes — its `GetOrCreateAsync` is an extension method that does a sequential read-check-write, so N concurrent misses all race to the database independently.
- `FusionCache` adds a per-key lock: only one factory runs per key at a time; all other concurrent callers wait and receive the same result. It also provides fail-safe, eager refresh, factory timeouts, adaptive caching, named caches, and a `.AsHybridCache()` adapter for .NET 9.
- `HybridCache` (Microsoft, .NET 9) is async-only and reduces allocations. Its stampede protection is non-deterministic in the current implementation, it has no multi-node sync without an external backplane, it does not support adaptive caching, and it cannot be instantiated directly (the class is internal).
- FusionCache is used in production by HaveIBeenPwned, Dometrain, and Microsoft's Data API Builder. The Microsoft team worked with the author to make FusionCache the recommended `HybridCache` implementation via the adapter.
- The correct namespace for `IMemoryCache` is `Microsoft.Extensions.Caching.Memory`; the `System.Runtime.Caching` namespace is deprecated and should be avoided.

---

### 6. Cache Setup

Registering and configuring a cache correctly at startup determines what is possible for the rest of the app's lifetime. The three caches have different construction models, DI patterns, and configuration APIs.

**What I learned:**
- `IMemoryCache` and `FusionCache` can be constructed with `new` (useful in tests and benchmarks); `HybridCache` cannot — it is an internal class and can only be obtained through DI.
- `IMemoryCache` registers as a single instance; FusionCache supports a default instance plus named instances (via `IFusionCacheProvider` or keyed services) for isolating different features without key collisions; `HybridCache` is a single instance with no control over the underlying L1/L2 layers.
- FusionCache's fluent builder (`WithOptions`, `WithDefaultEntryOptions`, `WithDistributedCache`, `WithBackplane`, etc.) makes it possible to compose exactly the feature set needed without unused infrastructure.
- Default entry options in FusionCache propagate correctly only when the lambda overload is used — passing a `new FusionCacheEntryOptions()` instance bypasses inheritance and loses the defaults configured at setup time.
- Named caches and cache key prefixes are the idiomatic way to isolate concerns: two `FusionCache` instances (one for "company" data, one for "fruit" data) eliminate any possibility of type collision on shared key strings.

---

### 7. Cache Stampede

A cache stampede (thundering herd) happens when many requests arrive simultaneously for a key that is not in the cache. Each request independently observes a miss and launches its own database query — N concurrent misses produce N concurrent DB calls for identical data.

**What I learned:**
- `IMemoryCache.GetOrCreateAsync` does not protect against stampedes. It is an extension method that performs a read, conditionally calls the factory, then writes — three separate steps that are not atomic and cannot coordinate concurrent callers.
- FusionCache protects against stampedes with a per-key lock: the first caller acquires the lock and executes the factory; subsequent callers for the same key wait and receive the cached result when it is written. One DB call serves N concurrent requests.
- Stampede protection only works via `GetOrSet` — not via manual `Get` + `Set` calls. The cache can only coordinate callers that route through the same entry point.
- Treating `null` as a cache miss is a latent denial-of-service: every request for a non-existent key will call the database. Cache `null` explicitly and use `TryGet` to distinguish "null is the answer" from "key not in cache".
- Null object pattern and result pattern are valid alternatives to caching `null` directly when the calling code needs to differentiate outcomes.

Maps to: `caching/stampede-protection/`, `caching/hybrid-stampede/`

---

### 8. Cache Management

Beyond reading and writing, a cache needs tools for controlling what stays in memory and for how long. Eviction priority, manual compaction, size limits, and clearing are the four knobs that prevent uncontrolled memory growth.

**What I learned:**
- Entry priority (`Low`, `Normal`, `High`, `NeverRemove`) determines eviction order under memory pressure. High-priority entries outlast lower-priority entries during capacity-based eviction; `NeverRemove` entries persist until the cache is explicitly cleared.
- Manual compaction removes a percentage of entries by count, in priority order. Automatic over-capacity compaction runs when a size limit is breached and compacts by size percentage, not entry count — a common source of confusion.
- If `SizeLimit` is set on the cache, every entry must declare a `Size` or an `InvalidOperationException` is thrown at write time. Size values are unitless: they are whatever unit the application uses consistently.
- Memory caches store references, not copies. Retrieving an object and mutating a property on it also mutates the cached entry — a silent correctness bug with no compiler warning.
- FusionCache's auto-clone feature (requires a serializer) transparently serializes on write and deserializes on read, giving each caller its own deep copy. `HybridCache` always clones because it always serializes.
- FusionCache's `Clear` is built on its tagging system and optimizes for L1-only or non-shared scenarios. `HybridCache` does not support `Clear` as of mid-2025.

Maps to: `caching/cache-invalidation/`, `caching/cache-immutability/`, `caching/manual-eviction/`

---

### 9. Houston, We Have a Problem

Caches are typically backed by databases, and databases fail. Transient failures (timeouts, brief overloads) and catastrophic failures (crashes, restarts) both surface as factory exceptions. Without a fallback strategy, a factory failure means every in-flight request gets an exception.

**What I learned:**
- The fallacies of distributed computing apply directly to cache backing stores: the network is not reliable, latency is not zero, and the database is not always available.
- Fail-safe turns a cache entry's duration into a "logical" expiration: the entry physically remains past expiry. On factory failure, FusionCache activates fail-safe, re-saves the stale value for the throttle duration, and retries after that window — serving stale rather than throwing.
- Fail-safe must be enabled at write time, not just at read time, because write time is when the physical TTL is extended to the max duration. An entry written without fail-safe enabled cannot benefit from it on a later failure.
- `expire` performs a logical removal: the entry is invisible to normal reads but remains physically in the cache for fail-safe fallback. `remove` performs a physical delete: the entry is gone and cannot serve as a fallback.
- `FailSafeDefaultValue` provides a static last-resort fallback for when there is no stale value yet — for example, on first-time cache miss while the database is already down.
- The throttle duration controls how long a failed factory backs off before retrying; the max duration is the absolute upper bound on how long a stale value is retained.

Maps to: `caching/fail-safe/`, `caching/factory-timeouts/`

---

### 10. Cache Synchronization

When the underlying data changes, the cache must be brought in sync. The right technique depends on whether the update originates inside the same process or from an external system, and on how frequently the affected key is read.

**What I learned:**
- For internal updates (same application writing to its own database), cache update (immediately writing the new value) keeps the cache warm without a miss. Cache invalidation (removing or expiring the entry) is simpler but causes one miss and one DB round-trip before the cache is warm again.
- The choice between update and invalidation is a read-frequency question: frequently read data benefits from an update to avoid a miss; rarely read data accepts the miss and keeps the write path simpler.
- Delete operations should use `remove` (physical deletion), not `expire` (logical expiry), to avoid serving a deleted record as a fail-safe fallback.
- For external updates (another service or background job modifying the database), an in-process memory cache cannot be notified. The correct approach is to keep TTLs short enough that the staleness window is acceptable — "cache for the maximum time you're OK being out-of-sync."
- Deeper synchronization patterns (cache-aside, read-through, write-through, write-behind) are architectural strategies for the L2 distributed tier and are covered in the advanced course.

Maps to: `caching/eager-refresh/`, `caching/eager-refresh-timeouts/`

---

### 11. Performance

A cache factory is a blocking operation: the caller waits for the database to respond before getting a value. Eager refresh and factory timeouts decouple the caller from the factory, turning cache misses and background refreshes into non-blocking work.

**What I learned:**
- Eager refresh triggers a background factory run when the remaining TTL drops below a configurable threshold (e.g., 60% of total duration). The current valid value is returned immediately; the background refresh updates the entry before it expires, so callers rarely observe a real miss.
- A soft factory timeout returns a stale value immediately when the configured wait time is exceeded — the factory continues running in the background and updates the cache when it completes. A soft timeout of zero means never wait at all: always return the stale value instantly and refresh behind the scenes.
- A hard factory timeout aborts the factory after the wait time. With no stale fallback available, it raises an exception or falls back to `FailSafeDefaultValue`.
- EF Core uses scoped `DbContext` instances; background factory execution runs outside the ASP.NET request scope. Injecting `DbContext` directly into a background factory causes `ObjectDisposedException`. The fix is to depend on `IServiceScopeFactory` and create a new scope inside the factory lambda.
- Micro-caching benchmark (1s TTL, soft timeout = 0, eager refresh): throughput went from 257 req/s to 513 req/s; average response time dropped from 172ms to 5–6ms; database calls dropped from 9,200+ to 6 over the same test period.

Maps to: `caching/micro-caching/`, `caching/eager-refresh-timeouts/`

---

### 12. Techniques and Best Practices

Several orthogonal techniques compose cleanly with the core features: immutability is a correctness concern, bypassing cache in write flows is a consistency concern, and adaptive caching is a flexibility concern.

**What I learned:**
- Reference semantics in .NET mean that a memory cache stores a pointer, not a copy. Reading a cached object and modifying a property on it modifies the shared cached entry — a silent correctness bug with no compiler warning.
- FusionCache's auto-clone opt-in (requires a serializer) serializes values on write and deserializes on read, giving each caller its own deep copy. `HybridCache` always clones because it always serializes. Types decorated with `[ImmutableObject(true)]` or declared as value types can skip cloning without correctness risk.
- Read-modify-write flows must bypass the cache on the read step. Reading a record from the cache, modifying it, and writing it back to the database risks overwriting changes made by another process during the window between read and write.
- Adaptive caching lets the factory control the entry's TTL based on the value being cached. The factory receives a `FusionCacheFactoryExecutionContext` and can modify `ctx.Options.Duration` before returning — for example, setting a shorter TTL for provisional or incomplete results. This is not supported in `HybridCache` (options are sealed and the factory has no return channel for metadata).

Maps to: `caching/adaptive-ttl/`

---

### 13. Putting It All Together

The final section revisits the fallacies of distributed computing and maps each one to a concrete mitigation implemented across the course. A progressive demo starts with no cache and layers in each technique to show cumulative impact.

**What I learned:**
- Each technique targets a specific failure mode: fail-safe handles database outages; eager refresh eliminates expiration latency; soft factory timeouts handle slow databases; adaptive caching handles data with unpredictable freshness requirements.
- The progression from no cache → `IMemoryCache` → FusionCache stampede protection → fail-safe → eager refresh → soft timeout = 0 is a sequence of independent, composable improvements. Each one addresses a distinct risk without coupling to the others.
- Measured impact of the full stack: 6 million database queries reduced to 12,500; average response time from 400ms to 5ms; P95 from 800ms to 10ms.
- Default entry options are the correct place for cross-cutting concerns (fail-safe configuration, eager refresh threshold, timeout values). Per-call options should carry only what is specific to that call site (typically just the duration). This keeps call sites clean and ensures all entries share the same resilience profile without repetition.
- FusionCache's `.AsHybridCache()` adapter lets the full FusionCache feature set (including adaptive caching and named instances) be used wherever a `HybridCache` is expected, bridging FusionCache with the `HybridCache` abstraction introduced in .NET 9.

Maps to: `caching/stampede/`

---

## How to Run

Each project is a standalone .NET app. Open the solution in Visual Studio or Rider:

```
caching/caching.slnx
```

Or run any project directly:

```bash
cd caching/<project-name>
dotnet run
```

For the micro-caching load test (requires [k6](https://k6.io/)):

```bash
# Start the API first
cd caching/micro-caching
dotnet run

# In a separate terminal, run the load tests
k6 run caching/micro-caching/k6/cache.js
k6 run caching/micro-caching/k6/nocache.js
```

## Folder Structure

```
caching/
├── adaptive-ttl/           # Section 12 — factory-controlled TTL
├── cache-immutability/     # Section 8  — reference semantics demo
├── cache-invalidation/     # Section 8  — explicit cache management
├── custom-implementation/  # Section 3  — memory cache built from scratch
├── eager-refresh/          # Section 10 — background refresh before expiry
├── eager-refresh-timeouts/ # Section 11 — eager refresh + factory timeouts
├── factory-timeouts/       # Section 9  — soft and hard timeout behaviour
├── fail-safe/              # Section 9  — stale-on-failure fallback
├── hybrid-stampede/        # Section 7  — stampede protection via HybridCache
├── manual-eviction/        # Section 8  — priority, size, compaction
├── micro-caching/          # Section 11 — 1s TTL benchmark with k6
├── stampede-protection/    # Section 7  — FusionCache per-key locking
└── stampede/               # Section 13 — full feature composition demo
```

---

Thanks to Jody Donetti for the course — [Getting Started: Caching in .NET](https://dometrain.com/course/getting-started-caching-in-dotnet/).

[Certificate of completion](<certificate/Caching in .NET - Einav Haimovich.pdf>)
