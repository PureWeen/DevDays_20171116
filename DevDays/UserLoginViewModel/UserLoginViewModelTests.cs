using DevDays;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using Xunit;

public class UlTests
{

    UserLoginViewModel Create(IScheduler scheduler)
    {
        return new UserLoginViewModel(backgroundScheduler: scheduler, uiScheduler: scheduler);
    }



    [Fact]
    public void InitialSetup()
    {
        (new TestScheduler()).With(sched => {

            var vm = Create(sched);
            sched.AdvanceByMs(500);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
        });
    }



    [Fact]
    public void PasswordsWithInvalidPasswordAreNotOk()
    {
        (new TestScheduler()).With(sched => {

            var vm = Create(sched);
            vm.Password = "InvalidPassword";
            sched.AdvanceByMs(100000);
            Assert.Equal("initialValue", vm.HaveIBeenPwnedData);
        });
    }


    [Fact]
    public void NotPwned()
    {
        (new TestScheduler()).With(sched => {

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
        (new TestScheduler()).With(sched => {

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
        (new TestScheduler()).With(sched => {

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
        (new TestScheduler()).With(sched => {

            var vm = Create(sched);
            vm.UserName = "goodusername";
            sched.AdvanceByMs(10);
            Assert.True(vm.UserNameIsValid);
        });
    }




    [Fact]
    public void IsEverythingValid()
    {
        (new TestScheduler()).With(sched => {

            var vm = Create(sched);
            Assert.False(vm.UserNameIsValid);
            vm.UserName = "goodusername";
            vm.Password = "goodpassword";
            sched.AdvanceByMs(2000);

            Assert.True(vm.UserNameIsValid);
            Assert.True(vm.IsPasswordValid);
            Assert.True(vm.IsEverythingValid);
        });
    }



}