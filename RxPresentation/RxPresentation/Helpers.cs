
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxPresentation
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
                })
                .Finally(() =>
                {
                    Debug.WriteLine($"Finally-{whoAmI}");
                });
        }
    }
}
