using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;

namespace RxPresentation
{
    public class UserLoginViewModelRxUIComplex : ReactiveObject, IDisposable
    {

        IScheduler _backgroundScheduler;
        IScheduler _uiScheduler;
        CompositeDisposable disposable = new CompositeDisposable();

        public UserLoginViewModelRxUIComplex(IScheduler backgroundScheduler, IScheduler uiScheduler)
        {
            _backgroundScheduler = backgroundScheduler;
            _uiScheduler = uiScheduler;

            SetupPasswordObervables();
            SetupValidationLogic();
            SetupCommands();
        }


        #region password
        void SetupPasswordObervables()
        {
            _isPasswordValid =
                this.WhenAnyValue(x => x.Password)
                    .Select(password => !String.IsNullOrWhiteSpace(password) && !password.Contains("InvalidPassword"))
                    .ToProperty(this, x => x.IsPasswordValid, scheduler: _uiScheduler, initialValue:false)
                    .DisposeWith(disposable);



            var resetHaveIBeenPwned =
                PasswordChanged.Select(_ => "initialValue");

            _HaveIBeenPwnedData =
                PasswordChangedAndIsValid
                   .Throttle(TimeSpan.FromMilliseconds(500), scheduler: _backgroundScheduler)
                   .Select(password => HaveIBeenPwnedFakeServerCall(password).TakeUntil(PasswordChanged.Skip(1)))
                   .Switch()
                   .Merge(resetHaveIBeenPwned)
                   .ToProperty(this, x => x.HaveIBeenPwnedData, scheduler: _uiScheduler, initialValue: "initialValue")
                   .DisposeWith(disposable);

        }

        public IObservable<string> PasswordChanged =>
            this.WhenAnyValue(x => x.Password);


        public IObservable<string> PasswordChangedAndIsValid =>
           this.WhenAnyValue(x => x.Password, x => x.IsPasswordValid)
                   .Select(e => new { password = e.Item1, isValid = e.Item2 })
                   .Where(e => e.isValid) //check validation
                   .Select(e => e.password); //select the password


        string _Password;
        public string Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }


        ObservableAsPropertyHelper<string> _HaveIBeenPwnedData;
        public string HaveIBeenPwnedData
        {
            get
            {
                return _HaveIBeenPwnedData.Value;
            }
        }

        ObservableAsPropertyHelper<bool> _isPasswordValid;
        public bool IsPasswordValid
        {
            get
            {
                return _isPasswordValid.Value;
            }
        }



        public IObservable<string> HaveIBeenPwnedFakeServerCall(string data)
        {
            return
                    Observable
                        .Timer(TimeSpan.FromMilliseconds(1000), scheduler: _backgroundScheduler)
                        .Select(_ => data == "password" ? "pwned" : "not pwned");
        }
        #endregion

        #region validate UserNames



        string _UserName;
        public string UserName
        {
            get => _UserName;
            set => this.RaiseAndSetIfChanged(ref _UserName, value);
        }



        ObservableAsPropertyHelper<bool> _UserNameIsValid;
        public bool UserNameIsValid
        {
            get
            {
                return _UserNameIsValid.Value;
            }
        }

        ObservableAsPropertyHelper<bool> _isEverythingValid;
        public bool IsEverythingValid
        {
            get
            {
                return _isEverythingValid.Value;
            }
        }

        void SetupValidationLogic()
        {
            _UserNameIsValid =
                this.WhenAnyValue(x => x.UserName)
                    .Select(userName => UserName == "goodusername")
                    .ToProperty(this, x=> x.UserNameIsValid, scheduler:_uiScheduler)
                    .DisposeWith(disposable);

            // Immutable Expectations
            _isEverythingValid =
                this.WhenAnyValue
                (
                    x => x.HaveIBeenPwnedData, 
                    x => x.UserNameIsValid, 
                    x => x.IsPasswordValid
                )
                .Select(i => new { pwnedResult = i.Item1, isValid = i.Item2 && i.Item3 })
                .Select(result => result.isValid && result.pwnedResult == "not pwned")
                .ToProperty(this, x => x.UserNameIsValid, scheduler: _uiScheduler)
                .DisposeWith(disposable);
                
        }
        #endregion

        #region some Commands
        ReactiveCommand<Unit, bool> _LogTheUserIn;


        public ReactiveCommand<Unit, bool> LogTheUserIn => _LogTheUserIn;  
        private void SetupCommands()
        {
            _LogTheUserIn = 
                ReactiveCommand.CreateFromObservable(
                    () =>  Observable.Timer(TimeSpan.FromMilliseconds(2000), _backgroundScheduler)
                            .TakeUntil(PasswordChanged.Skip(1))
                            .Select(_ => true),
                    canExecute: this.WhenAnyValue(x=> x.UserNameIsValid),
                    outputScheduler: _uiScheduler);
        }
        #endregion



        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
