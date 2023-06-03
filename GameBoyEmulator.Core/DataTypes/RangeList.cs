using System.Diagnostics.CodeAnalysis;

namespace GameBoyEmulator.Core.DataTypes
{
    public class RangeList<TKey, TValue> : SortedList<(TKey minIncl, TKey maxIncl), TValue>
        where TKey : IComparable
    {
        public RangeList() : base(RangeComparerInstance)
        {
        }

        public RangeList(IDictionary<(TKey minIncl, TKey maxIncl), TValue> dictionary)
            : base(dictionary, RangeComparerInstance)
        {
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value)) throw new KeyNotFoundException();
                return value!;
            }
        }

        public bool TryGetValue(TKey key, [NotNullWhen(returnValue: true)]out TValue? value)
        {
            foreach (var curKey in Keys)
            {
                if (curKey.minIncl.CompareTo(key) > 0 || key.CompareTo(curKey.maxIncl) > 0) continue;
                
                value = this[curKey]!;
                return true;
            }

            value = default;
            return false;
        }

        private static readonly RangeComparer RangeComparerInstance = new RangeComparer();
        private class RangeComparer : IComparer<(TKey minIncl, TKey maxIncl)>
        {
            public int Compare((TKey minIncl, TKey maxIncl) x, (TKey minIncl, TKey maxIncl) y)
            {
                return x.minIncl.CompareTo(y.minIncl);
            }
        }
    }
}