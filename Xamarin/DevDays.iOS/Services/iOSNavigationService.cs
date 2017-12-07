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
            GetNavigationController().PushViewController (ctrl, true);
            ctrl.ViewModel = DevDays.iOS.Application.CompositionRoot.CreateUserDetailsViewModel(model.Clone());
            return Observable.Return(Unit.Default);
        }

        UIViewController GetPresentedController()
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var vc = window.RootViewController;
            while (vc.PresentedViewController != null)
                vc = vc.PresentedViewController;

            return vc;
        }

        UINavigationController GetNavigationController()
        {
            var vc = GetPresentedController();
            var navController = vc as UINavigationController;
            if (navController != null)
                vc = navController.ViewControllers.Last();

            return navController;
        }
    }
}