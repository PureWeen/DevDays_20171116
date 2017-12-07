using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using ReactiveUI.Android;
using ReactiveUI.AndroidSupport;
using DevDays.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace DevDays.Android
{
    public abstract class BaseActivity<TViewModel> : ReactiveAppCompatActivity<TViewModel>
         where TViewModel : class
    {
        protected CompositeDisposable disposable;
        readonly int _viewId;

        protected AndroidCompositionRoot CompositionRoot => MainApplication.Instance.CompositionRoot;

        public BaseActivity(int viewId)
        {
            disposable = new CompositeDisposable();
            _viewId = viewId;
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(_viewId);

            this.WireUpControls();
            Setup(disposable);
        }

        protected override void OnDestroy()
        {
            disposable.Dispose();
            base.OnDestroy();
        }

        protected abstract void Setup(CompositeDisposable disposable);
    }
}

