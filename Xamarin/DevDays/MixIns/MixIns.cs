using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DevDays
{
    public static class Extensions
    {
        public static Lazy<T> GetLazyLocator<T>(this IDependencyResolver This, T constant = null)
          where T : class
        {
            return new Lazy<T>(() => constant ?? This.GetService<T>());
        }

        public static IObservable<T> PermaRef<T>(this IConnectableObservable<T> This, Action<IDisposable> disposeToken = null)
        {
            var token = This.Connect();
            disposeToken?.Invoke(token);

            return This;
        }

        public static IObservable<Unit> ToSignal<T>(this IObservable<T> This)
        {
            return This.Select(_ => Unit.Default);
        }

    }
}
