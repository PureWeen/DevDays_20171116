using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using RxPresentation;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using Xunit;

public class UserLoginViewModelReactiveTests
{
    UserLoginViewModelReactive Create(IScheduler scheduler)
    {
        return new UserLoginViewModelReactive(backgroundScheduler: scheduler, uiScheduler: scheduler);
    }

    [Fact]
    public void BasicPropertyChanged()
    {
        (new TestScheduler()).With(sched =>
        {
            var vm = Create(sched);
            string testMe = null;
            vm.PasswordChanged.Subscribe(result =>
            {
                testMe = result;
            });

            vm.Password = "Password";
            Assert.Equal(testMe, "Password");

            testMe = null;
            vm.Password = "Password";
            Assert.NotEqual(testMe, "Password");


            vm.Password = "";
            vm.Password = "Password";
            Assert.Equal(testMe, "Password");
        });
    }


    [Fact]
    public void BasicThrottleTest()
    {
        (new TestScheduler()).With(sched =>
        {
            var vm = Create(sched);
            vm.SelectedObservable = nameof(UserLoginViewModelReactive.DebounceObservable);

            vm.UserName = "awe";
            Assert.Null(vm.SearchTerms);
            sched.AdvanceByMs(10000);
            Assert.NotNull(vm.SearchTerms);
            Assert.Equal(1, vm.Notifications);
            Assert.Equal(1, vm.FilterStarted);
        });
    }
} 
