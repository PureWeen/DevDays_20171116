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
        private string _selectedObservable;
        private ObservableAsPropertyHelper<bool> _isValid;
        private ObservableAsPropertyHelper<int> _notifications;
        private ObservableAsPropertyHelper<int> _filterStarted;

        public UserLoginViewModelRxUI(IScheduler backgroundScheduler, IScheduler uiScheduler)
        {
            // Layer is null but I can still define these 
            this.WhenAnyValue(x => x.Layer.Notifications)
                .ToProperty(this, vm => vm.Notifications, out _notifications);

            this.WhenAnyValue(x => x.Layer.FilterStarted)
                .ToProperty(this, vm => vm.FilterStarted, out _filterStarted);

            Layer = new PointlessLayer();


            _backgroundScheduler = backgroundScheduler;
            _uiScheduler = uiScheduler;
            ReadFileIn();
            PasswordChangeObservable();

            // Attach IsValid to immutable IsValid property
            //Ensures this is the only source of truth for the property
            _isValid =
                this.WhenAnyValue(x => x.UserName, x => x.Password,
                    (username, password) =>
                    {
                        if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                            return false;

                        return password.Length > 5;
                    })
                    .ToProperty(this, vm => vm.IsValid, initialValue: false);


            SetupObservablePicker();

            // Command that can be interact with in an observable way
            // just listens for changes to isvalid opposed to us having to push
            LoginCommand = ReactiveCommand<Unit, Unit>.Create(() => { },
                canExecute: this.WhenAnyValue(x => x.IsValid));

            LoginCommand
                .Subscribe(valueProducedByExecute => { });

            LoginCommand
                .IsExecuting.Subscribe(isExecuting => { });
        }

        public IObservable<string> UserNameChanged =>
           this.WhenAnyValue(x => x.UserName);

        public IObservable<string> PasswordChanged =>
           this.WhenAnyValue(x => x.Password);

        public IObservable<string> SelectedObservableChanged =>
           this.WhenAnyValue(x => x.SelectedObservable);

        void PasswordChangeObservable()
        {
            Layer.Notifications = 0;
            PasswordChanged
                .SelectMany(result => FilterList(_words, result))
                .Select(results => results.Select(result => Enumerable.Range(1, result.Length).Select(x => "*").ToArray()))
                .Select(arrays => arrays.Select(array => String.Join("", array)).ToList())
                .Subscribe(OnHandleInputList);
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

        void OnHandleInputList(IReadOnlyList<string> inputList)
        {
            Layer.Notifications++;
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
        private PointlessLayer _layer;

        IObservable<IReadOnlyList<string>> FilterList(IReadOnlyList<string> inputList, string filter)
        {
            Layer.FilterStarted++;

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


        void ResetCounters()
        {
            Layer.Notifications = 0;
            Layer.FilterStarted = 0;
        }




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
        public int Notifications
        {
            get => _notifications.Value;
        }

        public int FilterStarted
        {
            get => _filterStarted.Value;
        }

        public PointlessLayer Layer
        {
            get => _layer;
            set => this.RaiseAndSetIfChanged(ref _layer, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }





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

    public class PointlessLayer : ReactiveObject
    {
        private int _notifications;
        private int _filterStarted;


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
    }
}