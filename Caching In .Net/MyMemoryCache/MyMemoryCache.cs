using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MemoryCache
{
    public class MyMemoryCache : IDisposable
    {

        private ConcurrentDictionary<string, MyMemoryCacheEntry> _entries = [];
        private MyMermoryCacheOptions _options;
        private CancellationTokenSource _cts = new();
        private bool _disposedValue;

        public MyMemoryCache(MyMermoryCacheOptions? options = null)
        {
            _options = options ?? new MyMermoryCacheOptions();

            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_options.ExporationScanFrequency, _cts.Token);
                        EvictExpiredEntries(_cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    
                }
            });
        }

        private void EvictExpiredEntries(CancellationToken token)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in _entries)
            {
                if (token.IsCancellationRequested) return;
                if ( kvp.Value.IsExpired)
                {
                    _ = _entries.TryRemove(kvp.Key, out _);
                }
            }
        }

        private void CheckDisposed()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(MyMemoryCache), "Cannot access a disposed MyMemoryCache instance.");
            }
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty.");
            }
        }

        public void Set<TValue>(string key, TValue? value, MyMemoryCacheEntryOptions? cacheEntryOptions = null)
        {
            CheckDisposed();
            ValidateKey(key);
            _entries[key] = new MyMemoryCacheEntry(
                value, 
                DateTimeOffset.UtcNow + (cacheEntryOptions?.Duration ?? _options.DefaultDuration));
        }

        public TValue? Get<TValue>(string key)
        {
            if (TryGetValue<TValue>(key, out var value))
            {
                return value;
            }
            return default;
        }

        public bool TryGetValue<TValue>(string key, out TValue? value)
        {

            if (_entries.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    value = (TValue?)entry.Value;
                    return true;
                }
                else
                {
                    _ = _entries.TryRemove(key, out _);
                }
            }
            value = default(TValue?);
            return false;
        }

        public void Remove(string key)
        {
            CheckDisposed();
            ValidateKey(key);
            _ = _entries.TryRemove(key, out _);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null!;
                    // TODO: dispose managed state (managed objects)
                }
                
                _entries = null!;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
