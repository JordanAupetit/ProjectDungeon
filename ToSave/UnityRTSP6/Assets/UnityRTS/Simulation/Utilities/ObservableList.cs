using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {

    public enum ChangedFlags { Add = 0x01, Remove = 0x02, Replace = 0x03, Clear = 0x12, };

    public class ListEvent<T> {
        public ChangedFlags Flags;
        public bool IsWholeList { get { return Flags.HasFlag(0x10); } }
        public bool IsSingleItem { get { return !IsWholeList; } }
        public int Index;
        public T Item;
    }

    public class ObservableList<T> : IList<T> {

        private List<T> list = new List<T>();

        public event Action<ObservableList<T>, T> OnAdding;
        public event Action<ObservableList<T>, T> OnRemoving;
        public event Action<ObservableList<T>> OnClearing;

        public event Action<ObservableList<T>, ListEvent<T>> OnChanged;


        public int Count { get { return list.Count; } }
        public bool IsReadOnly { get { return false; } }

        public int IndexOf(T item) { return list.IndexOf(item); }

        public void Add(T item) { Insert(list.Count, item); }
        public void Insert(int index, T item) {
            if (OnAdding != null) OnAdding(this, item);
            list.Insert(index, item);
            NotifyChanged(ChangedFlags.Add, index, item);
        }

        public bool Remove(T item) { var index = list.IndexOf(item); if (index >= 0) { RemoveAt(index); return true; } return false; }
        public void RemoveAt(int index) {
            var item = list[index];
            if (OnRemoving != null) OnRemoving(this, item);
            list.RemoveAt(index);
            NotifyChanged(ChangedFlags.Remove, index, item);
        }

        public void Clear() {
            if (OnClearing != null) OnClearing(this);
            //if (OnRemoved != null) for (int i = 0; i < list.Count; ++i) OnRemoved(this, list[i]);
            list.Clear();
            NotifyChanged(ChangedFlags.Clear, -1, default(T));
        }

        public T this[int index] {
            get { return list[index]; }
            set { throw new NotImplementedException(); }
        }

        public bool Contains(T item) { return list.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { list.CopyTo(array, arrayIndex); }
        public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return list.GetEnumerator(); }


        private void NotifyChanged(ChangedFlags flags, int index, T item) {
            if (OnChanged != null) {
                OnChanged(this, new ListEvent<T>() {
                    Flags = flags,
                    Index = index,
                    Item = item,
                });
            }
        }
    }

}
