using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DevDays.Services;
using System.Reactive.Linq;
using Plugin.CurrentActivity;
using Android.Content.Res;
using DevDays.Models;
using Newtonsoft.Json;

namespace DevDays.Android.Services
{
    public class AndroidNavigationService : INavigationService
    {
        Activity CurrentActivity => 
            CrossCurrentActivity.Current.Activity;

        public IObservable<Unit> UserDetailsView()
        {
            return UserDetailsView(new UserModel());
        }


        public IObservable<Unit> UserDetailsView(UserModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var intent = new Intent(CurrentActivity, typeof(UserDetailsActivity));
            intent.PutExtra(CurrentActivity.Resources.GetString(Resource.String.UserDetailsIntentKey), json);
            CurrentActivity.StartActivityForResult(intent, 1);
            return Observable.Return(Unit.Default);
        }
    }
}