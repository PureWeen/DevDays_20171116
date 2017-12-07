using Android.App;
using Android.OS;
using ReactiveUI.AndroidSupport;
using DevDays.ViewModels;
using Android.Support.V7.Widget;
using Android.Views;
using ReactiveUI.Android.Support;
using System.Collections.ObjectModel;
using Android.Widget;
using ReactiveUI;
using System.Reactive.Disposables;
using Android.Content;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.IO;
using FFImageLoading;
using FFImageLoading.Views;

namespace DevDays.Android.Adapters
{

    public class UserListAdapter : DynamicDataRecyclerViewAdapter<UserDetailsViewModel>
    {
        public UserListAdapter(ReadOnlyObservableCollection<UserDetailsViewModel> backingList) : base(backingList)
        {
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = 
                LayoutInflater
                    .From(parent.Context)
                    .Inflate(Resource.Layout.UserRow, parent, false);


            UserViewHolder vh = new UserViewHolder(itemView);
            return vh;
        }

        public class UserViewHolder : ReactiveRecyclerViewViewHolder<UserDetailsViewModel>
        {
            CompositeDisposable _disposable;
            public TextView UserNameUserRow { get; set; }
            public ImageViewAsync UserImageUserName { get; set; }


            public UserViewHolder(View view) : base(view)
            {
                this.WireUpControls();
                _disposable = new CompositeDisposable();

                this.Bind(ViewModel, vm => vm.UserName, v => v.UserNameUserRow.Text)
                    .DisposeWith(_disposable);


                this.Selected
                    .ToSignal()
                    .InvokeCommand(this, v => v.ViewModel.Edit)
                    .DisposeWith(_disposable);


                this.WhenAnyValue(x => x.ViewModel.UserImage)
                  .Subscribe(bytes =>
                  {

                      if(bytes == null || bytes.Length == 0)
                      {
                          UserImageUserName.SetImageResource(0);
                      }
                      else
                      {
                          Func<CancellationToken, Task<Stream>> processMe =
                              (ct) =>
                              {
                                  return Task.FromResult((System.IO.Stream)new MemoryStream(bytes ?? new byte[0]));
                              };


                          ImageService.Instance
                              .LoadStream(processMe)
                              .DownSample(60, 60)
                              .Into(UserImageUserName);
                      }

                  })
                  .DisposeWith(_disposable);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _disposable.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }

}

