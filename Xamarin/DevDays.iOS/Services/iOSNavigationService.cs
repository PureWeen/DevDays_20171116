using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using DevDays.Services;
using System.Reactive.Linq;
using DevDays.Models;
using Newtonsoft.Json;
using UIKit;

namespace DevDays.iOS.Services
{
    public class iOSNavigationService : INavigationService
    {

        
        public IObservable<Unit> UserDetailsView()
        {
            return UserDetailsView(new UserModel());
        }


        public IObservable<Unit> UserDetailsView(UserModel model)
        {
            UIStoryboard board = UIStoryboard.FromName ("Main", null);
            UserDetailsViewController ctrl = (UserDetailsViewController)board.InstantiateViewController ("UserDetailsView");
            UIApplication.SharedApplication.KeyWindow.RootViewController.NavigationController.PushViewController (ctrl, true);
            ctrl.ViewModel = DevDays.iOS.Application.CompositionRoot.CreateUserDetailsViewModel(model.Clone());
            return Observable.Return(Unit.Default);
        }
    }
}