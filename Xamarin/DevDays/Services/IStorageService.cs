using System;
using System.Collections.Generic;
using System.Reactive;

namespace DevDays.Services
{
    public interface IStorageService
    {
        IObservable<IEnumerable<T>> GetDataOfType<T>();
        IObservable<T> GetData<T>(string key);
        IObservable<Unit> StoreData<T>(string key, T data);
        IObservable<Unit> DeleteData<T>(string id);

        IObservable<Unit> Insert(string key, byte[] data);

        IObservable<byte[]> Get(string key);
    }
}