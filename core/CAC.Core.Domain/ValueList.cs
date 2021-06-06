using System.Collections.Generic;
using System.Linq;

// we want to keep the class and extensions together
#pragma warning disable SA1402

// the code formatter separates the methods
#pragma warning disable SA1202
#pragma warning disable S4136

// the `Empty` field should be a static singleton
#pragma warning disable S3887
#pragma warning disable S2386

// ReSharper disable once CheckNamespace (we want this to be in the same namespace as the original `ImmutableList`)
namespace System.Collections.Immutable
{
    public sealed class ValueList<T> : IList<T>, IList, IEquatable<ValueList<T>>
        where T : notnull
    {
        public static readonly ValueList<T> Empty = new ValueList<T>(ImmutableList<T>.Empty);

        private readonly ImmutableList<T> wrappedList;

        internal ValueList(ImmutableList<T> wrappedList) => this.wrappedList = wrappedList;

        public T this[int index] => wrappedList[index];

        public bool Equals(ValueList<T>? other) => this.SequenceEqual(other ?? Empty);

        public int Count => wrappedList.Count;

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => wrappedList.IsEmpty ? Enumerable.Empty<T>().GetEnumerator() : wrappedList.GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => wrappedList.GetEnumerator();

        #endregion

        public override bool Equals(object? obj) => Equals(obj as ValueList<T>);

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable once ArrangeRedundantParentheses
                return this.Aggregate(19, (h, i) => (h * 19) + i.GetHashCode());
            }
        }

        public ValueList<T> Add(T value) => wrappedList.Add(value).ToValueList();
        public ValueList<T> AddRange(IEnumerable<T> items) => wrappedList.AddRange(items).ToValueList();
        public ValueList<T> Clear() => wrappedList.Clear().ToValueList();
        public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer = null) => wrappedList.IndexOf(item, index, count, equalityComparer);
        public ValueList<T> Insert(int index, T element) => wrappedList.Insert(index, element).ToValueList();
        public ValueList<T> InsertRange(int index, IEnumerable<T> items) => wrappedList.InsertRange(index, items).ToValueList();
        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer = null) => wrappedList.LastIndexOf(item, index, count, equalityComparer);
        public ValueList<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null) => wrappedList.Remove(value, equalityComparer).ToValueList();
        public ValueList<T> RemoveAll(Predicate<T> match) => wrappedList.RemoveAll(match).ToValueList();
        public ValueList<T> RemoveAt(int index) => wrappedList.RemoveAt(index).ToValueList();
        public ValueList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer = null) => wrappedList.RemoveRange(items, equalityComparer).ToValueList();
        public ValueList<T> RemoveRange(int index, int count) => wrappedList.RemoveRange(index, count).ToValueList();
        public ValueList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer = null) => wrappedList.Replace(oldValue, newValue, equalityComparer).ToValueList();
        public ValueList<T> SetItem(int index, T value) => wrappedList.SetItem(index, value).ToValueList();

        private static bool IsCompatibleObject(object? value)
        {
            // ReSharper disable once ArrangeRedundantParentheses
            return value is T || (value == null && default(T) == null);
        }

        #region ICollection Methods

        void ICollection.CopyTo(Array array, int index) => ((ICollection)wrappedList).CopyTo(array, index);
        bool ICollection.IsSynchronized => true;
        object ICollection.SyncRoot => wrappedList;

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add(T item) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();
        bool ICollection<T>.IsReadOnly => true;
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        bool ICollection<T>.Contains(T item) => wrappedList.Contains(item);
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => wrappedList.CopyTo(array, arrayIndex);

        #endregion

        #region IList members

        int IList.Add(object? value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        bool IList.Contains(object? value) => IsCompatibleObject(value) && wrappedList.Contains((T)value!);
        int IList.IndexOf(object? value) => IsCompatibleObject(value) ? wrappedList.IndexOf((T)value!) : -1;
        void IList.Insert(int index, object? value) => throw new NotSupportedException();
        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        void IList.Remove(object? value) => throw new NotSupportedException();

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        #endregion

        #region IList<T> Members

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
        int IList<T>.IndexOf(T item) => wrappedList.IndexOf(item);

        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        #endregion

        public override string ToString() => $"[{string.Join(", ", wrappedList.Select(i => i.ToString()))}]";

        public static bool operator ==(ValueList<T> lhs, ValueList<T> rhs) => lhs.Equals(rhs);

        public static bool operator !=(ValueList<T> lhs, ValueList<T> rhs) => !(lhs == rhs);
    }

    public static class Ex
    {
        public static ValueList<T> ToValueList<T>(this IEnumerable<T> list)
            where T : notnull => new ValueList<T>(list.ToImmutableList());
    }
}
