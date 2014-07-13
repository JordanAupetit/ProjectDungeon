using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {
    public class AggregateList<T, S, I> : ObservableList<T> {

        public readonly ObservableList<S> Source;
        public readonly Func<S, ObservableList<I>> Convert;
        public readonly Func<I, T> ConvertItem;
        private Dictionary<S, ObservableList<I>> mappings = new Dictionary<S, ObservableList<I>>();
        private Dictionary<I, T> itemMappings = new Dictionary<I, T>();

        public AggregateList(ObservableList<S> source, Func<S, ObservableList<I>> convert, Func<I, T> convertItem) {
            Source = source;
            Convert = convert;
            ConvertItem = convertItem;
            Source.OnAdding += Source_OnAdded;
            Source.OnRemoving += Source_OnRemovd;
            Source.OnClearing += Source_OnClearing;
        }

        private void Source_OnAdded(ObservableList<S> source, S item) {
            var titem = Convert(item);
            if (titem != null) {
                mappings.Add(item, titem);
                AddCollection(titem);
            }
        }

        private void Source_OnRemovd(ObservableList<S> source, S item) {
            ObservableList<I> titem;
            if (mappings.TryGetValue(item, out titem)) {
                mappings.Remove(item);
                RemoveCollection(titem);
            }
        }

        private void Source_OnClearing(ObservableList<S> source) {
            mappings.Clear();
            Clear();
            itemMappings.Clear();
        }

        private void AddCollection(ObservableList<I> collection) {
            for (int i = 0; i < collection.Count; ++i) Item_OnAdding(collection, collection[i]);
            collection.OnAdding += Item_OnAdding;
            collection.OnRemoving += Item_OnRemoving;
            collection.OnClearing += Item_OnClearing;
        }
        private void RemoveCollection(ObservableList<I> collection) {
            collection.OnAdding -= Item_OnAdding;
            collection.OnRemoving -= Item_OnRemoving;
            collection.OnClearing -= Item_OnClearing;
            for (int i = 0; i < collection.Count; ++i) Item_OnRemoving(collection, collection[i]);
        }

        private void Item_OnAdding(ObservableList<I> source, I item) {
            var titem = ConvertItem(item);
            if (titem != null) {
                itemMappings.Add(item, titem);
                Add(titem);
            }
        }
        private void Item_OnRemoving(ObservableList<I> source, I item) {
            T titem;
            if (itemMappings.TryGetValue(item, out titem)) {
                itemMappings.Remove(item);
                Remove(titem);
            }
        }
        private void Item_OnClearing(ObservableList<I> source) { for (int i = 0; i < source.Count; ++i) Item_OnRemoving(source, source[i]); }

    }
}
