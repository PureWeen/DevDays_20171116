using Microsoft.Reactive.Testing;
using ReactiveUI;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RxPresentation
{
    public static class Helpers
    {

        public static void TestCanExecute(this ReactiveCommand command, TestScheduler sched, bool desire)
        {
            bool canExecute = !desire;
            command.CanExecute.Take(1)
                .Subscribe(result => canExecute = result);
            Assert.Equal(desire, canExecute);
        }

        public static void TestIsExecuting(this ReactiveCommand command, TestScheduler sched, bool desire)
        {
            bool canExecute = !desire;
            command.IsExecuting.Take(1)
                .Subscribe(result => canExecute = result);
            Assert.Equal(desire, canExecute);
        }
    }
}
