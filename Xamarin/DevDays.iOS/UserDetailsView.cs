using Foundation;
using System;
using UIKit;
using ReactiveUI;
using DevDays.ViewModels;

namespace DevDays.iOS
{
    public partial class UserDetailsView : ReactiveView<UserDetailsViewModel>
    {
        public UserDetailsView(IntPtr handle) : base(handle)
        {
            
        }
 
    }
}