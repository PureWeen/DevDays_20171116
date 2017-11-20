using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDays.Refit
{
    public class User { }

    public interface IMyApi
    {
        [Post("/users/new")]
        IObservable<Unit> CreateUser(
            [Body] User user,
            [Header("X-Emoji")] string emoji);
    }

    public class Code
    {
        public void Api()
        {
            RestService.For<IMyApi>("http://www.someendpoint.com")
                .CreateUser(new User(), "some emoji")
                .Retry(3)
                .Replay(1)
                .Publish()
                .RefCount();
        }

    }
}
