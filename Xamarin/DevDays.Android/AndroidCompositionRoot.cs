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
using DevDays.Services;
using DevDays.Android.Services;

namespace DevDays.Android
{
    public class AndroidCompositionRoot : CompositionRoot
    {
        protected override INavigationService CreateNavigationService()
        {
            return new AndroidNavigationService();
        }

        protected override ISecureStorageService CreateSecureStorageService()
        {
            return new SecureStorageService();
        }
    }
}