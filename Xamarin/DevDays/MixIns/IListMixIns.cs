using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevDays
{
    public static class IListMixIns
    {

        public static void SyncList<T>(
            this IList<T> This,
            IEnumerable<T> sourceList)
        {
            This.SyncList<T, T>(sourceList, x => x, (x, y) => x.Equals(y), null);
        }


        public static void SyncList<T>(
			this IList<T> This,
			IEnumerable sourceList,
			Func<object, T> selector,
			Func<object, T, bool> areEqual,
			Action<object, T> updateExisting,
			bool dontRemove = false)
		{
			var sourceListEnum = sourceList.OfType<Object>().ToList();

			//passengers
			foreach (T dest in This.ToList())
			{
				var match = sourceListEnum.FirstOrDefault(p => areEqual(p, dest));
				if (match != null)
				{
					if (updateExisting != null)
						updateExisting(match, dest);
				}
				else if (!dontRemove)
				{
					This.Remove(dest);
				}
			}

			sourceListEnum.Where(x => !This.Any(p => areEqual(x, p)))
				.ToList().ForEach(p =>
			{
                if (This.Count >= sourceListEnum.IndexOf(p))
                    This.Insert(sourceListEnum.IndexOf(p), selector(p));
                else
                {
                    var result = selector(p);
                    if (!EqualityComparer<T>.Default.Equals(result, default(T)))
                        This.Add(result);
                }
			});
		}


        public static void SyncList<T, Source>(
            this IList<T> This,
            IEnumerable<Source> sourceList,
            Func<Source, T> selector,
            Func<Source, T, bool> areEqual,
            Action<Source, T> updateExisting,
            bool dontRemove = false)
        {
            //passengers
            foreach (T dest in This.ToList())
            {
                var match = sourceList.FirstOrDefault(p => areEqual(p, dest));
                if (!EqualityComparer<Source>.Default.Equals(match, default(Source)))
                {
                    updateExisting?.Invoke(match, dest);
                }
                else if (!dontRemove)
                {
                    This.Remove(dest);
                }
            }

            sourceList.Where(x => !This.Any(p => areEqual(x, p)))
                .ToList().ForEach(p =>
                {
                    var result = selector(p);
                    if(!EqualityComparer<T>.Default.Equals(result, default(T)))
                        This.Add(result);
                });
        }
    }

}
