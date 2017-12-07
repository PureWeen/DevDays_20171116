using Android.App;
using Android.OS;
using ReactiveUI.AndroidSupport;
using DevDays.ViewModels;
using Android.Support.V7.Widget;
using Android.Views;
using ReactiveUI.Android.Support;
using Android.Support.Design.Widget;
using DevDays.Android.Adapters;
using System.Reactive.Disposables;
using System;
using Android.Content;
using ReactiveUI;
using Newtonsoft.Json;
using DevDays.Models;
using Android.Support.V7.Widget.Helper;

namespace DevDays.Android
{
    [Activity(Label = "DevDays Users", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class UserListActivity : BaseActivity<UserListViewModel>
    {
        public FloatingActionButton AddNewUserButton { get; set; }
        public RecyclerView UserListRecyclerView { get; set; }
        public global::Android.Support.V7.Widget.Toolbar UserListToolBar { get; set; }


        public UserListActivity() : base(Resource.Layout.UserList)
        {
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 1)
            {
                if (resultCode == Result.Ok)
                {
                    var json = data.GetStringExtra(GetString(Resource.String.UserDetailsIntentKey));
                    var updatedModel = JsonConvert.DeserializeObject<UserModel>(json);
                    ViewModel.AddOrUpdate(updatedModel);
                }
            }
        }

        protected override void Setup(CompositeDisposable disposable)
        {
            SetSupportActionBar(UserListToolBar);


            ViewModel =
                MainApplication
                    .Instance
                    .CompositionRoot
                    .CreateUserListViewModel();

            UserListAdapter adapter = new UserListAdapter(ViewModel.VisibleUsers);
            UserListRecyclerView.SetAdapter(adapter);
            UserListRecyclerView.HasFixedSize = true;
            UserListRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
            UserListRecyclerView.AddItemDecoration(new DividerItemDecoration(this,
                DividerItemDecoration.Vertical));

            ItemTouchHelper itemTouchHelper = new ItemTouchHelper(new SimpleItemTouchHelperCallback(ViewModel));
            itemTouchHelper.AttachToRecyclerView(UserListRecyclerView);


            ViewModel.LoadList();

            adapter
                .DisposeWith(disposable);


            AddNewUserButton
                 .Events()
                 .Click
                 .ToSignal()
                 .InvokeCommand(this, v => v.ViewModel.AddNew)
                 .DisposeWith(disposable);
        }

        public class SimpleItemTouchHelperCallback : ItemTouchHelper.Callback
        {
            private UserListViewModel viewModel;


            public SimpleItemTouchHelperCallback(UserListViewModel viewModel)
            {
                this.viewModel = viewModel;
            }

            public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            {
                int dragFlags = 0;
                int swipeFlags = ItemTouchHelper.Left | ItemTouchHelper.Right;

                return MakeMovementFlags(dragFlags, swipeFlags);
            }

            public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
            {
                return false;
            }

            public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
            {
                viewModel.Delete((viewHolder as UserListAdapter.UserViewHolder).ViewModel);
            }
        }
    }

}

