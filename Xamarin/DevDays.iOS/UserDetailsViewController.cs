using Foundation;
using System;
using DevDays.ViewModels;
using ReactiveUI;
using UIKit;

namespace DevDays.iOS
{
    public partial class UserDetailsViewController : ReactiveViewController<UserDetailsViewModel>
    {
        public UserDetailsViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }
    }
}