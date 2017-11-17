using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;

namespace DevDays.GeneralObservables
{
    public class BasicReactiveIdeas
    {
        

        class Parent
        {
            public string Name { get; set; }
            public List<Child> Children { get; set; }
        }

        class Child
        {
            public bool HasBeenNice { get; set; }
            public string Name { get; set; }
        }


        [Fact]
        public void TakeFromRange()
        {
            List<Parent> parents = new List<Parent>();
                        
            parents
                .Select(x => x.Children
                        .Where(c => c.HasBeenNice)
                )
                .SelectMany(child => child); //flatten list


        }

        [Fact]
        public void TakeUntilSomeEventOccurs()
        {
            Observable.Range(1, 5)
                .TakeUntil(NoLongerCare());
        }


        [Fact]
        public async Task RetryWithException()
        {
            try
            {
                await SomeSketchyTask();
            }
            catch (Exception e)
            {
            }

            SomeSketchyTask()
                .ToObservable()
                .Retry(3)
                .Subscribe(
                    (result) => { },
                    (error) => { }
                );
                
        }

        IObservable<Unit> NoLongerCare()
        {            
            return Observable.Return(Unit.Default);
        }


        Task<bool> SomeSketchyTask()
        {
            if(new Random().Next(0, 3) == 2)
            {
                throw new Exception("The internet is broken");
            }

            return Task.FromResult(true);
        }
    }
}
