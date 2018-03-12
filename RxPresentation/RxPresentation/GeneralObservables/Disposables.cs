using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace RxPresentation.GeneralObservables
{
    class Disposables
    {
        public Disposables()
        {
            CompositeDisposable compositeDisposable = new CompositeDisposable();
            compositeDisposable.Clear();
            

            SerialDisposable serialDisposable = new SerialDisposable();
            serialDisposable.Disposable = Disposable.Create(() => { });
            serialDisposable.Disposable = Disposable.Create(() => { });
        }
    }
}
