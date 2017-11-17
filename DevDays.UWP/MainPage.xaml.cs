﻿using System;
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
using System.Collections.ObjectModel;
using DynamicData.Binding;
using DynamicData.Aggregation;

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
            _components.Add(new ViewComponents(DynamicDataFiltering, RadioDDFiltering, SetupFilteringAnimals()));



            this.Events().Loaded
                .Select
                (_=> 


                    _components
                        .Connect()
                        .DisposeMany()
                        .MergeMany(x => x.CheckChanged.IsTrue().Select(__=> x.MyDemo))
                        .Switch()
                        .TakeUntil(this.Events().Unloaded)

                )
                .Switch()
                .Subscribe();
        }



        ReadOnlyObservableCollection<Animal> filteredAnimals;
        IObservable<Unit> SetupFilteringAnimals()
        {

            SourceList<Animal> animalList = Animal.CreateMeSomeAnimalsPlease();

            IObservable<Func<Animal, bool>> dynamicFilter = 
                // ReactiveUI connecting to the DepedencyProperty
                this.WhenAnyValue(x => x.tbFilterText.Text) 
                // Debounce
                    .Throttle(TimeSpan.FromMilliseconds(250))
                    .Select(CreatePredicate);

            IObservable<IComparer<Animal>> dynamicSort =
                this.WhenAnyValue(x => x.cbSortByName.IsChecked)
                    .Select(x => x ?? false)
                    .Select(isChecked => isChecked ? 
                        SortExpressionComparer<Animal>.Ascending(i => i.Name) 
                        : SortExpressionComparer<Animal>.Ascending(i => animalList.Items.IndexOf(i))
                    );

            var returnValue =
                animalList
                   .Connect()
                   .Filter(dynamicFilter) //accepts any observable
                   .Sort(dynamicSort)
                   .ObserveOnDispatcher()
                   .Bind(out filteredAnimals);


            //Create filtered source to watch for changes on
            var filteredAnimalsChanged = 
                filteredAnimals
                    .ToObservableChangeSet()
                    .Publish()
                    .RefCount();


            var calculateAverage =
                // Watch for property Changed events
                filteredAnimalsChanged
                   .WhenPropertyChanged(x => x.AnimalRating).ToUnit()

                   // watch for list changed
                   .Merge(filteredAnimalsChanged.ToUnit())

                   // Perform calculation over data set
                    .Select(_ => filteredAnimals.Where(x => x.AnimalRating > 0).ToArray())
                    .Do(filterBy =>
                    {
                        if(!filterBy.Any())
                        {
                            tbAverageRating.Text = "Unknown";
                            return;
                        }

                        tbAverageRating.Text =
                            filterBy.Average(x => x.AnimalRating)
                                .ToString();
                    })
                    .ToUnit();


            lvFilteredAnimals.ItemsSource = filteredAnimals;


            return returnValue.ToUnit().Merge(calculateAverage);

            Func<Animal, bool> CreatePredicate(string text)
            {
                if (text == null || text.Length < 3)
                    return animal => true;

                
                return animal => animal.Name.ToLower().Contains(text)
                                 || animal.Type.ToLower().Contains(text)
                                 || animal.Family.ToString().ToLower().Contains(text);
            }
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
                    .ToUnit();
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
                    .ToUnit();
        } 
    }
}

                            