using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GameBoyEmulator.Core.DataTypes
{
    public class Fifo<T> : ICollection, IReadOnlyCollection<T>
    {
        private const int MaxItems = 16;
        
        private readonly Queue<T> _fifo = new Queue<T>(MaxItems);

        public void Clear() => _fifo.Clear();
        
        public void Enqueue(T item)
        {
            if (_fifo.Count >= MaxItems) throw new IndexOutOfRangeException();

            _fifo.Enqueue(item);
        }

        public T Dequeue() => _fifo.Dequeue();
        public bool TryDequeue([MaybeNullWhen(false)] out T result) => _fifo.TryDequeue(out result);
        public bool TryPeek([MaybeNullWhen(false)] out T result) => _fifo.TryPeek(out result);

        public IEnumerator<T> GetEnumerator()
        {
            return _fifo.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_fifo).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_fifo).CopyTo(array, index);
        }

        public int Count => _fifo.Count;

        public bool IsSynchronized => ((ICollection)_fifo).IsSynchronized;

        public object SyncRoot => ((ICollection)_fifo).SyncRoot;
    }
}