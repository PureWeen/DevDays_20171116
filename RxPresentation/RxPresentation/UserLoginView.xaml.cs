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
            
            // complete event bug :-(
            int touchCount = 0;
            gesture
                .Events()
                .PanUpdated
                .GroupBy(x=> x.GestureId)
                .SelectMany(groups =>
                {
                    var closureCount = Interlocked.Increment(ref touchCount);
                    var refCount = groups.Publish().RefCount();
                    var startingBounds = absoluteBox.Bounds;

                    return refCount
                            .TakeUntil(refCount.Where(x => x.StatusType == GestureStatus.Completed || x.StatusType == GestureStatus.Canceled))
                            .Select(result => new { Bounds = startingBounds, TouchNumber = closureCount, result })
                            .Finally(() => Interlocked.Decrement(ref touchCount));
                   
                })        
                .ObserveOn(XamarinDispatcherScheduler.Current)
                .DoLog("test")
                .Subscribe(data =>
                {
                    if(data.TouchNumber == 1)
                    {
                        AbsoluteLayout.SetLayoutBounds(absoluteBox, 
                            new Rectangle(data.Bounds.X + data.result.TotalX, data.Bounds.Y + data.result.TotalY, absoluteBox.Bounds.Width, absoluteBox.Bounds.Height));
                    }
                    else if(data.TouchNumber == 2)
                    {
                        AbsoluteLayout.SetLayoutBounds(absoluteBox,
                            new Rectangle(absoluteBox.X, absoluteBox.Y, data.Bounds.Width + data.result.TotalX, data.Bounds.Height + data.result.TotalY));

                    }
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