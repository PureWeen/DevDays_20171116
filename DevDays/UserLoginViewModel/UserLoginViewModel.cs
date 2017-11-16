using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace DevDays
{
    public class UserLoginViewModel : ReactiveObject
    {

        IScheduler _backgroundScheduler;
        IScheduler _uiScheduler;
        public UserLoginViewModel(IScheduler backgroundScheduler, IScheduler uiScheduler)
        {
            _backgroundScheduler = backgroundScheduler;
            _uiScheduler = uiScheduler;

            SetupPasswordObervables();
            SetupValidationLogic();
        }


        #region password
        void SetupPasswordObervables()
        {
            _isPasswordValid =
                this.WhenAnyValue(x => x.Password)
                    .Select(password => !String.IsNullOrWhiteSpace(password) && !password.Contains("InvalidPassword"))
                    .ToProperty(this, x => x.IsPasswordValid, scheduler: _uiScheduler, initialValue:false);


            _HaveIBeenPwnedData =
                this.WhenAnyValue(x => x.Password, x=> x.IsPasswordValid)
                   .Select(e => new { password = e.Item1, isValid = e.Item2  })
                   .Where(e => e.isValid) //check validation
                   .Select(e => e.password) //select the password
                   .Throttle(TimeSpan.FromMilliseconds(500), scheduler: _backgroundScheduler)
                   .Select(password => HaveIBeenPwnedFakeServerCall(password).TakeUntil(this.WhenAnyValue(x => x.Password).Skip(1)))
                   .Switch()
                   .ToProperty(this, x => x.HaveIBeenPwnedData, scheduler: _uiScheduler, initialValue: "initialValue");

        }


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


        #region validate Passwords



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
                    .ToProperty(this, x=> x.UserNameIsValid, scheduler:_uiScheduler);


            _isEverythingValid =
                this.WhenAnyValue(x => x.HaveIBeenPwnedData, x => x.UserNameIsValid, x=> x.IsPasswordValid)
                    .Select(i => new { pwnedResult = i.Item1, isValid = i.Item2 && i.Item3  })
                    .Select(result => result.isValid && result.pwnedResult == "not pwned")
                    .ToProperty(this, x=> x.UserNameIsValid, scheduler: _uiScheduler);
                
        }
        #endregion

    }
}
