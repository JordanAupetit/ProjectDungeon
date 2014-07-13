using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {
    public class ForwardingList<T, S> : ObservableList<T> {

        public readonly ObservableList<S> Source;
        public readonly Func<S, T> Convert;
        private Dictionary<S, T> mappings = new Dictionary<S, T>();

        public ForwardingList(ObservableList<S> source, Func<S, T> convert) {
            Source = source;
            Convert = convert;
            Source.OnAdding += Source_OnAdded;
            Source.OnRemoving += Source_OnRemovd;
            Source.OnClearing += Source_OnClearing;
        }

        private void Source_OnAdded(ObservableList<S> source, S item) {
            var titem = Convert(item);
            if (titem != null) {
                mappings.Add(item, titem);
                Add(titem);
            }
        }

        private void Source_OnRemovd(ObservableList<S> source, S item) {
            T titem;
            if (mappings.TryGetValue(item, out titem)) {
                mappings.Remove(item);
                Remove(titem);
            }
        }

        private void Source_OnClearing(ObservableList<S> source) {
            mappings.Clear();
            Clear();
        }

    }
}
