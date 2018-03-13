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
using ReactiveUI;
using System.Reactive;

namespace RxPresentation
{
    public class UserLoginViewModelRxUI : ReactiveObject
    {
        IScheduler _backgroundScheduler;
        IScheduler _uiScheduler; 
        string _Password;
        string _UserName;
        private IReadOnlyList<string> _searchTerms;
        SerialDisposable serialDisposable = new SerialDisposable(); 
        List<string> _words = null;
        private int _notifications;
        private int _filterStarted;
        private string _selectedObservable;
        private ObservableAsPropertyHelper<bool> _isValid;

        public UserLoginViewModelRxUI(IScheduler backgroundScheduler, IScheduler uiScheduler)
        {
            _backgroundScheduler = backgroundScheduler;
            _uiScheduler = uiScheduler;
            ReadFileIn();
            PasswordChangeObservable();
            SetupObservablePicker();

            LoginCommand = ReactiveCommand<Unit, Unit>.Create(() => { },
                canExecute: this.WhenAnyValue(x=> x.IsValid));
        }

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
                    .Catch((TimeoutException exc)=> Observable.Return(new List<string> { exc.Message }) )
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

        void OnHandleInputList(IReadOnlyList<string> inputList)
        {
            Notifications++;
            SearchTerms = inputList;
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

        Random randomGenerator = new Random();
        IObservable<IReadOnlyList<string>> FilterList(IReadOnlyList<string> inputList, string filter)
        {
            FilterStarted++;

            // delay simulates a lookup or just a general time wait
            return Observable.Timer(TimeSpan.FromSeconds(1), scheduler: _backgroundScheduler)
                .SelectMany(_ =>
                {
                    if(randomGenerator.Next(0, 10) == 3)
                    {
                        return Observable.Throw<List<string>>(new TimeoutException("Internet is down panic"));
                    }

                    return Observable.Return(inputList.Where(word => word.StartsWith(filter)).ToList());
                });
        }


        void ResetCounters()
        {
            Notifications = 0;
            FilterStarted = 0;
        }


        public IObservable<string> UserNameChanged =>
           this.WhenAnyValue(x => x.UserName);

        public IObservable<string> PasswordChanged =>
           this.WhenAnyValue(x => x.Password);

        public IObservable<string> SelectedObservableChanged =>
           this.WhenAnyValue(x => x.SelectedObservable);


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
            get => _isValid.Value;
        }
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        void PasswordChangeObservable()
        {
            Notifications = 0;
            PasswordChanged
                .SelectMany(result => FilterList(_words, result))
                .Select(results => results.Select(result => Enumerable.Range(1, result.Length).Select(x => "*").ToArray()))
                .Select(arrays => arrays.Select(array => String.Join("", array)).ToList())
                .Subscribe(OnHandleInputList);

            _isValid = 
                this.WhenAnyValue(x => x.UserName, x => x.Password,
                    (username, password) =>
                    {
                        if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                            return false;

                        return password.Length > 5;
                    })
                    .ToProperty(this, vm => vm.IsValid, initialValue: false);
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

    }
}