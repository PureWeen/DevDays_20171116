using Foundation;
using System;
using DevDays.ViewModels;
using ReactiveUI;
using UIKit;
using System.Reactive.Disposables;
using System.Linq;

namespace DevDays.iOS
{
    public partial class UserDetailsViewController : ReactiveViewController<UserDetailsViewModel>
    {

        CompositeDisposable disposable = new CompositeDisposable();
        IViewFor<UserDetailsViewModel> UserDetailsView
        {
            get => (IViewFor<UserDetailsViewModel>)View;
            set => View = (UIView)value;
            
        }
        public UIBarButtonItem save { get; private set; }

        public UserDetailsViewController (IntPtr handle) : base (handle)
        { 
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Password.SecureTextEntry = true;
            this.OneWayBind(ViewModel, x => x, x => x.UserDetailsView.ViewModel)
                .DisposeWith(disposable);
            
            this.OneWayBind(ViewModel, x => x.UserName, x => x.Title)
                .DisposeWith(disposable);
             
            this.WhenAnyObservable(v => v.ViewModel.GoBackWithResult)
                .Subscribe(results =>
                {
                    NavigationController
                        .ChildViewControllers
                        .OfType<UserListViewController>()
                        .First()
                        .ViewModel
                        .AddOrUpdate(results);

                    NavigationController.PopViewController(true);
                })
                .DisposeWith(disposable);


            save = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationItem.RightBarButtonItem = save;

            this.Bind(ViewModel, x => x.UserName, x => x.UserName.Text)
                .DisposeWith(disposable);
            
            this.BindCommand(ViewModel, x => x.SaveUser, x => x.save)
                .DisposeWith(disposable);

            this.Bind(ViewModel, x=> x.Password, x=> x.Password.Text)
                .DisposeWith(disposable);


        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            disposable.Dispose();
        }
    }
}