using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xam.Reactive.Concurrency;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace RxPresentation
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UserLoginView : ContentPage
	{
        CompositeDisposable disposable = new CompositeDisposable();
		public UserLoginView ()
		{
			InitializeComponent ();

            // all hail the notch
            this.On<iOS>().SetUseSafeArea(true);

            this.BindingContext = 
                new UserLoginViewModelReactive(TaskPoolScheduler.Default, XamarinDispatcherScheduler.Current);

            Observable.FromEventPattern<EventHandler<TextChangedEventArgs>, TextChangedEventArgs>
                (
                    x => tbUserName.TextChanged += x,
                    x => tbUserName.TextChanged -= x
                );
		}


        protected override void OnAppearing()
        {
            base.OnAppearing();


            var gesture = new PanGestureRecognizer();
            
            int touchCount = 0;
            gesture
                .Events()
                .PanUpdated
                .GroupBy(x=> x.GestureId)
                .SelectMany(groups =>
                {
                    var closureCount = Interlocked.Increment(ref touchCount);
                    var refCount = groups.Publish().RefCount();
                    return refCount
                            .TakeUntil(refCount.Where(x => x.StatusType == GestureStatus.Completed || x.StatusType == GestureStatus.Canceled))
                            .Select(result => new { TouchNumber = closureCount, result })
                            .Finally(() => touchCount = Interlocked.Decrement(ref touchCount));
                   
                })
                .Subscribe(data =>
                {
                    var thing = data.result;
                    Debug.WriteLine($"{data.TouchNumber} {thing.GestureId} {thing.StatusType} {thing.TotalX} {thing.TotalY}");
                })
                .DisposeWith(disposable);


            mainLayout.GestureRecognizers.Add(gesture);
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            disposable.Clear();
        }


        #region System.Reactive
        #endregion

        #region ReactiveUI
        #endregion

    }
}