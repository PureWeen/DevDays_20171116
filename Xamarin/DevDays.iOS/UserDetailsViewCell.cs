using Foundation;
using System;
using ReactiveUI;
using DevDays.ViewModels;
using UIKit;

namespace DevDays.iOS
{
    public partial class UserDetailsViewCell : ReactiveTableViewCell<UserDetailsViewModel>
    {
        static public NSString CellId = new NSString("UserDetailsRow");
        public UserDetailsViewCell()
        {
        }
        public UserDetailsViewCell (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            this.OneWayBind(ViewModel, vm => vm.UserName, v => v.lblUserName.Text);
        }

    }  
}