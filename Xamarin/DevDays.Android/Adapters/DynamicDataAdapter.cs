using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using ReactiveUI;
using System.Threading;
using System.Reactive.Disposables;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DynamicData.Binding;

namespace DevDays.Android.Adapters
{
    public abstract class DynamicDataRecyclerViewAdapter<TViewModel> : RecyclerView.Adapter where TViewModel : class, IReactiveObject
    {
        private readonly ReadOnlyObservableCollection<TViewModel> list;
        private IDisposable _inner;

        protected DynamicDataRecyclerViewAdapter(ReadOnlyObservableCollection<TViewModel> backingList)
        {
            this.list = backingList;

            _inner = list
                        .ToObservableChangeSet()
                        .Subscribe(_ => NotifyDataSetChanged());
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {            
            ((IViewFor)holder).ViewModel = this.list[position];
        }

        public override int ItemCount
        {
            get
            {
                return this.list.Count;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Interlocked.Exchange<IDisposable>(ref this._inner, Disposable.Empty).Dispose();
        }
    }
}