using System;
using System.Collections;
using System.Collections.Generic;

namespace MaximDl
{
    public sealed class MaxImDlDocCollection : ComType, IReadOnlyCollection<MaxImDlDoc>
    {

        public struct Enumerator : IEnumerator<MaxImDlDoc>
        {
            private readonly MaxImDlDocCollection _collection;
            private int _currentIndex;
            private readonly int _count;
            internal Enumerator(MaxImDlDocCollection collection)
            {
                _collection = collection;
                _currentIndex = -1;
                _count = collection.Count;
            }

            public bool MoveNext()
                => ++_currentIndex < _count;

            public void Reset()
                => _currentIndex = -1;

            public MaxImDlDoc Current => _currentIndex >= 0 && _currentIndex < _count
                ? _collection[_currentIndex]
                : throw new InvalidOperationException();

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }


        public int Count => FromGetter<int>();

        public MaxImDlDoc this[int i] =>
            new MaxImDlDoc(Type.InvokeMember(@"Item",
                               System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance,
                               null, ComInstance, new object[] {i + 1})
                           ?? throw new InvalidOperationException("Failed to retrieve item from the collection"));

        internal MaxImDlDocCollection(object comInstance) : base (comInstance.GetType())
            => ComInstance = comInstance;

        Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<MaxImDlDoc> IEnumerable<MaxImDlDoc>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}