// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace DevDays.iOS
{
    [Register ("UserListViewController")]
    partial class UserListViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView UserListTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (UserListTableView != null) {
                UserListTableView.Dispose ();
                UserListTableView = null;
            }
        }
    }
}