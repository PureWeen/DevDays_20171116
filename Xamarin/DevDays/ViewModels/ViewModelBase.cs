using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace DevDays.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        protected CompositeDisposable disposable = new CompositeDisposable();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposable.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
