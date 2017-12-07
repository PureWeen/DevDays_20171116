using Microsoft.Reactive.Testing;
using Moq;
using DevDays.Models;
using DevDays.Services;
using DevDays.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ReactiveUI.Testing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

public class UserDetailsViewModelTests
{

    UserDetailsViewModel createViewModel(IScheduler scheduler)
    {

        Mock<IUserService> userService = new Mock<IUserService>();

        userService.Setup(x => x.ValidatePassword(It.IsAny<string>()))
            .Returns(true);

        userService.Setup(x => x.GetUserId(It.IsAny<string>()))
            .Returns(Observable.Return(String.Empty));

        return
            new UserDetailsViewModel(
                new UserModel(),
                userService.Object,
                new Mock<INavigationService>().Object,
                scheduler);
    }


    [Fact]
    public void SaveButtonDeactivated()
    {
        (new TestScheduler())
          .With(sched =>
          {
              var vm = createViewModel(sched);
              sched.AdvanceByMs(1000);
              Assert.False(vm.EnableSaveButton);

          });
    }



    [Fact]
    public void SaveButtonActivated()
    {
        (new TestScheduler())
            .With(sched =>
            {
                var vm = createViewModel(sched);
                vm.UserName = "12345";
                vm.Password = "Pasword12345";
                sched.AdvanceByMs(1000);
                Assert.True(vm.EnableSaveButton);

            });
    }
}