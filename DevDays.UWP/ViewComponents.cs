using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ReactiveUI;
using DynamicData;
using System.Reactive.Disposables;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DevDays.UWP
{
    public class ViewComponents : IDisposable
    {
        CompositeDisposable disposeOfMe = new CompositeDisposable();
        public ViewComponents(UIElement control, RadioButton radioDemo, IObservable<Unit> demoToRun)
        {
            this.Control = control;
            this.Button = radioDemo;
            this.MyDemo = demoToRun;

            CheckChanged =
                Button.Events().Checked.Merge(Button.Events().Unchecked)
                    .Select(_ => Button.IsChecked ?? false)
                    .Merge(Observable.Return(Button.IsChecked ?? false));

            CheckChanged
                .Subscribe(IsChecked =>
                {
                    if (IsChecked)
                    {
                        Control.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Control.Visibility = Visibility.Collapsed;
                    }

                })
                .DisposeWith(disposeOfMe);

        }


        public UIElement Control { get; set; }
        public RadioButton Button { get; set; }

        public IObservable<Unit> MyDemo { get; set; }
        public IObservable<bool> CheckChanged { get; }

        public void Dispose()
        {
            disposeOfMe.Dispose();
        }
    }
}

                            