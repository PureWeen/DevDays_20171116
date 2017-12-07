using Akavache;
using Akavache.Sqlite3;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDays.Services
{
    public class AkavacheStorageService : IStorageService
    {
        // IFileSystemProvider is defined internally on Akavache for each platform
        private Lazy<IFilesystemProvider> _fileSystemProvider;
        private Lazy<IObservable<IBlobCache>> _getCache;
        private SerialDisposable _previousCache;
        private readonly IScheduler _scheduler;
        private readonly string _dbFileName;
        private readonly IBlobCache _blobCache;


        public AkavacheStorageService(
            Lazy<IFilesystemProvider> fileSystemProvider,
            IScheduler scheduler,
            string dbFileName = "blobs.db",
            IBlobCache blobCache = null)
        {
            _dbFileName = dbFileName;
            _fileSystemProvider = _fileSystemProvider ?? Locator.Current.GetLazyLocator<IFilesystemProvider>();
            _previousCache = new SerialDisposable();
            _scheduler = scheduler;
            _blobCache = blobCache;

            SetupNewCache();
        }


        public IObservable<Unit> DeleteData<T>(string key)
        {
            return InvalidateObject<T>(key);
        }
        public IObservable<Unit> StoreData<T>(string key, T data)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            return InsertObject(key, data);
        }

        public IObservable<T> GetData<T>(string key)
        {
            return GetObject<T>(key);
        }

        public IObservable<IEnumerable<T>> GetDataOfType<T>()
        { 
            return GetAllObjects<T>();
        }


        public IObservable<IBlobCache> CreateLocalMachine()
        {
            BlobCache.ApplicationName = "DevDays"; 
            var fs = _fileSystemProvider.Value;
            var path = fs.GetDefaultLocalMachineCacheDirectory();

            return
                fs.CreateRecursive(path)
                    .SubscribeOn(_scheduler)
                    .Select(_ => _blobCache ??  new SQLitePersistentBlobCache(Path.Combine(path, _dbFileName), _scheduler));
        }


        public IObservable<Unit> Delete()
        {
            return _fileSystemProvider.Value.Delete(_dbFileName);
        }


        void SetupNewCache()
        {            
            _previousCache.Disposable = Disposable.Empty;
            _getCache = new Lazy<IObservable<IBlobCache>>(() =>
                CreateLocalMachine()
                    .Replay(1)
                    .PermaRef(d => _previousCache.Disposable = d));
        }

        public void Flush()
        {
            if (_getCache.IsValueCreated)
            {
                _getCache
                    .Value
                    .SelectMany(cache => cache.Flush())
                    .Wait();
            }
        }

        public IObservable<Unit> ShutDown()
        {
            if (_getCache.IsValueCreated)
            {
                return 
                    _getCache
                        .Value
                        .SelectMany(cache => cache.Shutdown);
            }

            return Observable.Return(Unit.Default);
        }



        public IObservable<byte[]> Get(string key)
        {
            return getCache().SelectMany(cache => cache.Get(key))
                        .Catch((KeyNotFoundException ke) => Observable.Return(new byte[0]));
        }



        public IObservable<Unit> Insert(string key, byte[] data)
        {
            return getCache().SelectMany(cache => cache.Insert(key, data));
        }


        IObservable<IBlobCache> getCache()
        {
            return _getCache.Value;
        }

        public IObservable<IEnumerable<T>> GetAllObjects<T>()
        {
            return getCache().SelectMany(cache => cache.GetAllObjects<T>());
        }

        public IObservable<T> GetAndFetchLatest<T>(string key, Func<IObservable<T>> fetchFunc, Func<DateTimeOffset, bool> fetchPredicate = null, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?), bool shouldInvalidateOnError = false)
        {
            return getCache().SelectMany(cache => cache.GetAndFetchLatest<T>(key, fetchFunc, fetchPredicate, absoluteExpiration, shouldInvalidateOnError));
        }

        public IObservable<T> GetAndFetchLatest<T>(string key, Func<Task<T>> fetchFunc, Func<DateTimeOffset, bool> fetchPredicate = null, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.GetAndFetchLatest<T>(key, fetchFunc, fetchPredicate, absoluteExpiration));
        }

        public IObservable<T> GetObject<T>(string key)
        {
            return getCache()
                .SelectMany(cache => cache.GetObject<T>(key));
        }

        public IObservable<DateTimeOffset?> GetObjectCreatedAt<T>(string key)
        {
            return getCache().SelectMany(cache => cache.GetObjectCreatedAt<DateTimeOffset?>(key));
        }

        public IObservable<T> GetOrCreateObject<T>(string key, Func<T> fetchFunc, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.GetOrCreateObject<T>(key, fetchFunc, absoluteExpiration));
        }

        public IObservable<T> GetOrFetchObject<T>(string key, Func<IObservable<T>> fetchFunc, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.GetOrFetchObject<T>(key, fetchFunc, absoluteExpiration));
        }

        public IObservable<T> GetOrFetchObject<T>(string key, Func<Task<T>> fetchFunc, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.GetOrFetchObject<T>(key, fetchFunc, absoluteExpiration));
        }

        public IObservable<Unit> InsertAllObjects<T>(IDictionary<string, T> keyValuePairs, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.InsertAllObjects<T>(keyValuePairs, absoluteExpiration));
        }

        public IObservable<Unit> InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return getCache().SelectMany(cache => cache.InsertObject<T>(key, value, absoluteExpiration));
        }

        public IObservable<Unit> InvalidateAllObjects<T>()
        {
            return getCache().SelectMany(cache => cache.InvalidateAllObjects<T>());
        }
        public IObservable<Unit> InvalidateObject<T>(string key)
        {
            return getCache().SelectMany(cache => cache.InvalidateObject<T>(key));
        }
        public IObservable<Unit> InvalidateObjects<T>(IEnumerable<string> keys)
        {
            return getCache().SelectMany(cache => cache.InvalidateObjects<T>(keys));
        }

        public IObservable<Unit> Invalidate(string key)
        {
            return getCache().SelectMany(cache => cache.Invalidate(key));
        }
        public IObservable<Unit> Invalidate(IEnumerable<string> keys)
        {
            return getCache().SelectMany(cache => cache.Invalidate(keys));
        }

        public IObservable<Unit> InvalidateAll()
        {
            return getCache().SelectMany(cache => cache.InvalidateAll());
        }
    }
}
