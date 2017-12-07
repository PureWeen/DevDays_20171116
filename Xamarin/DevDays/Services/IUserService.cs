using System;
using System.Collections.Generic;
using DevDays.ViewModels;
using System.Reactive;
using DevDays.Models;

namespace DevDays.Services
{
    public interface IUserService
    {
        IObservable<IEnumerable<UserModel>> GetUsers();
        IObservable<UserModel> GetUser(string userDetailsId);
        bool ValidatePassword(string password);
        bool SavePassword(string userId, string password);
        IObservable<Unit> DeleteUser(string id);
        IObservable<UserModel> SaveUser(UserModel model, string password);
        IObservable<string> GetUserId(string userName);
    }
}