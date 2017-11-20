using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DevDays.DynamicData
{
    public class BasicTests
    {
        public class GrandParent
        {
        }

       [Fact]
        public void ReadonlyTest()
        {
            ReadOnlyObservableCollection<GrandParent>
                data = null;
            SourceList<GrandParent> source = new SourceList<GrandParent>();

            source.Add(new GrandParent());
            var sourceObs =
                Observable.Defer(()=>
                    source
                    .Connect()
                    .Bind(out data)
                );

            var disp = sourceObs.Subscribe();
            Assert.Equal(1, data.Count);

            disp.Dispose();
            disp = sourceObs.Subscribe();
            Assert.Equal(1, data.Count); //fails count equals 2

        }
    }
}
