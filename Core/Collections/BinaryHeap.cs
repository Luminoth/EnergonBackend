using System;

namespace EnergonSoftware.Core.Collections
{
// https://en.wikipedia.org/wiki/Binary_heap
    // TODO: implement ICollection<T> and all the other relevant interfaces
    public sealed class BinaryHeap<T> where T: IComparable<T>
    {
        public enum HeapType
        {
            MinHeap,
            MaxHeap
        }

        public HeapType Type { get; }

        public int Capacity => _heap?.Length ?? 0;

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        private T[] _heap;

        public BinaryHeap(HeapType type)
        {
            Type = type;
        } 

        public void Add(T item)
        {
            Initialize();

            _heap[Count] = item;
            ++Count;

 // TODO: fix the heap
        }

        public bool Remove(T item)
        {
// TODO:
throw new NotImplementedException();
        }

        public void Clear()
        {
            _heap = null;
            Count = 0;
        }

        public bool Contains(T item)
        {
            if(Count < 1) {
                return false;
            }

// TODO: this can probably be optimized

            foreach(T i in _heap) {
                if(i.Equals(item)) {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _heap.CopyTo(array, arrayIndex);
        }

        public T Peek()
        {
// TODO: default(T) seems bad here
            return null == _heap || _heap.Length < 1 ? default(T) : _heap[0];
        }

        public T Pop()
        {
            if(null == _heap || _heap.Length < 1) {
// TODO: default(T) seems bad here
                return default(T);
            }

// TODO: this has a lot of bad edge cases to check
            T top = Peek();
            _heap[0] = _heap[--Count];
            Heapify();
            return top;
        }

        private void Initialize()
        {
            if(null == _heap) {
                _heap = new T[4];
                return;
            }

            if(_heap.Length == Count) {
                Grow();
            }
        }

        private void Grow()
        {
            // copy the existing heap into a temp array
            var scratch = new T[_heap.Length];
            Array.Copy(_heap, scratch, scratch.Length);

            // double our capacity
            _heap = new T[_heap.Length * 2];
            Array.Copy(scratch, _heap, scratch.Length);
        }

        private void Heapify()
        {
            switch(Type)
            {
            case HeapType.MaxHeap:
                MaxHeapify(0);
                break;
            case HeapType.MinHeap:
                MinHeapify(0);
                break;
            }
        }

        private void MaxHeapify(int index)
        {
            int largest;

            int left = Left(index);
            if(left < Count && _heap[left].CompareTo(_heap[index]) > 0) {
                largest = left;
            } else {
                largest = index;
            }

            int right = Right(index);
            if(right < Count && _heap[right].CompareTo(_heap[largest]) > 0) {
                largest = right;
            }

            if(largest != index) {
                Exchange(index, largest);
                MaxHeapify(largest);
            }
        }

        private void MinHeapify(int index)
        {
            int smallest;

            int left = Left(index);
            if(left < Count && _heap[left].CompareTo(_heap[index]) < 0) {
                smallest = left;
            } else {
                smallest = index;
            }

            int right = Right(index);
            if(right < Count && _heap[right].CompareTo(_heap[smallest]) < 0) {
                smallest = right;
            }

            if(smallest != index) {
                Exchange(index, smallest);
                MinHeapify(smallest);
            }
        }

        private void Exchange(int a, int b)
        {
            T av = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = av;
        }

        private static int Parent(int index)
        {
            return (index - 1) / 2;
        }

        private static int Left(int index)
        {
            return (2 * index) + 1;
        }

        private static int Right(int index)
        {
            return (2 * index) + 2;
        }
    }
}
