using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;


namespace RxPresentation.GeneralObservables
{


    public class EnumerableTest
    {
        public EnumerableTest()
        {
            List<Child> Children = new List<Child>();
            IObservable<Child> childrenProducer = Children.ToObservable();


            Children
                .Where(x => x.IsNice)
                .Select(x => x.Name)
                .ToList()
                .ForEach(childName => Debug.WriteLine(childName));

            childrenProducer
                .Where(x=> x.IsNice)
                .Select(x=> x.Name)
                .Subscribe(childName => Debug.WriteLine(childName));
        }
    }

    public class Parent
    {
        public List<Child> Children { get; set; }
    }

    public class Child
    {
        public bool IsNice { get; set; }

        public string Name { get; set; }
    }
}
