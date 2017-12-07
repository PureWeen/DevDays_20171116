using Foundation;
using System;
using UIKit;
using ReactiveUI;
using DevDays.ViewModels;
using DynamicData.Binding;
using DynamicData.ReactiveUI;

namespace DevDays.iOS
{
    public partial class UserListViewController : ReactiveViewController<UserListViewModel>
    {
        public UserListViewController (IntPtr handle) : base (handle)
        {
            
        }


        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
        }
        

        UIBarButtonItem add;
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
 
            
            
            
            ViewModel =
                Application
                    .CompositionRoot
                    .CreateUserListViewModel();


            ViewModel.LoadList();

            ReactiveList<UserDetailsViewModel> list = new ReactiveList<UserDetailsViewModel>();

            ViewModel
                .VisibleUsers
                .ToObservableChangeSet()
                .Bind(list)
                .Subscribe();
            
            

            UserListTableView.Source = new UserTableViewSource(
                UserListTableView, 
                list, 
                UserDetailsViewCell.CellId, 
                118);
            
            
            
            add = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            NavigationItem.RightBarButtonItem = add;

            this.BindCommand(ViewModel, x => x.AddNew, x=> x.add);

        }
    }


    public class UserTableViewSource : ReactiveTableViewSource<UserDetailsViewModel>
    {
        public UserTableViewSource(
            UITableView tableView, 
            IReactiveNotifyCollectionChanged<UserDetailsViewModel> collection, 
            NSString cellKey, 
            float sizeHint, 
            Action<UITableViewCell> initializeCellAction = null) : base(tableView, collection, cellKey, sizeHint, initializeCellAction)
        {
        }
    }
}