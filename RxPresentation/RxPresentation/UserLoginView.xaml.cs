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
                new UserLoginViewModelRxUI(TaskPoolScheduler.Default, XamarinDispatcherScheduler.Current);
             

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
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