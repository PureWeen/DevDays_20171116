using DevDays.Services;
using System;
using System.Collections.Generic;
using System.Text;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DevDays.Models;
using ReactiveUI;
using System.Reactive;
using System.Linq;

namespace DevDays.ViewModels
{
    public class UserListViewModel : ViewModelBase
    {
        readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        readonly ReadOnlyObservableCollection<UserDetailsViewModel> _visibleUsers;
        readonly SourceList<UserDetailsViewModel> _users;
        readonly Func<UserModel, UserDetailsViewModel> _userDetailsViewModelFactory;
        readonly IScheduler _uiScheduler;

        public UserListViewModel(
            IUserService userService,
            INavigationService navigationService,
            Func<UserModel, UserDetailsViewModel> userDetailsViewModelFactory,
            IScheduler uiScheduler)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _uiScheduler = uiScheduler ?? throw new ArgumentNullException(nameof(uiScheduler));
            _userDetailsViewModelFactory = userDetailsViewModelFactory ?? throw new ArgumentNullException(nameof(userDetailsViewModelFactory));

            _users = new SourceList<UserDetailsViewModel>();

            _users
                .Connect()     
                .DisposeMany()
                .ObserveOn(uiScheduler)
                .Bind(out _visibleUsers)
                .Subscribe()
                .DisposeWith(disposable);


            AddNew = ReactiveCommand.CreateFromObservable(() => _navigationService.UserDetailsView());
        }


        public void Delete(UserDetailsViewModel viewModel)
        {
            _userService
                .DeleteUser(viewModel.Id)                
                .Subscribe(_=>
                {
                    _users
                        .Edit(innerList =>
                        {
                            var existing = innerList.FirstOrDefault(x => x.Id == viewModel.Id);
                            if(existing != null)
                            {
                                innerList.Remove(existing);
                            }
                        });
                });
        }

        public void AddOrUpdate(UserModel updatedModel)
        {
            _users
                .Edit(innerList =>
                {                    
                    var existing = innerList.Select((item, index) => new { Item = item, Index = index })
                                   .Where(pair => pair.Item.Id == updatedModel.Id)
                                   .FirstOrDefault();

                    var replacementVm = _userDetailsViewModelFactory(updatedModel);

                    if (existing != null)
                    {
                        innerList.Remove(existing.Item);
                        innerList.Insert(existing.Index, replacementVm);
                    }
                    else
                    {
                        innerList.Add(replacementVm);
                    }
                });
        }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public void LoadList()
        {
            _userService
                .GetUsers()
                .Subscribe(users =>
                {
                    _users
                        .Edit(innerList =>
                        {
                            innerList.Clear();
                            innerList.AddRange(users.Select(_userDetailsViewModelFactory)); 
                        });
                });
        }

        public ReadOnlyObservableCollection<UserDetailsViewModel> VisibleUsers => _visibleUsers;

    }
}
