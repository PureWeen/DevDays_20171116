using RxPresentation;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reactive;

public class UserLoginViewModelRxUITests
{

    UserLoginViewModelRxUIComplex Create(IScheduler scheduler)
    {
        return new UserLoginViewModelRxUIComplex(backgroundScheduler: scheduler, uiScheduler: scheduler);
    }



    [Fact]
    public void InitialSetup()
    {
        (new TestScheduler()).With(sched =>
        {
            var vm = Create(sched);
            sched.AdvanceByMs(500);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
        });
    }



    [Fact]
    public void PasswordsWithInvalidPasswordAreNotOk()
    {
        (new TestScheduler()).With(sched =>
        {

            var vm = Create(sched);
            vm.Password = "InvalidPassword";
            sched.AdvanceByMs(100000);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
            Assert.False(vm.IsPasswordValid);
        });
    }


    [Fact]
    public void NotPwned()
    {
        (new TestScheduler()).With(sched =>
        {

            var vm = Create(sched);
            vm.Password = "thisoneisfine";
            sched.AdvanceByMs(1499);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
            sched.AdvanceByMs(2);
            Assert.Equal("not pwned", vm.HaveIBeenPwnedData);
        });
    }


    [Fact]
    public void UserChangedThereMinds()
    {
        (new TestScheduler()).With(sched =>
        {

            var vm = Create(sched);
            vm.Password = "thisoneisfine";
            sched.AdvanceByMs(1400);
            vm.Password = "someotherpassword";


            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
            sched.AdvanceByMs(1499);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
            sched.AdvanceByMs(2);
            Assert.Equal("not pwned", vm.HaveIBeenPwnedData);
        });
    }

    [Fact]
    public void IsUserNameIsInValid()
    {
        (new TestScheduler()).With(sched =>
        {

            var vm = Create(sched);
            Assert.False(vm.UserNameIsValid);
            vm.UserName = "badusername";
            sched.AdvanceByMs(10);
            Assert.False(vm.UserNameIsValid);
        });
    }

    [Fact]
    public void IsUserNameValid()
    {
        (new TestScheduler()).With(sched =>
        {

            var vm = Create(sched);
            vm.UserName = "goodusername";
            sched.AdvanceByMs(10);
            Assert.True(vm.UserNameIsValid);
        });
    }




    [Fact]
    public void TestEverything()
    {
        (new TestScheduler()).With(sched =>
        {
            var vm = Create(sched);
            Assert.False(vm.UserNameIsValid);
            Assert.False(vm.IsPasswordValid);
            vm.LogTheUserIn
                .TestCanExecute(sched, false);

            vm.UserName = "goodusername";
            vm.Password = "goodpassword";
            sched.AdvanceByMs(2000);

            //All our data is valid
            Assert.True(vm.UserNameIsValid);
            Assert.True(vm.IsPasswordValid);
            Assert.True(vm.IsEverythingValid);

            vm.LogTheUserIn
              .TestCanExecute(sched, true);

            vm.LogTheUserIn
                .IsExecuting
                .DoLog("IsExecuting")
                .Subscribe();

            vm.LogTheUserIn.Execute().Subscribe(); // execute doesn't execute
            sched.AdvanceByMs(10); // start execute

            // Can Execute is now false
            vm.LogTheUserIn
              .TestCanExecute(sched, false);

            // finish executing
            sched.AdvanceByMs(3000);

            // now it's true
            vm.LogTheUserIn
              .TestCanExecute(sched, true);

            // Subscribe to the commands
            vm.LogTheUserIn
                .Where(x=> x == true)
                .Subscribe(_ => Console.WriteLine("User Logged In"));

            // Watch for exceptionss
            vm.LogTheUserIn
                .ThrownExceptions
                .Subscribe(exc => Console.WriteLine($"{exc}"));

            // Let's execute again
            vm.LogTheUserIn.Execute().Subscribe();
            sched.AdvanceByMs(10);
            vm.LogTheUserIn
                .TestIsExecuting(sched, true);

            vm.Password = "changed my mind";
            sched.AdvanceByMs(10);
            vm.LogTheUserIn
                .TestIsExecuting(sched, false);
        });
    }
}