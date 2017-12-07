using DevDays.ViewModels;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using System.Reactive.Concurrency;
using DevDays.Models;
using System.Text.RegularExpressions;

namespace DevDays.Services
{
    public class UserService : IUserService
    {
        readonly Lazy<ISecureStorageService> _secureStorageService = null;
        readonly Lazy<IStorageService> _storageService = null;
        readonly IScheduler _backgroundScheduler = null;

        public UserService(
            Lazy<IStorageService> storageService, 
            Lazy<ISecureStorageService> secureStorageService,
            IScheduler backgroundScheduler)
        {
            _storageService = storageService;
            _backgroundScheduler = backgroundScheduler;
            _secureStorageService = secureStorageService;
        }



        public bool ValidateNoRepeatedPatterns(string password)
        {
            for (int w = 0; w <= password.Length; w++)
            {
                var testWord = password.Substring(w, password.Length - w);
                for (int i = 1; i <= (testWord.Length / 2); i++)
                {
                    string firstPart = testWord.Substring(0, i);
                    string secondPart = testWord.Substring(i, i);


                    if (firstPart == secondPart)
                        return false;
                }
            }

            return true;
        }

        public bool ValidatePassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password)) { return false; }
            if(password.Length < 5 || password.Length > 12) { return false; }

            var numbersAndLetters = Regex.Match(password, "^([0-9]+[a-zA-Z]+|[a-zA-Z]+[0-9]+)[0-9a-zA-Z]*$");
            if (!numbersAndLetters.Success) { return false; }
            return ValidateNoRepeatedPatterns(password);
        }

        public bool SavePassword(string userId, string password)
        {
            if(!ValidatePassword(password))
            {
                throw new ArgumentException("Invalid Password");
            }

            _secureStorageService
                .Value
                .SavePassword(userId, password);

            return true;
        }


        public IObservable<string> GetUserId(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
                return Observable.Return(String.Empty);

            return 
                _storageService.Value.GetData<string>(userName)
                    .Catch((KeyNotFoundException ke) => Observable.Return(String.Empty));
        }

        public IObservable<UserModel> SaveUser(UserModel model, string password)
        {
            if(String.IsNullOrWhiteSpace(model.Id))
            {
                model.Id = $"{Guid.NewGuid()}";
            }

            var storeUserName = _storageService.Value.StoreData(model.UserName, model.Id);
            var storeModel = _storageService.Value.StoreData(model.Id, model);


            return Observable.Zip(storeUserName, storeModel)
                        .Select(_=> model)
                        .Do(_=>
                        {
                            if(!String.IsNullOrWhiteSpace(password))
                            {
                                SavePassword(model.Id, password);
                            };
                        });
        }


        public IObservable<IEnumerable<UserModel>> GetUsers()
        {
            return 
                _storageService
                    .Value
                    .GetDataOfType<UserModel>();

        }

        public IObservable<UserModel> GetUser(string userDetailsId)
        {
            var storage = _storageService.Value;

            return storage
                .GetData<UserModel>(userDetailsId);
        }

        public IObservable<Unit> DeleteUser(string id)
        {

            var deletedUserModel =
                GetUser(id)
                    .SelectMany(model =>
                    {
                        return 
                            Observable.Zip(
                                _storageService.Value.DeleteData<UserModel>(id),
                                _storageService.Value.DeleteData<string>(model.UserName)
                                );
                    });


            return deletedUserModel.ToSignal();
        }
    }
}
