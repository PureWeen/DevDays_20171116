using DevDays.Models;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace DevDays.Services
{
    public interface INavigationService
    {
        IObservable<Unit> UserDetailsView();
        IObservable<Unit> UserDetailsView(UserModel model);
    }
}
