using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper
{
    internal static class LocalExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                collection.Add(item);
            }
        }
    }
}
