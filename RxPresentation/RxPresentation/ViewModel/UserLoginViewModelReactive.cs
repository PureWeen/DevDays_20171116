using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;
using Xamarin.Forms;

namespace RxPresentation
{
    public class UserLoginViewModelReactive : INotifyPropertyChanged
    {
        IScheduler _backgroundScheduler;
        IScheduler _uiScheduler;
        CompositeDisposable disposable = new CompositeDisposable();
        string _Password;
        string _UserName;
        private IReadOnlyList<string> _searchTerms;
        SerialDisposable serialDisposable = new SerialDisposable();
        public event PropertyChangedEventHandler PropertyChanged;
        List<string> _words = null;
        private int _notifications;
        private int _filterStarted;
        private string _selectedObservable = nameof(BasicChangeObservable);
        private bool _isValid;

        public UserLoginViewModelReactive(IScheduler backgroundScheduler, IScheduler uiScheduler)
        {
            _backgroundScheduler = backgroundScheduler;
            _uiScheduler = uiScheduler;
            ReadFileIn();
            PasswordChangeObservable();
            SetupObservablePicker();
            LoginCommand = new Command(() => { }, () => IsValid);
        }

        // our Properties that we bind to
        public string Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }

        public string UserName
        {
            get => _UserName;
            set => this.RaiseAndSetIfChanged(ref _UserName, value);
        }

        public int Notifications
        {
            get => _notifications;
            set => this.RaiseAndSetIfChanged(ref _notifications, value);
        }

        public int FilterStarted
        {
            get => _filterStarted;
            set => this.RaiseAndSetIfChanged(ref _filterStarted, value);
        }

        public string SelectedObservable
        {
            get => _selectedObservable;
            set => this.RaiseAndSetIfChanged(ref _selectedObservable, value);
        }

        public IReadOnlyList<string> SearchTerms
        {
            get => _searchTerms;
            set => this.RaiseAndSetIfChanged(ref _searchTerms, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => this.RaiseAndSetIfChanged(ref _isValid, value);
        }
        public Command LoginCommand { get; }

        // our lazy implementation for raising changes when things change
        public void RaiseAndSetIfChanged<T>(ref T input, T newValue, [CallerMemberName]string propertyName = "")
        {
            if (Object.Equals(input, newValue)) return;
            input = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




        Random randomGenerator = new Random();
        //processes inputs
        IObservable<IReadOnlyList<string>> FilterList(IReadOnlyList<string> inputList, string filter)
        {
            FilterStarted++;

            // influence the stream easily
            if (String.IsNullOrWhiteSpace(filter))
                return Observable.Return(new List<string>());

            // delay simulates a lookup or just a general time wait
            return Observable.Timer(TimeSpan.FromSeconds(1), scheduler: _backgroundScheduler)
                .SelectMany(_ =>
                {
                    if (randomGenerator.Next(0, 10) == 3)
                    {
                        return Observable.Throw<List<string>>(new TimeoutException("Internet is down panic"));
                    }

                    return Observable.Return(inputList.Where(word => word.StartsWith(filter)).ToList());
                });
        }

        // handles subscription notification
        void OnHandleInputList(IReadOnlyList<string> inputList)
        {
            Notifications++;
            SearchTerms = inputList;
        }



        // wiring events to notify property changes
        public IObservable<T> GetPropertyChangedObservable<T>(string propertyName, Func<T> propFunc) =>
            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>
            (
                x => this.PropertyChanged += x,
                x => this.PropertyChanged -= x
            )
            .Where(x => x.EventArgs.PropertyName == propertyName)
            .Select(_ => propFunc());

        public IObservable<string> UserNameChanged =>
           GetPropertyChangedObservable(nameof(UserName), () => UserName);

        public IObservable<string> PasswordChanged =>
            GetPropertyChangedObservable(nameof(Password), () => Password);

        public IObservable<string> SelectedObservableChanged =>
            GetPropertyChangedObservable(nameof(SelectedObservable), () => SelectedObservable);


        // implementation of Observable auto completes
        public void BasicChangeObservable()
        {
            ResetCounters();
            serialDisposable.Disposable =
                UserNameChanged
                    .SelectMany(result => FilterList(_words, result))
                    .Catch((TimeoutException exc) => Observable.Return(new List<string> { exc.Message }))
                    .Repeat()
                    .ObserveOn(_uiScheduler)
                    .Subscribe(OnHandleInputList);
        }

        public void ThrowAwayResults()
        {
            ResetCounters();
            serialDisposable.Disposable =
                UserNameChanged
                    .Select(result => FilterList(_words, result))
                    .Switch()
                    .Catch((TimeoutException exc) => Observable.Return(new List<string> { exc.Message }))
                    .Repeat()
                    .ObserveOn(_uiScheduler)
                    .Subscribe(OnHandleInputList);
        }

        public void DebounceObservable()
        {
            ResetCounters();
            serialDisposable.Disposable =
                UserNameChanged
                    .Throttle(TimeSpan.FromSeconds(1), scheduler: _backgroundScheduler)
                    .SelectMany(result => FilterList(_words, result))
                    .Catch((TimeoutException exc) => Observable.Return(new List<string> { exc.Message }))
                    .Repeat()
                    .ObserveOn(_uiScheduler)
                    .Subscribe(OnHandleInputList);
        }






        private void SetupObservablePicker()
        {
            SelectedObservableChanged
                .StartWith(nameof(BasicChangeObservable))
                .Subscribe(selected =>
                {
                    switch (selected)
                    {
                        case nameof(BasicChangeObservable):
                            BasicChangeObservable();
                            break;
                        case nameof(DebounceObservable):
                            DebounceObservable();
                            break;
                        case nameof(ThrowAwayResults):
                            ThrowAwayResults();
                            break;
                    }
                });
        }


        void ResetCounters()
        {
            Notifications = 0;
            FilterStarted = 0;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }

        void ReadFileIn()
        {

            var assembly = typeof(UserLoginViewModelReactive).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream("RxPresentation.Words.txt");
            _words = new List<string>();
            using (var reader = new System.IO.StreamReader(stream))
            {
                while (!reader.EndOfStream)
                    _words.Add(reader.ReadLine());
            }
        }

        void PasswordChangeObservable()
        {
            Notifications = 0;
            PasswordChanged
                .SelectMany(result => FilterList(_words, result))
                .Select(results => results.Select(result => Enumerable.Range(1, result.Length).Select(x => "*").ToArray()))
                .Select(arrays => arrays.Select(array => String.Join("", array)).ToList())
                .Subscribe(OnHandleInputList);


            Observable.CombineLatest(UserNameChanged, PasswordChanged,
                (username, password) =>
                {
                    if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                        return false;

                    return password.Length > 5;
                })
                .StartWith(false)
                .Subscribe(isValid =>
                {
                    IsValid = isValid;
                    LoginCommand?.ChangeCanExecute();
                });
        }
    }
}