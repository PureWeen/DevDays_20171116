using Akavache;
using ReactiveUI;
using DevDays.Models;
using DevDays.Services;
using DevDays.ViewModels;
using Splat;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using DebugLogger = DevDays.Services.DebugLogger;

namespace DevDays
{
    public abstract class CompositionRoot
    {
        protected readonly Lazy<IUserService> _userService;
        protected readonly Lazy<IStorageService> _storageService;
        protected readonly Lazy<IFilesystemProvider> _fileSystemProvider;
        protected readonly Lazy<INavigationService> _navigationService;
        protected readonly Lazy<ISecureStorageService> _secureStorageService;

        public CompositionRoot()
        {
            _userService = new Lazy<IUserService>(CreateUserService);
            _storageService = new Lazy<IStorageService>(CreateStorageService);
            _fileSystemProvider = new Lazy<IFilesystemProvider>(CreateFileSystemProvider);
            _navigationService = new Lazy<INavigationService>(CreateNavigationService);
            _secureStorageService = new Lazy<ISecureStorageService>(CreateSecureStorageService);


#if DEBUG
            var logger = new DebugLogger();
            Locator.CurrentMutable.RegisterConstant(logger, typeof(ILogger));
            
            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                logger.Write($"{ex}", LogLevel.Error);
            });
            
#endif
        }

        protected abstract INavigationService CreateNavigationService();

        protected abstract ISecureStorageService CreateSecureStorageService();

        public virtual IScheduler CreateUiScheduler() =>
            RxApp.MainThreadScheduler;


        public virtual IScheduler CreateBackgroundScheduler() =>
            RxApp.TaskpoolScheduler;


        public UserListViewModel CreateUserListViewModel() =>
              new UserListViewModel(this._userService.Value, _navigationService.Value, (userModel) => CreateUserDetailsViewModel(userModel), CreateUiScheduler());
        

        public UserDetailsViewModel CreateUserDetailsViewModel(UserModel model) =>
              new UserDetailsViewModel(model, this._userService.Value, _navigationService.Value, CreateUiScheduler());



        protected virtual IFilesystemProvider CreateFileSystemProvider()
        {
            // this is registered internally by Akavache
            return Locator.Current.GetService<IFilesystemProvider>();
        }

        protected virtual IUserService CreateUserService()
        {
            return new UserService(_storageService, _secureStorageService, CreateBackgroundScheduler());
        }


        public virtual IStorageService CreateStorageService()
        {
            return new AkavacheStorageService(_fileSystemProvider, CreateBackgroundScheduler());
        }
        
    }
}
