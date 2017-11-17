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

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SourceList<ViewComponents> _components = new SourceList<ViewComponents>();

        public MainPage()
        {
            this.InitializeComponent();

            _components.Add(new ViewComponents(spButtonDemo, RadioDemo, SetupButtonClicks()));
            _components.Add(new ViewComponents(lvData,  RadioPosition, SetupPointerMovedSample()));


            this.Events().Loaded
                .Select
                (_=> 


                    _components
                        .Connect()
                        .DisposeMany()
                        .MergeMany(x => x.CheckChanged.Where(isTrue => isTrue).Select(__=> x.MyDemo))
                        .Switch()
                        .TakeUntil(this.Events().Unloaded)

                )
                .Switch()
                .Subscribe();
        }

        


        IObservable<Unit> SetupButtonClicks()
        {
            return 
                btnClick
                    .Events()
                    .Click
                    .Buffer(5)
                    .Do(_=> lblClicked.Text = "Good Job")
                    .DelayLabelClear(lblClicked)
                    .ToSignal();
        }


        IObservable<Unit> SetupPointerMovedSample()
        {
            return
                this.Events()
                    .PointerPressed.Do(_ => lblEvents.Text = "Pressed")
                    .Select
                    (_ =>
                        this.Events().PointerMoved.Do(__ => lblEvents.Text = "PointerMoved")
                            .Select(args => args.GetCurrentPoint(this))
                            .TakeUntil(
                                this.Events().PointerReleased.Do(__ => lblEvents.Text = "PointerReleased")
                            )
                            .ToList()
                    )
                    .Switch()
                    //Transform
                    .Select(args => args.Select(x => $"{Math.Round(x.Position.X, 2)}, {Math.Round(x.Position.Y, 2)}").ToArray())
                    .Do(data => lvData.ItemsSource = data)

                    //delay a couple seconds then clear label
                    .DelayLabelClear(lblEvents)
                    .ToSignal();
        } 
    }
}

                            