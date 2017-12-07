using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;

namespace DevDays.Android
{
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private static MainApplication _instance;
        private readonly AndroidCompositionRoot _compositionRoot;

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
            : base(handle, transer)
        {
            _instance = this;
            this._compositionRoot = new AndroidCompositionRoot();
        }

        public override void OnCreate()
        {

            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
        }


        public static MainApplication Instance => _instance;
        public AndroidCompositionRoot CompositionRoot => _compositionRoot;


        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }
        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            
        }

        public void OnActivityDestroyed(Activity activity)
        {
            
        }

        public void OnActivityPaused(Activity activity)
        {
            
        }

        public void OnActivityResumed(Activity activity)
        {

            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;

        }

        public void OnActivityStopped(Activity activity)
        {
            
        }
    }
}