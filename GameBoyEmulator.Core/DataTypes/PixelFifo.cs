using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GameBoyEmulator.Core.DataTypes
{
    public class PixelFifo : ICollection, IReadOnlyCollection<Pixel>
    {
        private const int MaxItems = 16;
        
        private readonly Queue<Pixel> _fifo = new Queue<Pixel>(MaxItems);

        public void Clear() => _fifo.Clear();
        
        public void Enqueue(Pixel item)
        {
            // TODO: if (_fifo.Count >= MaxItems) throw new IndexOutOfRangeException();

            _fifo.Enqueue(item);
        }

        public Pixel Dequeue() => _fifo.Dequeue();
        public bool TryDequeue([MaybeNullWhen(false)] out Pixel result) => _fifo.TryDequeue(out result);
        public bool TryPeek([MaybeNullWhen(false)] out Pixel result) => _fifo.TryPeek(out result);

        public IEnumerator<Pixel> GetEnumerator()
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