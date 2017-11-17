
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace DevDays
{
    public static class Helpers
    {

        public static IObservable<T> DoLog<T>(this IObservable<T>  This, string whoAmI)
        {
            return 
                This.Do((data) =>
                {
                    Debug.WriteLine($"Event-{whoAmI}: {data}");
                },
                (e) =>
                {
                    Debug.WriteLine($"Exception-{whoAmI}: {e}");
                },
                () =>
                {
                    Debug.WriteLine($"Completed-{whoAmI}");
                });
        }


    
        public static IObservable<T> DelayLabelClear<T>(this IObservable<T> This, TextBlock view)
        {
            return 
                This.SelectMany(result => Observable.Timer(TimeSpan.FromSeconds(1)).Select(_ => result))
                .ObserveOnDispatcher()
                .Do(__ => view.Text = "");
        }

        public static IObservable<Unit> ToUnit<T>(this IObservable<T> This)
        {
            return
                This.Select(_ => Unit.Default);
        }

        public static IObservable<bool> IsTrue(this IObservable<bool> This)
        {
            return
                This.Where(x => x);
        }

        public static IObservable<bool> IsFalse(this IObservable<bool> This)
        {
            return
                This.Where(x => !x);
        }
    }
}
