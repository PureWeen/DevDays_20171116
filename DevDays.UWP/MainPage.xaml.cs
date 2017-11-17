using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DevDays.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            btnClick
                .Events()
                .Click
                .Buffer(5)
                .Take(1)
                .Subscribe(_ => lblClicked.Text = "clicked 5 times");


            this.Events()
                .PointerPressed.Do(_=> lblEvents.Text = "Pressed" )
                .Select
                (_=> 
                    this.Events().PointerMoved.Do(__ => lblEvents.Text = "PointerMoved")
                        .TakeUntil(
                            this.Events().PointerReleased.Do(__ => lblEvents.Text = "PointerReleased")
                        )
                        .Select(args => args.GetCurrentPoint(this))
                        .ToList()
                )                
                .Switch()
                .TakeUntil(this.Events().Unloaded)
                .Subscribe(args =>
                {
                    var data = args.Select(x => x.Position.X.ToString()).ToArray();
                    lblClicked.Text = String.Join(",", data);
                });
        }
    }
}
