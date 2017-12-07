using Android.App;
using Android.OS;
using Android.Support.V7.App;
using ReactiveUI.Android;
using ReactiveUI.AndroidSupport;
using DevDays.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using Android.Views;
using Android.Widget;
using Android.Content;
using Newtonsoft.Json;
using DevDays.Models;
using Android.Support.Design.Widget;
using FFImageLoading;
using FFImageLoading.Views;
using Android.Provider;
using Java.IO;
using Android.Graphics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Android.Support.V7.Widget;

namespace DevDays.Android
{
    [Activity(Label = "Welcome")]
    public class UserDetailsActivity : BaseActivity<UserDetailsViewModel>
    { 
        public TextInputLayout UserNameTextInputLayout { get; set; }
        public TextInputEditText UserName { get; set; }
        public AppCompatButton UpdateUser { get; set; }
        public global::Android.Support.V7.Widget.Toolbar UserDetailsToolBar { get; set; }
        public ImageViewAsync ToolBarImage { get; set; }
        public TextInputEditText Password { get; set; }
        public TextInputLayout PasswordTextInputLayout { get; set; }

        public CollapsingToolbarLayout UserDetailsCollapsingToolBar { get; set; }
        public FloatingActionButton AddNewImageButton { get; set; }


        public UserDetailsActivity() : base(Resource.Layout.UserDetails)
        {
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void Setup(CompositeDisposable disposable)
        {
            SetSupportActionBar(UserDetailsToolBar);

            var json = Intent.GetStringExtra(GetString(Resource.String.UserDetailsIntentKey));
            if(!String.IsNullOrWhiteSpace(json))
            {
                ViewModel = CompositionRoot.CreateUserDetailsViewModel(JsonConvert.DeserializeObject<UserModel>(json));
            }
            else
            {
                ViewModel = CompositionRoot.CreateUserDetailsViewModel(null);
            }
            

            this.OneWayBind(ViewModel, 
                    vm => vm.UserName, 
                    v=> v.UserDetailsCollapsingToolBar.Title, 
                    txt => !String.IsNullOrWhiteSpace(txt) ? txt : "Welcome")
                .DisposeWith(disposable);


            this.Bind(ViewModel, vm => vm.UserName, v => v.UserName.Text)
                .DisposeWith(disposable);

            this.WhenAnyValue(v=> v.ViewModel.UserNameIsUnique, v=> v.ViewModel.UserNameIsValid)
                .Subscribe(items=>
                {
                    if(!items.Item1)
                    {
                        UserNameTextInputLayout.Error = GetString(Resource.String.UserNameIsUnique);
                        return;
                    }

                    if (!items.Item2)
                    {
                        UserNameTextInputLayout.Error = GetString(Resource.String.UserNameIsInvalid);
                        return;
                    }

                    UserNameTextInputLayout.Error = String.Empty;
                })
                .DisposeWith(disposable);




            this.Bind(ViewModel, vm => vm.Password, v => v.Password.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel, vm => vm.ShowPasswordIsInvalid, v => v.PasswordTextInputLayout.Error,
                result => result ? GetString(Resource.String.PasswordIsInvalid) : String.Empty)
                .DisposeWith(disposable);




            this.WhenAnyValue(x => x.ViewModel.UserImage)
                .Subscribe(bytes =>
                {
                    if (bytes == null || bytes.Length == 0)
                    {
                        ToolBarImage.SetImageResource(0);
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
                            .Into(ToolBarImage);
                    }

                })
                .DisposeWith(disposable);

          




            this.OneWayBind(ViewModel, vm => vm.EnableSaveButton, v => v.UpdateUser.Enabled)
                .DisposeWith(disposable);

            this.BindCommand(ViewModel, vm => vm.SaveUser, v => v.UpdateUser)                
                .DisposeWith(disposable); 


            this.WhenAnyObservable(v=> v.ViewModel.GoBackWithResult)
                .Subscribe(results =>
                {
                    Intent intent = new Intent();
                    intent.PutExtra(
                        Resources.GetString(Resource.String.UserDetailsIntentKey), 
                        JsonConvert.SerializeObject(results)
                    );

                    SetResult(Result.Ok, intent);
                    OnBackPressed();
                })
                .DisposeWith(disposable);


            this.AddNewImageButton
                .Events()
                .Click.Subscribe(_ =>
                {
                    var imageIntent = new Intent();
                    imageIntent.SetType("image/*");
                    imageIntent.SetAction(Intent.ActionGetContent);
                    StartActivityForResult(
                        Intent.CreateChooser(imageIntent, "Select photo"), 0);
                });

            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar?.SetHomeButtonEnabled(true);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {

                using (System.IO.Stream stream = new MemoryStream())
                {
                    var bitMap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, data.Data);

                    if(bitMap.Height > 100 || bitMap.Width > 100)
                    {
                        bitMap = Bitmap.CreateScaledBitmap(bitMap, 100, 100, true);
                    }

                    bitMap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    ViewModel.UserImage = (stream as MemoryStream).ToArray();

                }

            }
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString(
                       Resources.GetString(Resource.String.UserDetailsIntentKey),
                       JsonConvert.SerializeObject(ViewModel.Model)
                   );
        }


        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            var json = savedInstanceState.GetString(GetString(Resource.String.UserDetailsIntentKey));

            if(!String.IsNullOrWhiteSpace(json) && ViewModel != null)
            {
                var model = JsonConvert.DeserializeObject<UserModel>(json);

                ViewModel.UserName = model.UserName;
                ViewModel.UserImage = model.UserImage;
                
            }
            

        }


        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

    }
}

