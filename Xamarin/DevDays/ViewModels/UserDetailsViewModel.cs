using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Runtime.Serialization;
using DevDays.Models;
using DevDays.Services;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace DevDays.ViewModels
{

    public class UserDetailsViewModel : ViewModelBase
    {
        private UserModel _model;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        readonly IScheduler _uiScheduler;
        string _password;
        ObservableAsPropertyHelper<bool> _showPasswordIsInvalid;
        ObservableAsPropertyHelper<bool> _enableSaveButton;
        ObservableAsPropertyHelper<bool> _userNameIsValid;
        ObservableAsPropertyHelper<bool> _UserNameIsUnique;

        private bool _addingNewUser;
        

        public UserDetailsViewModel(
            UserModel model,
            IUserService userService,
            INavigationService navigationService,
            IScheduler uiScheduler)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _uiScheduler = uiScheduler ?? throw new ArgumentNullException(nameof(uiScheduler));
            _model = model ?? throw new ArgumentNullException(nameof(model));


            _addingNewUser = String.IsNullOrWhiteSpace(_model.Id);


            SetupUserNameValidations();
            SetupPasswordValidations();


            var canSaveObs =
                this.WhenAnyValue
                (
                    x => x.ShowPasswordIsInvalid,
                    x => x.UserNameIsValid
                )
                .Select(items =>
                {
                    var returnValue = !items.Item1 && items.Item2;
                    return returnValue;
                });

            canSaveObs
                .ToProperty(this, x => x.EnableSaveButton, out _enableSaveButton, scheduler: uiScheduler)
                .DisposeWith(disposable);


            SaveUser = ReactiveCommand.CreateFromObservable(SaveDetails, canExecute: canSaveObs.ObserveOn(uiScheduler));

            GoBackWithResult = ReactiveCommand.Create<UserModel, UserModel>(m => m);
            Edit = ReactiveCommand.CreateFromObservable(() => _navigationService.UserDetailsView(_model));
        }


        void SetupUserNameValidations()
        {
            this.WhenAnyValue(x => x.UserName)
                .Select(username => !String.IsNullOrWhiteSpace(username))
                .ToProperty(this, x => x.UserNameIsValid, out _userNameIsValid, scheduler: _uiScheduler)
                .DisposeWith(disposable);


            this.WhenAnyValue(x => x.UserName)
                .SelectMany(userName => _userService.GetUserId(userName))
                .Select(userId => String.IsNullOrWhiteSpace(userId) || userId == Id)
                .ToProperty(this, x => x.UserNameIsUnique, out _UserNameIsUnique, scheduler: _uiScheduler)
                .DisposeWith(disposable);
        }

        void SetupPasswordValidations()
        {
            this.WhenAnyValue(x => x.Password)
               .Select(password =>
               {
                   if (String.IsNullOrWhiteSpace(password))
                   {
                       return _addingNewUser;
                   }

                   return !_userService.ValidatePassword(password);
               })
               .ToProperty(this, x => x.ShowPasswordIsInvalid, out _showPasswordIsInvalid, scheduler: _uiScheduler)
               .DisposeWith(disposable);
        }


        public IObservable<UserModel> SaveDetails()
        {
            return _userService
                        .SaveUser(_model, Password)
                        .SelectMany(GoBackWithResult.Execute);
        }

        public bool UserNameIsUnique => _UserNameIsUnique.Value;
        public bool UserNameIsValid => _userNameIsValid.Value;
        public bool EnableSaveButton => _enableSaveButton.Value;
        public bool ShowPasswordIsInvalid => _showPasswordIsInvalid.Value;

        public ReactiveCommand<Unit, UserModel> SaveUser { get; }
        public ReactiveCommand<UserModel, UserModel> GoBackWithResult { get; }
        public ReactiveCommand<Unit, Unit> Edit { get; }


        public string Id
        {
            get => _model.Id;
        }


        public UserModel Model => _model;
        public byte[] UserImage
        {
            get => _model.UserImage;
            set
            {
                _model.UserImage = value;
                this.RaisePropertyChanged(nameof(UserImage));
            }
        }


        public string UserName
        {
            get => _model.UserName;
            set
            {
                _model.UserName = value;
                this.RaisePropertyChanged(nameof(UserName));
            }
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }


        public void Update(UserModel model)
        {
            _model = model;
        }
    }
}
